/* =====================================================================
   GroceryPOS — سكربت قاعدة بيانات SQL Server (وثيقة التصميم، الجزء 2)
   المرجع: System-Overview.md + قرارات تصميم قاعدة البيانات في هذا السكربت
   المتطلب: SQL Server 2019 أو أحدث (يعمل على Express)

   طريقة التشغيل: على خادم جديد، شغّل السكربت كاملًا مرة واحدة
   (SSMS: افتح الملف ثم Execute). إن كانت الجداول موجودة يتوقف تلقائيًا.

   ملاحظات تصميمية مهمة:
   1) الأرصدة: العميل  Balance = Σ(Debit − Credit)  (موجب = مدين لنا،
      وسالب = رصيد دائن للعميل حسب D-04).
      المورد  Balance = Σ(Credit − Debit)  (موجب = علينا للمورد).
   2) قيم التكلفة والأسعار (AvgCost, Batches.UnitCost, أسعار ProductUnits,
      أعمدة *Base) بالعملة الأساسية. أما مبالغ الفواتير والسندات فبعملة المستند.
   3) كل الكميات المخزنة بوحدة المنتج الأساسية (QtyBase) ما لم يُذكر غير ذلك.
   4) تعديل المخزون يجب أن يتم بجملة ذرية (RCSI لا يمنع سباق القراءة):
        UPDATE dbo.Batches SET QtyRemaining = QtyRemaining - @q
         WHERE Id = @id AND QtyRemaining >= @q;   -- ثم افحص @@ROWCOUNT
      وقيد CK_Batches_Qty يمنع السالب كخط دفاع أخير (BR-03).
   5) ترقيم المستندات: استدعِ dbo.usp_GetNextDocumentNumber كآخر خطوة داخل
      الـ Transaction ليقصر زمن القفل ولا تتكرر الأرقام ولا تنشأ فجوات.
   6) AuditLogs تستخدم UserId بدل CreatedBy (قد لا يوجد مستخدم عند فشل الدخول).
   7) تعليق الفاتورة (FR-POS-06) يُنفَّذ في ذاكرة التطبيق لكل جهاز، فلا جدول له هنا.
   8) CreditLimit بالعملة الأساسية، والقيمة 0 تعني لا بيع آجل إلا بإذن.
   9) تواريخ النظام بالتوقيت المحلي للخادم (فرع واحد): SYSDATETIME().
   ===================================================================== */

SET NOCOUNT ON;
GO

USE master;
GO

IF DB_ID(N'GroceryPOS') IS NULL
    CREATE DATABASE GroceryPOS COLLATE Arabic_100_CI_AI;   -- ترتيب عربي، غير حساس للحالة
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'GroceryPOS' AND is_read_committed_snapshot_on = 0)
    ALTER DATABASE GroceryPOS SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO

USE GroceryPOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- حماية: لا نعيد الإنشاء فوق قاعدة موجودة
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
BEGIN
    RAISERROR(N'الجداول موجودة مسبقًا. لن يُنفَّذ السكربت. احذف القاعدة إن أردت البدء من الصفر.', 16, 1);
    SET NOEXEC ON;
END
GO

/* =====================================================================
   1) الأمان والإعدادات والعملات
   ===================================================================== */

CREATE TABLE dbo.Users (
    Id                 int IDENTITY(1,1) NOT NULL,
    Username           nvarchar(50)   NOT NULL,
    FullName           nvarchar(150)  NOT NULL,
    PasswordHash       varbinary(128) NOT NULL,
    PasswordSalt       varbinary(64)  NOT NULL,
    ManagerPinHash     varbinary(128) NULL,          -- رمز المدير للموافقات (FR-AUTH-06)
    ManagerPinSalt     varbinary(64)  NULL,
    MustChangePassword bit NOT NULL DEFAULT (0),
    IsActive           bit NOT NULL DEFAULT (1),
    FailedAttempts     int NOT NULL DEFAULT (0),
    LockedUntil        datetime2(3) NULL,
    LastLoginAt        datetime2(3) NULL,
    CreatedAt          datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy          int NULL,
    UpdatedAt          datetime2(3) NULL,
    UpdatedBy          int NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username)
);
GO

CREATE TABLE dbo.Roles (
    Id          int IDENTITY(1,1) NOT NULL,
    Name        nvarchar(100) NOT NULL,
    Description nvarchar(300) NULL,
    IsSystem    bit NOT NULL DEFAULT (0),            -- أدوار النظام الأساسية
    IsActive    bit NOT NULL DEFAULT (1),
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    UpdatedAt   datetime2(3) NULL,
    UpdatedBy   int NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Permissions (
    Id        int IDENTITY(1,1) NOT NULL,
    Code      nvarchar(100) NOT NULL,
    Name      nvarchar(150) NOT NULL,                -- الاسم المعروض بالعربية
    Module    nvarchar(50)  NOT NULL,
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY (Id),
    CONSTRAINT UQ_Permissions_Code UNIQUE (Code)
);
GO

CREATE TABLE dbo.UserRoles (
    UserId    int NOT NULL,
    RoleId    int NOT NULL,
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
);
GO

CREATE TABLE dbo.RolePermissions (
    RoleId       int NOT NULL,
    PermissionId int NOT NULL,
    CreatedAt    datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy    int NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id)
);
GO

CREATE TABLE dbo.Currencies (
    Id            int IDENTITY(1,1) NOT NULL,
    Code          nvarchar(10)  NOT NULL,
    Name          nvarchar(100) NOT NULL,
    Symbol        nvarchar(10)  NULL,
    DecimalPlaces int NOT NULL,
    RoundingMode  nvarchar(30) NOT NULL DEFAULT (N'AwayFromZero'),   -- أسماء MidpointRounding في .NET
    IsBase        bit NOT NULL DEFAULT (0),
    IsActive      bit NOT NULL DEFAULT (1),
    CreatedAt     datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy     int NOT NULL,
    UpdatedAt     datetime2(3) NULL,
    UpdatedBy     int NULL,
    CONSTRAINT PK_Currencies PRIMARY KEY (Id),
    CONSTRAINT UQ_Currencies_Code UNIQUE (Code),
    CONSTRAINT CK_Currencies_Decimals CHECK (DecimalPlaces BETWEEN 0 AND 6),
    CONSTRAINT CK_Currencies_Rounding CHECK (RoundingMode IN
        (N'AwayFromZero', N'ToEven', N'ToZero', N'ToPositiveInfinity', N'ToNegativeInfinity'))
);
GO
-- عملة أساسية واحدة فقط
CREATE UNIQUE INDEX UX_Currencies_OneBase ON dbo.Currencies (IsBase) WHERE IsBase = 1;
GO

CREATE TABLE dbo.ExchangeRates (
    Id            int IDENTITY(1,1) NOT NULL,
    CurrencyId    int NOT NULL,
    RateToBase    decimal(18,6) NOT NULL,            -- كم وحدة أساسية تساوي وحدة هذه العملة
    EffectiveFrom datetime2(3) NOT NULL,
    Notes         nvarchar(300) NULL,
    CreatedAt     datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy     int NOT NULL,
    CONSTRAINT PK_ExchangeRates PRIMARY KEY (Id),
    CONSTRAINT FK_ExchangeRates_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT CK_ExchangeRates_Rate CHECK (RateToBase > 0),
    CONSTRAINT UQ_ExchangeRates_Currency_Effective UNIQUE (CurrencyId, EffectiveFrom)
);
GO
CREATE INDEX IX_ExchangeRates_Latest ON dbo.ExchangeRates (CurrencyId, EffectiveFrom DESC) INCLUDE (RateToBase);
GO

CREATE TABLE dbo.Settings (
    [Key]       nvarchar(100)  NOT NULL,
    [Value]     nvarchar(1000) NOT NULL,
    Description nvarchar(300)  NULL,
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    UpdatedAt   datetime2(3) NULL,
    UpdatedBy   int NULL,
    CONSTRAINT PK_Settings PRIMARY KEY ([Key])
);
GO

CREATE TABLE dbo.DocumentSequences (
    Name        nvarchar(50) NOT NULL,
    Prefix      nvarchar(10) NULL,
    PadLength   int NOT NULL DEFAULT (6),
    LastNumber  bigint NOT NULL DEFAULT (0),
    RowVersion  rowversion NOT NULL,
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    CONSTRAINT PK_DocumentSequences PRIMARY KEY (Name),
    CONSTRAINT CK_DocumentSequences_Last CHECK (LastNumber >= 0 AND PadLength BETWEEN 1 AND 18)
);
GO

CREATE TABLE dbo.AuditLogs (
    Id         bigint IDENTITY(1,1) NOT NULL,
    UserId     int NULL,
    Action     nvarchar(100) NOT NULL,
    EntityName nvarchar(100) NULL,
    EntityId   nvarchar(100) NULL,
    OldValues  nvarchar(max) NULL,
    NewValues  nvarchar(max) NULL,
    DeviceName nvarchar(100) NULL,
    CreatedAt  datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO
CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs (EntityName, EntityId);
CREATE INDEX IX_AuditLogs_User_Date ON dbo.AuditLogs (UserId, CreatedAt);
CREATE INDEX IX_AuditLogs_Date ON dbo.AuditLogs (CreatedAt);
GO

/* =====================================================================
   2) الكتالوج
   ===================================================================== */

CREATE TABLE dbo.Categories (
    Id        int IDENTITY(1,1) NOT NULL,
    ParentId  int NULL,
    Name      nvarchar(150) NOT NULL,
    IsActive  bit NOT NULL DEFAULT (1),
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    UpdatedAt datetime2(3) NULL,
    UpdatedBy int NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (Id),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES dbo.Categories(Id),
    CONSTRAINT CK_Categories_NotSelf CHECK (ParentId IS NULL OR ParentId <> Id),
    CONSTRAINT UQ_Categories_Parent_Name UNIQUE (ParentId, Name)
);
GO

CREATE TABLE dbo.Units (
    Id        int IDENTITY(1,1) NOT NULL,
    Name      nvarchar(50) NOT NULL,
    IsActive  bit NOT NULL DEFAULT (1),
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    UpdatedAt datetime2(3) NULL,
    UpdatedBy int NULL,
    CONSTRAINT PK_Units PRIMARY KEY (Id),
    CONSTRAINT UQ_Units_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Products (
    Id          int IDENTITY(1,1) NOT NULL,
    Sku         nvarchar(50)  NULL,
    Name        nvarchar(200) NOT NULL,
    CategoryId  int NOT NULL,
    BaseUnitId  int NOT NULL,
    ImagePath   nvarchar(300) NULL,
    TracksExpiry bit NOT NULL DEFAULT (0),
    MinStock    decimal(18,3) NOT NULL DEFAULT (0),
    AvgCost     decimal(18,4) NOT NULL DEFAULT (0),   -- متوسط التكلفة المرجح بالعملة الأساسية (D-03)
    IsActive    bit NOT NULL DEFAULT (1),
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    UpdatedAt   datetime2(3) NULL,
    UpdatedBy   int NULL,
    CONSTRAINT PK_Products PRIMARY KEY (Id),
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT FK_Products_BaseUnit FOREIGN KEY (BaseUnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_Products_Values CHECK (MinStock >= 0 AND AvgCost >= 0)
);
GO
CREATE UNIQUE INDEX UX_Products_Sku ON dbo.Products (Sku) WHERE Sku IS NOT NULL;
CREATE INDEX IX_Products_Name ON dbo.Products (Name);
CREATE INDEX IX_Products_Category ON dbo.Products (CategoryId);
GO

CREATE TABLE dbo.ProductUnits (
    Id               int IDENTITY(1,1) NOT NULL,
    ProductId        int NOT NULL,
    UnitId           int NOT NULL,
    FactorToBase     decimal(18,6) NOT NULL,           -- الوحدة الأساسية لها معامل 1 (يضمنه التطبيق)
    Barcode          nvarchar(50) NULL,
    RetailPrice      decimal(18,4) NOT NULL DEFAULT (0),    -- بالعملة الأساسية
    WholesalePrice   decimal(18,4) NULL,
    IsDefaultSaleUnit bit NOT NULL DEFAULT (0),
    IsActive         bit NOT NULL DEFAULT (1),
    CreatedAt        datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy        int NOT NULL,
    UpdatedAt        datetime2(3) NULL,
    UpdatedBy        int NULL,
    CONSTRAINT PK_ProductUnits PRIMARY KEY (Id),
    CONSTRAINT FK_ProductUnits_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_ProductUnits_Unit FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT UQ_ProductUnits_Product_Unit UNIQUE (ProductId, UnitId),
    CONSTRAINT CK_ProductUnits_Values CHECK (FactorToBase > 0 AND RetailPrice >= 0
                                             AND (WholesalePrice IS NULL OR WholesalePrice >= 0))
);
GO
-- البحث السريع بالباركود (NFR-01)
CREATE UNIQUE INDEX UX_ProductUnits_Barcode ON dbo.ProductUnits (Barcode)
    INCLUDE (ProductId, UnitId, FactorToBase, RetailPrice) WHERE Barcode IS NOT NULL;
GO

/* =====================================================================
   3) الموردون والعملاء
   ===================================================================== */

CREATE TABLE dbo.Suppliers (
    Id        int IDENTITY(1,1) NOT NULL,
    Name      nvarchar(200) NOT NULL,
    Phone     nvarchar(30)  NULL,
    Address   nvarchar(300) NULL,
    Notes     nvarchar(500) NULL,
    IsActive  bit NOT NULL DEFAULT (1),
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    UpdatedAt datetime2(3) NULL,
    UpdatedBy int NULL,
    CONSTRAINT PK_Suppliers PRIMARY KEY (Id)
);
GO
CREATE INDEX IX_Suppliers_Name ON dbo.Suppliers (Name);
GO

CREATE TABLE dbo.Customers (
    Id          int IDENTITY(1,1) NOT NULL,
    Name        nvarchar(200) NOT NULL,
    Phone       nvarchar(30)  NULL,
    Address     nvarchar(300) NULL,
    Notes       nvarchar(500) NULL,
    CreditLimit decimal(18,4) NOT NULL DEFAULT (0),    -- بالعملة الأساسية؛ 0 = لا بيع آجل إلا بإذن
    IsActive    bit NOT NULL DEFAULT (1),
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    UpdatedAt   datetime2(3) NULL,
    UpdatedBy   int NULL,
    CONSTRAINT PK_Customers PRIMARY KEY (Id),
    CONSTRAINT CK_Customers_Limit CHECK (CreditLimit >= 0)
);
GO
CREATE INDEX IX_Customers_Name ON dbo.Customers (Name);
CREATE INDEX IX_Customers_Phone ON dbo.Customers (Phone);
GO

/* =====================================================================
   4) المشتريات والمخزون
   ===================================================================== */

CREATE TABLE dbo.PurchaseOrders (
    Id         int IDENTITY(1,1) NOT NULL,
    OrderNo    nvarchar(30) NOT NULL,
    SupplierId int NOT NULL,
    CurrencyId int NOT NULL,
    Status     nvarchar(20) NOT NULL DEFAULT (N'Draft'),
    OrderDate  date NOT NULL,
    Notes      nvarchar(500) NULL,
    ApprovedBy int NULL,
    ApprovedAt datetime2(3) NULL,
    CreatedAt  datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy  int NOT NULL,
    UpdatedAt  datetime2(3) NULL,
    UpdatedBy  int NULL,
    CONSTRAINT PK_PurchaseOrders PRIMARY KEY (Id),
    CONSTRAINT UQ_PurchaseOrders_No UNIQUE (OrderNo),
    CONSTRAINT FK_PurchaseOrders_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_PurchaseOrders_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_PurchaseOrders_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_PurchaseOrders_Status CHECK (Status IN
        (N'Draft', N'Approved', N'PartiallyReceived', N'Received', N'Cancelled'))
);
GO
CREATE INDEX IX_PurchaseOrders_Supplier ON dbo.PurchaseOrders (SupplierId, OrderDate);
GO

CREATE TABLE dbo.PurchaseOrderLines (
    Id            bigint IDENTITY(1,1) NOT NULL,
    OrderId       int NOT NULL,
    ProductUnitId int NOT NULL,
    Qty           decimal(18,3) NOT NULL,
    UnitPrice     decimal(18,4) NOT NULL,
    ReceivedQty   decimal(18,3) NOT NULL DEFAULT (0),
    CreatedAt     datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy     int NOT NULL,
    UpdatedAt     datetime2(3) NULL,
    UpdatedBy     int NULL,
    CONSTRAINT PK_PurchaseOrderLines PRIMARY KEY (Id),
    CONSTRAINT FK_PurchaseOrderLines_Order FOREIGN KEY (OrderId) REFERENCES dbo.PurchaseOrders(Id),
    CONSTRAINT FK_PurchaseOrderLines_Unit FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_PurchaseOrderLines_Values CHECK (Qty > 0 AND UnitPrice >= 0 AND ReceivedQty >= 0)
);
GO
CREATE INDEX IX_PurchaseOrderLines_Order ON dbo.PurchaseOrderLines (OrderId);
GO

CREATE TABLE dbo.PurchaseInvoices (
    Id              bigint IDENTITY(1,1) NOT NULL,
    InvoiceNo       nvarchar(30) NOT NULL,
    SupplierInvoiceNo nvarchar(50) NULL,              -- رقم فاتورة المورد الورقية
    SupplierId      int NOT NULL,
    OrderId         int NULL,
    CurrencyId      int NOT NULL,
    ExchangeRateId  int NULL,
    RateUsed        decimal(18,6) NOT NULL DEFAULT (1),
    InvoiceDate     date NOT NULL,
    Total           decimal(18,4) NOT NULL,           -- بعملة الفاتورة
    TotalBase       decimal(18,4) NOT NULL,           -- المكافئ بالعملة الأساسية وقت الإنشاء
    PaidAmount      decimal(18,4) NOT NULL DEFAULT (0),   -- المتبقي (Total − PaidAmount) دين للمورد
    Status          nvarchar(10) NOT NULL DEFAULT (N'Posted'),
    Notes           nvarchar(500) NULL,
    IdempotencyKey  nvarchar(64) NOT NULL,
    RowVersion      rowversion NOT NULL,
    CreatedAt       datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy       int NOT NULL,
    UpdatedAt       datetime2(3) NULL,
    UpdatedBy       int NULL,
    CONSTRAINT PK_PurchaseInvoices PRIMARY KEY (Id),
    CONSTRAINT UQ_PurchaseInvoices_No UNIQUE (InvoiceNo),
    CONSTRAINT UQ_PurchaseInvoices_Idem UNIQUE (IdempotencyKey),
    CONSTRAINT FK_PurchaseInvoices_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_PurchaseInvoices_Order FOREIGN KEY (OrderId) REFERENCES dbo.PurchaseOrders(Id),
    CONSTRAINT FK_PurchaseInvoices_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_PurchaseInvoices_Rate FOREIGN KEY (ExchangeRateId) REFERENCES dbo.ExchangeRates(Id),
    CONSTRAINT CK_PurchaseInvoices_Values CHECK (RateUsed > 0 AND Total >= 0 AND TotalBase >= 0
                                                 AND PaidAmount >= 0 AND PaidAmount <= Total),
    CONSTRAINT CK_PurchaseInvoices_Status CHECK (Status IN (N'Posted', N'Voided'))
);
GO
CREATE INDEX IX_PurchaseInvoices_Supplier ON dbo.PurchaseInvoices (SupplierId, InvoiceDate);
CREATE INDEX IX_PurchaseInvoices_Date ON dbo.PurchaseInvoices (InvoiceDate);
GO

CREATE TABLE dbo.PurchaseInvoiceLines (
    Id            bigint IDENTITY(1,1) NOT NULL,
    InvoiceId     bigint NOT NULL,
    ProductUnitId int NOT NULL,
    Qty           decimal(18,3) NOT NULL,             -- بوحدة الشراء
    QtyBase       decimal(18,3) NOT NULL,             -- بالوحدة الأساسية
    UnitPrice     decimal(18,4) NOT NULL,             -- بعملة الفاتورة، لوحدة الشراء
    UnitCostBase  decimal(18,4) NOT NULL,             -- بالعملة الأساسية، للوحدة الأساسية
    LineTotal     decimal(18,4) NOT NULL,
    BatchNo       nvarchar(50) NULL,
    ExpiryDate    date NULL,
    CreatedAt     datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy     int NOT NULL,
    CONSTRAINT PK_PurchaseInvoiceLines PRIMARY KEY (Id),
    CONSTRAINT FK_PurchaseInvoiceLines_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.PurchaseInvoices(Id),
    CONSTRAINT FK_PurchaseInvoiceLines_Unit FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_PurchaseInvoiceLines_Values CHECK (Qty > 0 AND QtyBase > 0 AND UnitPrice >= 0
                                                     AND UnitCostBase >= 0 AND LineTotal >= 0)
);
GO
CREATE INDEX IX_PurchaseInvoiceLines_Invoice ON dbo.PurchaseInvoiceLines (InvoiceId);
CREATE INDEX IX_PurchaseInvoiceLines_Unit ON dbo.PurchaseInvoiceLines (ProductUnitId);
GO

CREATE TABLE dbo.Batches (
    Id                    bigint IDENTITY(1,1) NOT NULL,
    ProductId             int NOT NULL,
    PurchaseInvoiceLineId bigint NULL,                -- NULL للرصيد الافتتاحي
    BatchNo               nvarchar(50) NULL,
    ExpiryDate            date NULL,                  -- NULL لما لا يتتبع الصلاحية
    QtyInitial            decimal(18,3) NOT NULL,
    QtyRemaining          decimal(18,3) NOT NULL,
    UnitCost              decimal(18,4) NOT NULL,     -- تكلفة الدفعة بالعملة الأساسية للوحدة الأساسية
    RowVersion            rowversion NOT NULL,
    CreatedAt             datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy             int NOT NULL,
    UpdatedAt             datetime2(3) NULL,
    UpdatedBy             int NULL,
    CONSTRAINT PK_Batches PRIMARY KEY (Id),
    CONSTRAINT FK_Batches_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_Batches_PurchaseLine FOREIGN KEY (PurchaseInvoiceLineId) REFERENCES dbo.PurchaseInvoiceLines(Id),
    CONSTRAINT CK_Batches_Qty CHECK (QtyInitial >= 0 AND QtyRemaining >= 0),   -- لا مخزون سالب (BR-03)
    CONSTRAINT CK_Batches_Cost CHECK (UnitCost >= 0)
);
GO
-- فهرس FEFO: في الاستعلام ضع الشرط QtyRemaining > 0 ورتّب ExpiryDate مع جعل NULL في الآخر
CREATE INDEX IX_Batches_FEFO ON dbo.Batches (ProductId, ExpiryDate)
    INCLUDE (QtyRemaining, UnitCost) WHERE QtyRemaining > 0;
CREATE UNIQUE INDEX UX_Batches_PurchaseLine ON dbo.Batches (PurchaseInvoiceLineId) WHERE PurchaseInvoiceLineId IS NOT NULL;
CREATE INDEX IX_Batches_Expiry ON dbo.Batches (ExpiryDate) WHERE QtyRemaining > 0 AND ExpiryDate IS NOT NULL;
GO

CREATE TABLE dbo.StockMovements (
    Id           bigint IDENTITY(1,1) NOT NULL,
    ProductId    int NOT NULL,
    BatchId      bigint NOT NULL,
    MovementType nvarchar(20) NOT NULL,
    Qty          decimal(18,3) NOT NULL,              -- بالوحدة الأساسية، موجب = دخول وسالب = خروج
    UnitCost     decimal(18,4) NOT NULL,              -- وقت الحركة (للبيع: AvgCost لحظة البيع)
    RefType      nvarchar(30) NOT NULL,
    RefId        bigint NOT NULL,
    Notes        nvarchar(300) NULL,
    CreatedAt    datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy    int NOT NULL,
    CONSTRAINT PK_StockMovements PRIMARY KEY (Id),
    CONSTRAINT FK_StockMovements_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_StockMovements_Batch FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT CK_StockMovements_Qty CHECK (Qty <> 0 AND UnitCost >= 0),
    CONSTRAINT CK_StockMovements_Type CHECK (MovementType IN
        (N'Opening', N'Purchase', N'PurchaseReturn', N'Sale', N'SaleReturn',
         N'SaleVoid', N'Adjustment', N'CountDifference'))
);
GO
CREATE INDEX IX_StockMovements_Product_Date ON dbo.StockMovements (ProductId, CreatedAt) INCLUDE (Qty, MovementType);
CREATE INDEX IX_StockMovements_Batch ON dbo.StockMovements (BatchId);
CREATE INDEX IX_StockMovements_Ref ON dbo.StockMovements (RefType, RefId);
GO

CREATE TABLE dbo.StockCounts (
    Id          int IDENTITY(1,1) NOT NULL,
    CountNo     nvarchar(30) NOT NULL,
    CountDate   date NOT NULL,
    Status      nvarchar(15) NOT NULL DEFAULT (N'Draft'),
    Notes       nvarchar(500) NULL,
    CompletedAt datetime2(3) NULL,
    CompletedBy int NULL,
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    UpdatedAt   datetime2(3) NULL,
    UpdatedBy   int NULL,
    CONSTRAINT PK_StockCounts PRIMARY KEY (Id),
    CONSTRAINT UQ_StockCounts_No UNIQUE (CountNo),
    CONSTRAINT FK_StockCounts_CompletedBy FOREIGN KEY (CompletedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_StockCounts_Status CHECK (Status IN (N'Draft', N'Completed', N'Cancelled'))
);
GO

CREATE TABLE dbo.StockCountLines (
    Id           bigint IDENTITY(1,1) NOT NULL,
    StockCountId int NOT NULL,
    BatchId      bigint NOT NULL,
    SystemQty    decimal(18,3) NOT NULL,
    ActualQty    decimal(18,3) NULL,
    Difference   AS (ActualQty - SystemQty) PERSISTED,
    CreatedAt    datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy    int NOT NULL,
    UpdatedAt    datetime2(3) NULL,
    UpdatedBy    int NULL,
    CONSTRAINT PK_StockCountLines PRIMARY KEY (Id),
    CONSTRAINT FK_StockCountLines_Count FOREIGN KEY (StockCountId) REFERENCES dbo.StockCounts(Id),
    CONSTRAINT FK_StockCountLines_Batch FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT UQ_StockCountLines_Count_Batch UNIQUE (StockCountId, BatchId),
    CONSTRAINT CK_StockCountLines_Qty CHECK (SystemQty >= 0 AND (ActualQty IS NULL OR ActualQty >= 0))
);
GO

CREATE TABLE dbo.StockAdjustments (
    Id           int IDENTITY(1,1) NOT NULL,
    AdjustmentNo nvarchar(30) NOT NULL,
    AdjustDate   date NOT NULL,
    ReasonType   nvarchar(20) NOT NULL,
    Reason       nvarchar(300) NOT NULL,              -- السبب إلزامي (FR-INV-06)
    StockCountId int NULL,                            -- إن نشأت التسوية من جرد
    CreatedAt    datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy    int NOT NULL,
    UpdatedAt    datetime2(3) NULL,
    UpdatedBy    int NULL,
    CONSTRAINT PK_StockAdjustments PRIMARY KEY (Id),
    CONSTRAINT UQ_StockAdjustments_No UNIQUE (AdjustmentNo),
    CONSTRAINT FK_StockAdjustments_Count FOREIGN KEY (StockCountId) REFERENCES dbo.StockCounts(Id),
    CONSTRAINT CK_StockAdjustments_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0),
    CONSTRAINT CK_StockAdjustments_Type CHECK (ReasonType IN
        (N'Damaged', N'Expired', N'Lost', N'Found', N'CountDifference', N'Other'))
);
GO

CREATE TABLE dbo.StockAdjustmentLines (
    Id           bigint IDENTITY(1,1) NOT NULL,
    AdjustmentId int NOT NULL,
    BatchId      bigint NOT NULL,
    Qty          decimal(18,3) NOT NULL,              -- بالوحدة الأساسية، سالب = نقص
    UnitCost     decimal(18,4) NOT NULL,
    CreatedAt    datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy    int NOT NULL,
    CONSTRAINT PK_StockAdjustmentLines PRIMARY KEY (Id),
    CONSTRAINT FK_StockAdjustmentLines_Adj FOREIGN KEY (AdjustmentId) REFERENCES dbo.StockAdjustments(Id),
    CONSTRAINT FK_StockAdjustmentLines_Batch FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT CK_StockAdjustmentLines_Qty CHECK (Qty <> 0 AND UnitCost >= 0)
);
GO

CREATE TABLE dbo.SupplierBalances (
    SupplierId int NOT NULL,
    CurrencyId int NOT NULL,
    Balance    decimal(18,4) NOT NULL DEFAULT (0),
    RowVersion rowversion NOT NULL,
    CreatedAt  datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy  int NOT NULL,
    UpdatedAt  datetime2(3) NULL,
    UpdatedBy  int NULL,
    CONSTRAINT PK_SupplierBalances PRIMARY KEY (SupplierId, CurrencyId),
    CONSTRAINT FK_SupplierBalances_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SupplierBalances_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id)
);
GO

CREATE TABLE dbo.SupplierLedger (
    Id          bigint IDENTITY(1,1) NOT NULL,
    SupplierId  int NOT NULL,
    CurrencyId  int NOT NULL,
    Debit       decimal(18,4) NOT NULL DEFAULT (0),   -- يخفض ما علينا (سداد/مرتجع)
    Credit      decimal(18,4) NOT NULL DEFAULT (0),   -- يزيد ما علينا (شراء آجل)
    RefType     nvarchar(30) NOT NULL,
    RefId       bigint NOT NULL,
    Description nvarchar(300) NULL,
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    CONSTRAINT PK_SupplierLedger PRIMARY KEY (Id),
    CONSTRAINT FK_SupplierLedger_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SupplierLedger_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT CK_SupplierLedger_Side CHECK ((Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0))
);
GO
CREATE INDEX IX_SupplierLedger_Account ON dbo.SupplierLedger (SupplierId, CurrencyId, Id) INCLUDE (Debit, Credit);
CREATE INDEX IX_SupplierLedger_Ref ON dbo.SupplierLedger (RefType, RefId);
GO

CREATE TABLE dbo.SupplierPayments (
    Id                   bigint IDENTITY(1,1) NOT NULL,
    PaymentNo            nvarchar(30) NOT NULL,
    SupplierId           int NOT NULL,
    DebtCurrencyId       int NOT NULL,
    PaidCurrencyId       int NOT NULL,
    AmountPaid           decimal(18,4) NOT NULL,
    ExchangeRateId       int NULL,
    RateUsed             decimal(18,6) NOT NULL DEFAULT (1),
    AmountInDebtCurrency decimal(18,4) NOT NULL,
    FxDifference         decimal(18,4) NOT NULL DEFAULT (0),
    PaymentDate          date NOT NULL,
    Status               nvarchar(10) NOT NULL DEFAULT (N'Posted'),   -- Posted / Reversed / Reversal
    ReversalOfId         bigint NULL,
    Notes                nvarchar(500) NULL,
    IdempotencyKey       nvarchar(64) NOT NULL,
    CreatedAt            datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy            int NOT NULL,
    UpdatedAt            datetime2(3) NULL,
    UpdatedBy            int NULL,
    CONSTRAINT PK_SupplierPayments PRIMARY KEY (Id),
    CONSTRAINT UQ_SupplierPayments_No UNIQUE (PaymentNo),
    CONSTRAINT UQ_SupplierPayments_Idem UNIQUE (IdempotencyKey),
    CONSTRAINT FK_SupplierPayments_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SupplierPayments_DebtCur FOREIGN KEY (DebtCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_SupplierPayments_PaidCur FOREIGN KEY (PaidCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_SupplierPayments_Rate FOREIGN KEY (ExchangeRateId) REFERENCES dbo.ExchangeRates(Id),
    CONSTRAINT FK_SupplierPayments_Reversal FOREIGN KEY (ReversalOfId) REFERENCES dbo.SupplierPayments(Id),
    CONSTRAINT CK_SupplierPayments_Values CHECK (AmountPaid > 0 AND RateUsed > 0 AND AmountInDebtCurrency > 0),
    CONSTRAINT CK_SupplierPayments_Status CHECK (Status IN (N'Posted', N'Reversed', N'Reversal'))
);
GO
CREATE INDEX IX_SupplierPayments_Supplier ON dbo.SupplierPayments (SupplierId, PaymentDate);
GO

CREATE TABLE dbo.PurchaseReturns (
    Id             bigint IDENTITY(1,1) NOT NULL,
    ReturnNo       nvarchar(30) NOT NULL,
    InvoiceId      bigint NOT NULL,
    ReturnDate     date NOT NULL,
    Total          decimal(18,4) NOT NULL,            -- بعملة الفاتورة الأصلية
    Reason         nvarchar(300) NULL,
    IdempotencyKey nvarchar(64) NOT NULL,
    CreatedAt      datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy      int NOT NULL,
    UpdatedAt      datetime2(3) NULL,
    UpdatedBy      int NULL,
    CONSTRAINT PK_PurchaseReturns PRIMARY KEY (Id),
    CONSTRAINT UQ_PurchaseReturns_No UNIQUE (ReturnNo),
    CONSTRAINT UQ_PurchaseReturns_Idem UNIQUE (IdempotencyKey),
    CONSTRAINT FK_PurchaseReturns_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.PurchaseInvoices(Id),
    CONSTRAINT CK_PurchaseReturns_Total CHECK (Total >= 0)
);
GO
CREATE INDEX IX_PurchaseReturns_Invoice ON dbo.PurchaseReturns (InvoiceId);
GO

CREATE TABLE dbo.PurchaseReturnLines (
    Id        bigint IDENTITY(1,1) NOT NULL,
    ReturnId  bigint NOT NULL,
    BatchId   bigint NOT NULL,
    Qty       decimal(18,3) NOT NULL,                 -- بالوحدة الأساسية
    UnitPrice decimal(18,4) NOT NULL,                 -- بعملة الفاتورة، للوحدة الأساسية
    LineTotal decimal(18,4) NOT NULL,
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    CONSTRAINT PK_PurchaseReturnLines PRIMARY KEY (Id),
    CONSTRAINT FK_PurchaseReturnLines_Return FOREIGN KEY (ReturnId) REFERENCES dbo.PurchaseReturns(Id),
    CONSTRAINT FK_PurchaseReturnLines_Batch FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT CK_PurchaseReturnLines_Values CHECK (Qty > 0 AND UnitPrice >= 0 AND LineTotal >= 0)
);
GO

/* =====================================================================
   5) الورديات والمبيعات
   ===================================================================== */

CREATE TABLE dbo.Shifts (
    Id                   int IDENTITY(1,1) NOT NULL,
    CashierId            int NOT NULL,
    DeviceName           nvarchar(100) NULL,
    OpenedAt             datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    ClosedAt             datetime2(3) NULL,
    Status               nvarchar(10) NOT NULL DEFAULT (N'Open'),
    CloseNotes           nvarchar(500) NULL,
    DifferenceApprovedBy int NULL,
    CreatedAt            datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy            int NOT NULL,
    UpdatedAt            datetime2(3) NULL,
    UpdatedBy            int NULL,
    CONSTRAINT PK_Shifts PRIMARY KEY (Id),
    CONSTRAINT FK_Shifts_Cashier FOREIGN KEY (CashierId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Shifts_ApprovedBy FOREIGN KEY (DifferenceApprovedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Shifts_Status CHECK (
        (Status = N'Open' AND ClosedAt IS NULL) OR (Status = N'Closed' AND ClosedAt IS NOT NULL))
);
GO
-- وردية مفتوحة واحدة لكل كاشير
CREATE UNIQUE INDEX UX_Shifts_OneOpenPerCashier ON dbo.Shifts (CashierId) WHERE Status = N'Open';
CREATE INDEX IX_Shifts_OpenedAt ON dbo.Shifts (OpenedAt);
GO

CREATE TABLE dbo.ShiftCashCounts (
    Id               bigint IDENTITY(1,1) NOT NULL,
    ShiftId          int NOT NULL,
    CurrencyId       int NOT NULL,
    OpeningAmount    decimal(18,4) NOT NULL DEFAULT (0),
    ExpectedAmount   decimal(18,4) NULL,
    ActualAmount     decimal(18,4) NULL,
    Difference       decimal(18,4) NULL,
    DifferenceReason nvarchar(300) NULL,
    CreatedAt        datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy        int NOT NULL,
    UpdatedAt        datetime2(3) NULL,
    UpdatedBy        int NULL,
    CONSTRAINT PK_ShiftCashCounts PRIMARY KEY (Id),
    CONSTRAINT FK_ShiftCashCounts_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_ShiftCashCounts_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT UQ_ShiftCashCounts_Shift_Currency UNIQUE (ShiftId, CurrencyId),
    CONSTRAINT CK_ShiftCashCounts_Values CHECK (OpeningAmount >= 0
        AND (ExpectedAmount IS NULL OR ExpectedAmount >= 0)
        AND (ActualAmount IS NULL OR ActualAmount >= 0))
);
GO

CREATE TABLE dbo.CashMovements (
    Id         bigint IDENTITY(1,1) NOT NULL,
    ShiftId    int NOT NULL,
    CurrencyId int NOT NULL,
    Direction  nvarchar(3) NOT NULL,
    Amount     decimal(18,4) NOT NULL,
    Reason     nvarchar(300) NOT NULL,
    CreatedAt  datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy  int NOT NULL,
    CONSTRAINT PK_CashMovements PRIMARY KEY (Id),
    CONSTRAINT FK_CashMovements_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_CashMovements_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT CK_CashMovements_Values CHECK (Direction IN (N'In', N'Out') AND Amount > 0
                                              AND LEN(LTRIM(RTRIM(Reason))) > 0)
);
GO
CREATE INDEX IX_CashMovements_Shift ON dbo.CashMovements (ShiftId, CurrencyId);
GO

CREATE TABLE dbo.SalesInvoices (
    Id               bigint IDENTITY(1,1) NOT NULL,
    InvoiceNo        nvarchar(30) NOT NULL,
    ShiftId          int NOT NULL,
    CustomerId       int NULL,
    CurrencyId       int NOT NULL,
    ExchangeRateId   int NULL,
    RateUsed         decimal(18,6) NOT NULL DEFAULT (1),
    Subtotal         decimal(18,4) NOT NULL,
    Discount         decimal(18,4) NOT NULL DEFAULT (0),
    Total            decimal(18,4) NOT NULL,           -- بعملة الفاتورة
    TotalBase        decimal(18,4) NOT NULL,           -- المكافئ بالعملة الأساسية (للتقارير)
    CreditAmount     decimal(18,4) NOT NULL DEFAULT (0),   -- الجزء الآجل
    CreditSettled    decimal(18,4) NOT NULL DEFAULT (0),   -- ما سُدِّد منه (سداد أو مرتجع مخصوم)
    ChangeAmount     decimal(18,4) NOT NULL DEFAULT (0),   -- الباقي المُسلَّم للعميل
    ChangeCurrencyId int NULL,
    Status           nvarchar(10) NOT NULL DEFAULT (N'Completed'),
    VoidReason       nvarchar(300) NULL,
    VoidedAt         datetime2(3) NULL,
    VoidedBy         int NULL,
    Notes            nvarchar(500) NULL,
    IdempotencyKey   nvarchar(64) NOT NULL,
    RowVersion       rowversion NOT NULL,
    CreatedAt        datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy        int NOT NULL,
    UpdatedAt        datetime2(3) NULL,
    UpdatedBy        int NULL,
    CONSTRAINT PK_SalesInvoices PRIMARY KEY (Id),
    CONSTRAINT UQ_SalesInvoices_No UNIQUE (InvoiceNo),
    CONSTRAINT UQ_SalesInvoices_Idem UNIQUE (IdempotencyKey),
    CONSTRAINT FK_SalesInvoices_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_SalesInvoices_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_SalesInvoices_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_SalesInvoices_Rate FOREIGN KEY (ExchangeRateId) REFERENCES dbo.ExchangeRates(Id),
    CONSTRAINT FK_SalesInvoices_ChangeCur FOREIGN KEY (ChangeCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_SalesInvoices_VoidedBy FOREIGN KEY (VoidedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_SalesInvoices_Amounts CHECK (RateUsed > 0 AND Subtotal >= 0 AND Discount >= 0
        AND Discount <= Subtotal AND Total >= 0 AND TotalBase >= 0 AND CreditAmount >= 0
        AND CreditAmount <= Total AND CreditSettled >= 0 AND CreditSettled <= CreditAmount
        AND ChangeAmount >= 0),
    CONSTRAINT CK_SalesInvoices_CreditNeedsCustomer CHECK (CreditAmount = 0 OR CustomerId IS NOT NULL),
    CONSTRAINT CK_SalesInvoices_Change CHECK (
        (ChangeAmount = 0 AND ChangeCurrencyId IS NULL) OR (ChangeAmount > 0 AND ChangeCurrencyId IS NOT NULL)),
    CONSTRAINT CK_SalesInvoices_Status CHECK (Status IN (N'Completed', N'Voided')),
    CONSTRAINT CK_SalesInvoices_Void CHECK (
        Status = N'Completed' OR (VoidReason IS NOT NULL AND VoidedBy IS NOT NULL AND VoidedAt IS NOT NULL))
);
GO
CREATE INDEX IX_SalesInvoices_Shift ON dbo.SalesInvoices (ShiftId);
CREATE INDEX IX_SalesInvoices_Date ON dbo.SalesInvoices (CreatedAt) INCLUDE (Total, TotalBase, Status);
CREATE INDEX IX_SalesInvoices_Customer ON dbo.SalesInvoices (CustomerId, CurrencyId, CreatedAt)
    INCLUDE (CreditAmount, CreditSettled, Status) WHERE CustomerId IS NOT NULL;
GO

CREATE TABLE dbo.SalesInvoiceLines (
    Id                 bigint IDENTITY(1,1) NOT NULL,
    InvoiceId          bigint NOT NULL,
    ProductUnitId      int NOT NULL,
    Qty                decimal(18,3) NOT NULL,         -- بوحدة البيع
    QtyBase            decimal(18,3) NOT NULL,
    UnitPrice          decimal(18,4) NOT NULL,         -- بعملة الفاتورة
    Discount           decimal(18,4) NOT NULL DEFAULT (0),
    LineTotal          decimal(18,4) NOT NULL,
    BelowCostConfirmed bit NOT NULL DEFAULT (0),       -- وافق الكاشير على البيع بخسارة (FR-POS-11)
    CreatedAt          datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy          int NOT NULL,
    CONSTRAINT PK_SalesInvoiceLines PRIMARY KEY (Id),
    CONSTRAINT FK_SalesInvoiceLines_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.SalesInvoices(Id),
    CONSTRAINT FK_SalesInvoiceLines_Unit FOREIGN KEY (ProductUnitId) REFERENCES dbo.ProductUnits(Id),
    CONSTRAINT CK_SalesInvoiceLines_Values CHECK (Qty > 0 AND QtyBase > 0 AND UnitPrice >= 0
                                                  AND Discount >= 0 AND LineTotal >= 0)
);
GO
CREATE INDEX IX_SalesInvoiceLines_Invoice ON dbo.SalesInvoiceLines (InvoiceId);
CREATE INDEX IX_SalesInvoiceLines_Unit ON dbo.SalesInvoiceLines (ProductUnitId) INCLUDE (QtyBase, LineTotal);
CREATE INDEX IX_SalesInvoiceLines_BelowCost ON dbo.SalesInvoiceLines (InvoiceId) WHERE BelowCostConfirmed = 1;
GO

CREATE TABLE dbo.SalesLineBatches (
    Id            bigint IDENTITY(1,1) NOT NULL,
    LineId        bigint NOT NULL,
    BatchId       bigint NOT NULL,
    QtyBase       decimal(18,3) NOT NULL,
    UnitCost      decimal(18,4) NOT NULL,              -- التكلفة المعتمدة للأرباح = AvgCost لحظة البيع (D-03)
    BatchUnitCost decimal(18,4) NOT NULL,              -- تكلفة الدفعة الفعلية (للتحول لاحقًا لطريقة أخرى)
    CreatedAt     datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy     int NOT NULL,
    CONSTRAINT PK_SalesLineBatches PRIMARY KEY (Id),
    CONSTRAINT FK_SalesLineBatches_Line FOREIGN KEY (LineId) REFERENCES dbo.SalesInvoiceLines(Id),
    CONSTRAINT FK_SalesLineBatches_Batch FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT CK_SalesLineBatches_Values CHECK (QtyBase > 0 AND UnitCost >= 0 AND BatchUnitCost >= 0)
);
GO
CREATE INDEX IX_SalesLineBatches_Line ON dbo.SalesLineBatches (LineId);
CREATE INDEX IX_SalesLineBatches_Batch ON dbo.SalesLineBatches (BatchId);
GO

CREATE TABLE dbo.SalesPayments (
    Id                      bigint IDENTITY(1,1) NOT NULL,
    InvoiceId               bigint NOT NULL,
    PaymentCurrencyId       int NOT NULL,
    AmountPaid              decimal(18,4) NOT NULL,    -- المبلغ المستلم فعليًا بعملة الدفع
    ExchangeRateId          int NULL,
    RateUsed                decimal(18,6) NOT NULL DEFAULT (1),
    AmountInInvoiceCurrency decimal(18,4) NOT NULL,
    CreatedAt               datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy               int NOT NULL,
    CONSTRAINT PK_SalesPayments PRIMARY KEY (Id),
    CONSTRAINT FK_SalesPayments_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.SalesInvoices(Id),
    CONSTRAINT FK_SalesPayments_Currency FOREIGN KEY (PaymentCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_SalesPayments_Rate FOREIGN KEY (ExchangeRateId) REFERENCES dbo.ExchangeRates(Id),
    CONSTRAINT CK_SalesPayments_Values CHECK (AmountPaid > 0 AND RateUsed > 0 AND AmountInInvoiceCurrency > 0)
);
GO
CREATE INDEX IX_SalesPayments_Invoice ON dbo.SalesPayments (InvoiceId);
GO

CREATE TABLE dbo.SalesReturns (
    Id               bigint IDENTITY(1,1) NOT NULL,
    ReturnNo         nvarchar(30) NOT NULL,
    InvoiceId        bigint NOT NULL,
    ShiftId          int NOT NULL,
    Total            decimal(18,4) NOT NULL,           -- بعملة الفاتورة الأصلية
    RefundMethod     nvarchar(15) NOT NULL,
    RefundCurrencyId int NOT NULL,
    Reason           nvarchar(300) NULL,
    IdempotencyKey   nvarchar(64) NOT NULL,
    CreatedAt        datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy        int NOT NULL,
    UpdatedAt        datetime2(3) NULL,
    UpdatedBy        int NULL,
    CONSTRAINT PK_SalesReturns PRIMARY KEY (Id),
    CONSTRAINT UQ_SalesReturns_No UNIQUE (ReturnNo),
    CONSTRAINT UQ_SalesReturns_Idem UNIQUE (IdempotencyKey),
    CONSTRAINT FK_SalesReturns_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.SalesInvoices(Id),
    CONSTRAINT FK_SalesReturns_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_SalesReturns_Currency FOREIGN KEY (RefundCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT CK_SalesReturns_Values CHECK (Total >= 0 AND RefundMethod IN (N'Cash', N'DebtReduction'))
);
GO
CREATE INDEX IX_SalesReturns_Invoice ON dbo.SalesReturns (InvoiceId);
CREATE INDEX IX_SalesReturns_Shift ON dbo.SalesReturns (ShiftId);
GO

CREATE TABLE dbo.SalesReturnLines (
    Id            bigint IDENTITY(1,1) NOT NULL,
    ReturnId      bigint NOT NULL,
    InvoiceLineId bigint NOT NULL,
    BatchId       bigint NOT NULL,                     -- الدفعة التي تعود إليها الكمية إن كانت سليمة
    Qty           decimal(18,3) NOT NULL,              -- بوحدة البيع
    QtyBase       decimal(18,3) NOT NULL,
    UnitPrice     decimal(18,4) NOT NULL,
    LineTotal     decimal(18,4) NOT NULL,
    ItemCondition nvarchar(10) NOT NULL,
    CreatedAt     datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy     int NOT NULL,
    CONSTRAINT PK_SalesReturnLines PRIMARY KEY (Id),
    CONSTRAINT FK_SalesReturnLines_Return FOREIGN KEY (ReturnId) REFERENCES dbo.SalesReturns(Id),
    CONSTRAINT FK_SalesReturnLines_Line FOREIGN KEY (InvoiceLineId) REFERENCES dbo.SalesInvoiceLines(Id),
    CONSTRAINT FK_SalesReturnLines_Batch FOREIGN KEY (BatchId) REFERENCES dbo.Batches(Id),
    CONSTRAINT CK_SalesReturnLines_Values CHECK (Qty > 0 AND QtyBase > 0 AND UnitPrice >= 0
                                                 AND LineTotal >= 0 AND ItemCondition IN (N'Good', N'Damaged'))
);
GO
CREATE INDEX IX_SalesReturnLines_Return ON dbo.SalesReturnLines (ReturnId);
CREATE INDEX IX_SalesReturnLines_InvoiceLine ON dbo.SalesReturnLines (InvoiceLineId);
GO

/* =====================================================================
   6) العملاء والديون والسدادات
   ===================================================================== */

CREATE TABLE dbo.CustomerBalances (
    CustomerId int NOT NULL,
    CurrencyId int NOT NULL,
    Balance    decimal(18,4) NOT NULL DEFAULT (0),     -- موجب = علينا له دين لنا، سالب = رصيد دائن للعميل
    RowVersion rowversion NOT NULL,
    CreatedAt  datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy  int NOT NULL,
    UpdatedAt  datetime2(3) NULL,
    UpdatedBy  int NULL,
    CONSTRAINT PK_CustomerBalances PRIMARY KEY (CustomerId, CurrencyId),
    CONSTRAINT FK_CustomerBalances_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_CustomerBalances_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id)
);
GO

CREATE TABLE dbo.CustomerLedger (
    Id          bigint IDENTITY(1,1) NOT NULL,
    CustomerId  int NOT NULL,
    CurrencyId  int NOT NULL,
    Debit       decimal(18,4) NOT NULL DEFAULT (0),    -- يزيد دين العميل (بيع آجل)
    Credit      decimal(18,4) NOT NULL DEFAULT (0),    -- يخفض دين العميل (سداد/مرتجع)
    RefType     nvarchar(30) NOT NULL,
    RefId       bigint NOT NULL,
    Description nvarchar(300) NULL,
    CreatedAt   datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy   int NOT NULL,
    CONSTRAINT PK_CustomerLedger PRIMARY KEY (Id),
    CONSTRAINT FK_CustomerLedger_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_CustomerLedger_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT CK_CustomerLedger_Side CHECK ((Debit > 0 AND Credit = 0) OR (Credit > 0 AND Debit = 0))
);
GO
CREATE INDEX IX_CustomerLedger_Account ON dbo.CustomerLedger (CustomerId, CurrencyId, Id) INCLUDE (Debit, Credit);
CREATE INDEX IX_CustomerLedger_Ref ON dbo.CustomerLedger (RefType, RefId);
GO

CREATE TABLE dbo.CustomerPayments (
    Id                   bigint IDENTITY(1,1) NOT NULL,
    ReceiptNo            nvarchar(30) NOT NULL,
    CustomerId           int NOT NULL,
    ShiftId              int NOT NULL,
    DebtCurrencyId       int NOT NULL,
    PaidCurrencyId       int NOT NULL,
    AmountPaid           decimal(18,4) NOT NULL,       -- المبلغ المستلم بعملة الدفع
    ExchangeRateId       int NULL,
    RateUsed             decimal(18,6) NOT NULL DEFAULT (1),
    AmountInDebtCurrency decimal(18,4) NOT NULL,       -- مجموع التوزيع على الفواتير + CreditedToBalance
    FxDifference         decimal(18,4) NOT NULL DEFAULT (0),
    CreditedToBalance    decimal(18,4) NOT NULL DEFAULT (0),   -- الفائض المقيَّد رصيدًا دائنًا (D-04)
    Status               nvarchar(10) NOT NULL DEFAULT (N'Posted'),   -- Posted / Reversed / Reversal
    ReversalOfId         bigint NULL,
    Notes                nvarchar(500) NULL,
    IdempotencyKey       nvarchar(64) NOT NULL,
    CreatedAt            datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy            int NOT NULL,
    UpdatedAt            datetime2(3) NULL,
    UpdatedBy            int NULL,
    CONSTRAINT PK_CustomerPayments PRIMARY KEY (Id),
    CONSTRAINT UQ_CustomerPayments_No UNIQUE (ReceiptNo),
    CONSTRAINT UQ_CustomerPayments_Idem UNIQUE (IdempotencyKey),
    CONSTRAINT FK_CustomerPayments_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_CustomerPayments_Shift FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_CustomerPayments_DebtCur FOREIGN KEY (DebtCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_CustomerPayments_PaidCur FOREIGN KEY (PaidCurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_CustomerPayments_Rate FOREIGN KEY (ExchangeRateId) REFERENCES dbo.ExchangeRates(Id),
    CONSTRAINT FK_CustomerPayments_Reversal FOREIGN KEY (ReversalOfId) REFERENCES dbo.CustomerPayments(Id),
    CONSTRAINT CK_CustomerPayments_Values CHECK (AmountPaid > 0 AND RateUsed > 0 AND AmountInDebtCurrency > 0
                                                 AND CreditedToBalance >= 0 AND CreditedToBalance <= AmountInDebtCurrency),
    CONSTRAINT CK_CustomerPayments_Status CHECK (Status IN (N'Posted', N'Reversed', N'Reversal'))
);
GO
CREATE INDEX IX_CustomerPayments_Customer ON dbo.CustomerPayments (CustomerId, CreatedAt);
CREATE INDEX IX_CustomerPayments_Shift ON dbo.CustomerPayments (ShiftId);
GO

CREATE TABLE dbo.CustomerPaymentAllocations (
    Id             bigint IDENTITY(1,1) NOT NULL,
    PaymentId      bigint NOT NULL,
    SalesInvoiceId bigint NOT NULL,
    Amount         decimal(18,4) NOT NULL,             -- بعملة الدين
    CreatedAt      datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy      int NOT NULL,
    CONSTRAINT PK_CustomerPaymentAllocations PRIMARY KEY (Id),
    CONSTRAINT FK_CPA_Payment FOREIGN KEY (PaymentId) REFERENCES dbo.CustomerPayments(Id),
    CONSTRAINT FK_CPA_Invoice FOREIGN KEY (SalesInvoiceId) REFERENCES dbo.SalesInvoices(Id),
    CONSTRAINT UQ_CPA_Payment_Invoice UNIQUE (PaymentId, SalesInvoiceId),
    CONSTRAINT CK_CPA_Amount CHECK (Amount > 0)
);
GO
CREATE INDEX IX_CPA_Invoice ON dbo.CustomerPaymentAllocations (SalesInvoiceId);
GO

/* =====================================================================
   7) المصروفات
   ===================================================================== */

CREATE TABLE dbo.ExpenseCategories (
    Id        int IDENTITY(1,1) NOT NULL,
    Name      nvarchar(100) NOT NULL,
    IsActive  bit NOT NULL DEFAULT (1),
    CreatedAt datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy int NOT NULL,
    UpdatedAt datetime2(3) NULL,
    UpdatedBy int NULL,
    CONSTRAINT PK_ExpenseCategories PRIMARY KEY (Id),
    CONSTRAINT UQ_ExpenseCategories_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.Expenses (
    Id             bigint IDENTITY(1,1) NOT NULL,
    CategoryId     int NOT NULL,
    CurrencyId     int NOT NULL,
    Amount         decimal(18,4) NOT NULL,
    ExchangeRateId int NULL,
    RateUsed       decimal(18,6) NOT NULL DEFAULT (1),
    AmountBase     decimal(18,4) NOT NULL,             -- المكافئ بالعملة الأساسية لحساب صافي الربح
    ExpenseDate    date NOT NULL,
    Notes          nvarchar(500) NULL,
    Status         nvarchar(10) NOT NULL DEFAULT (N'Posted'),
    CreatedAt      datetime2(3) NOT NULL DEFAULT (SYSDATETIME()),
    CreatedBy      int NOT NULL,
    UpdatedAt      datetime2(3) NULL,
    UpdatedBy      int NULL,
    CONSTRAINT PK_Expenses PRIMARY KEY (Id),
    CONSTRAINT FK_Expenses_Category FOREIGN KEY (CategoryId) REFERENCES dbo.ExpenseCategories(Id),
    CONSTRAINT FK_Expenses_Currency FOREIGN KEY (CurrencyId) REFERENCES dbo.Currencies(Id),
    CONSTRAINT FK_Expenses_Rate FOREIGN KEY (ExchangeRateId) REFERENCES dbo.ExchangeRates(Id),
    CONSTRAINT CK_Expenses_Values CHECK (Amount > 0 AND RateUsed > 0 AND AmountBase > 0),
    CONSTRAINT CK_Expenses_Status CHECK (Status IN (N'Posted', N'Voided'))
);
GO
CREATE INDEX IX_Expenses_Date ON dbo.Expenses (ExpenseDate) INCLUDE (CategoryId, AmountBase, Status);
GO

/* =====================================================================
   8) مفاتيح أجنبية لحقول التدقيق (CreatedBy / UpdatedBy → Users)
   ===================================================================== */

DECLARE @sql nvarchar(max);
SELECT @sql = STRING_AGG(CAST(
         N'ALTER TABLE dbo.' + QUOTENAME(t.name) +
         N' ADD CONSTRAINT ' + QUOTENAME(N'FK_' + t.name + N'_' + c.name) +
         N' FOREIGN KEY (' + QUOTENAME(c.name) + N') REFERENCES dbo.Users(Id);' AS nvarchar(max)), CHAR(10))
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.schema_id = SCHEMA_ID(N'dbo') AND c.name IN (N'CreatedBy', N'UpdatedBy');
EXEC sys.sp_executesql @sql;
GO

/* =====================================================================
   9) حماية السجلات المالية (BR-08): لا تعديل ولا حذف، والتصحيح بقيد عكسي
      (المشغّلات من نوع INSTEAD OF للتعديل/الحذف فقط، فلا تؤثر على INSERT)
   ===================================================================== */

DECLARE @Immutable TABLE (TableName sysname);
INSERT @Immutable VALUES
    (N'AuditLogs'), (N'StockMovements'), (N'CustomerLedger'), (N'SupplierLedger'),
    (N'SalesInvoiceLines'), (N'SalesLineBatches'), (N'SalesPayments'), (N'SalesReturnLines'),
    (N'PurchaseInvoiceLines'), (N'PurchaseReturnLines'), (N'CustomerPaymentAllocations'),
    (N'ExchangeRates'), (N'CashMovements');

DECLARE @NoDelete TABLE (TableName sysname);
INSERT @NoDelete VALUES
    (N'Users'), (N'Currencies'), (N'Products'), (N'ProductUnits'), (N'Customers'), (N'Suppliers'),
    (N'Batches'), (N'PurchaseOrders'), (N'PurchaseInvoices'), (N'PurchaseReturns'), (N'SupplierPayments'),
    (N'SalesInvoices'), (N'SalesReturns'), (N'CustomerPayments'), (N'Shifts'), (N'ShiftCashCounts'),
    (N'StockCounts'), (N'StockAdjustments'), (N'StockAdjustmentLines'), (N'Expenses'),
    (N'CustomerBalances'), (N'SupplierBalances');

DECLARE @name sysname, @stmt nvarchar(max);

DECLARE cur1 CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM @Immutable;
OPEN cur1;
FETCH NEXT FROM cur1 INTO @name;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @stmt = N'CREATE TRIGGER dbo.' + QUOTENAME(N'trg_' + @name + N'_NoUpdateDelete') + N' ON dbo.' + QUOTENAME(@name) + N'
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50010, N''هذا السجل غير قابل للتعديل أو الحذف (BR-08). استخدم قيدًا عكسيًا.'', 1;
END';
    EXEC sys.sp_executesql @stmt;
    FETCH NEXT FROM cur1 INTO @name;
END
CLOSE cur1; DEALLOCATE cur1;

DECLARE cur2 CURSOR LOCAL FAST_FORWARD FOR SELECT TableName FROM @NoDelete;
OPEN cur2;
FETCH NEXT FROM cur2 INTO @name;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @stmt = N'CREATE TRIGGER dbo.' + QUOTENAME(N'trg_' + @name + N'_NoDelete') + N' ON dbo.' + QUOTENAME(@name) + N'
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 50011, N''الحذف غير مسموح. استخدم التعطيل (IsActive) أو الإلغاء بقيد عكسي.'', 1;
END';
    EXEC sys.sp_executesql @stmt;
    FETCH NEXT FROM cur2 INTO @name;
END
CLOSE cur2; DEALLOCATE cur2;
GO

/* =====================================================================
   10) إجراء الترقيم + عروض التحقق
   ===================================================================== */

CREATE PROCEDURE dbo.usp_GetNextDocumentNumber
    @Name      nvarchar(50),
    @Number    bigint OUTPUT,
    @Formatted nvarchar(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Prefix nvarchar(10), @Pad int;

    UPDATE dbo.DocumentSequences WITH (UPDLOCK, ROWLOCK)
       SET @Number = LastNumber = LastNumber + 1,
           @Prefix = Prefix,
           @Pad    = PadLength
     WHERE Name = @Name;

    IF @@ROWCOUNT = 0
        THROW 50001, N'تسلسل المستندات غير معرّف.', 1;

    DECLARE @n nvarchar(20) = CAST(@Number AS nvarchar(20));
    SET @Formatted = ISNULL(@Prefix, N'') +
        CASE WHEN LEN(@n) >= @Pad THEN @n ELSE RIGHT(REPLICATE(N'0', @Pad) + @n, @Pad) END;
END
GO

-- مخزون كل منتج وأقرب انتهاء
CREATE VIEW dbo.vw_ProductStock
AS
SELECT p.Id AS ProductId,
       ISNULL(SUM(b.QtyRemaining), 0) AS TotalQtyBase,
       MIN(CASE WHEN b.QtyRemaining > 0 THEN b.ExpiryDate END) AS NearestExpiry
FROM dbo.Products p
LEFT JOIN dbo.Batches b ON b.ProductId = p.Id
GROUP BY p.Id;
GO

-- تحقق: أرصدة العملاء المخزنة مقابل السجل (يجب أن تكون النتيجة فارغة)
CREATE VIEW dbo.vw_CustomerBalanceCheck
AS
SELECT COALESCE(b.CustomerId, l.CustomerId) AS CustomerId,
       COALESCE(b.CurrencyId, l.CurrencyId) AS CurrencyId,
       ISNULL(b.Balance, 0)       AS StoredBalance,
       ISNULL(l.LedgerBalance, 0) AS LedgerBalance,
       ISNULL(b.Balance, 0) - ISNULL(l.LedgerBalance, 0) AS Diff
FROM dbo.CustomerBalances b
FULL JOIN (SELECT CustomerId, CurrencyId, SUM(Debit - Credit) AS LedgerBalance
           FROM dbo.CustomerLedger GROUP BY CustomerId, CurrencyId) l
       ON l.CustomerId = b.CustomerId AND l.CurrencyId = b.CurrencyId
WHERE ISNULL(b.Balance, 0) <> ISNULL(l.LedgerBalance, 0);
GO

-- تحقق: أرصدة الموردين (يجب أن تكون النتيجة فارغة)
CREATE VIEW dbo.vw_SupplierBalanceCheck
AS
SELECT COALESCE(b.SupplierId, l.SupplierId) AS SupplierId,
       COALESCE(b.CurrencyId, l.CurrencyId) AS CurrencyId,
       ISNULL(b.Balance, 0)       AS StoredBalance,
       ISNULL(l.LedgerBalance, 0) AS LedgerBalance,
       ISNULL(b.Balance, 0) - ISNULL(l.LedgerBalance, 0) AS Diff
FROM dbo.SupplierBalances b
FULL JOIN (SELECT SupplierId, CurrencyId, SUM(Credit - Debit) AS LedgerBalance
           FROM dbo.SupplierLedger GROUP BY SupplierId, CurrencyId) l
       ON l.SupplierId = b.SupplierId AND l.CurrencyId = b.CurrencyId
WHERE ISNULL(b.Balance, 0) <> ISNULL(l.LedgerBalance, 0);
GO

/* =====================================================================
   11) البيانات الأولية (Seed)
       ملاحظة: لا يُنشأ مستخدم المالك هنا لأن كلمة المرور يجب أن تُشفَّر من التطبيق.
       يُنشئ التطبيق المالك عند أول تشغيل (شاشة الإعداد الأولي).
       ولا تُدرج أسعار صرف: يدخلها المالك أو المحاسب (لا تخمين).
   ===================================================================== */

DECLARE @Sys int;

-- مستخدم النظام (معطّل، لا يمكنه تسجيل الدخول) لتوثيق البيانات الأولية
INSERT dbo.Users (Username, FullName, PasswordHash, PasswordSalt, IsActive, CreatedBy)
VALUES (N'system', N'النظام', 0x00, 0x00, 0, NULL);
SET @Sys = SCOPE_IDENTITY();

-- العملات (القيم الافتراضية للخانات العشرية قابلة للتعديل من المالك)
INSERT dbo.Currencies (Code, Name, Symbol, DecimalPlaces, RoundingMode, IsBase, CreatedBy) VALUES
 (N'YER', N'الريال اليمني',   N'ر.ي', 0, N'AwayFromZero', 1, @Sys),
 (N'SAR', N'الريال السعودي',  N'ر.س', 2, N'AwayFromZero', 0, @Sys),
 (N'USD', N'الدولار الأمريكي', N'$',   2, N'AwayFromZero', 0, @Sys);

-- الأذونات
INSERT dbo.Permissions (Code, Name, Module, CreatedBy) VALUES
 (N'Users.Manage',            N'إدارة المستخدمين',                    N'Auth', @Sys),
 (N'Roles.Manage',            N'إدارة الأدوار والصلاحيات',            N'Auth', @Sys),
 (N'Approvals.Grant',         N'منح موافقة على العمليات الحساسة',     N'Auth', @Sys),
 (N'Products.View',           N'عرض المنتجات',                        N'Products', @Sys),
 (N'Products.Manage',         N'إضافة وتعديل المنتجات',               N'Products', @Sys),
 (N'Products.EditPrice',      N'تعديل أسعار البيع',                   N'Products', @Sys),
 (N'Products.ImportExport',   N'استيراد وتصدير المنتجات',             N'Products', @Sys),
 (N'Categories.Manage',       N'إدارة التصنيفات',                     N'Products', @Sys),
 (N'Inventory.View',          N'عرض المخزون والتنبيهات',              N'Inventory', @Sys),
 (N'Inventory.Count',         N'جرد المخزون',                         N'Inventory', @Sys),
 (N'Inventory.Adjust',        N'تسوية المخزون',                       N'Inventory', @Sys),
 (N'Inventory.SellExpired',   N'بيع صنف منتهي الصلاحية (استثناء)',    N'Inventory', @Sys),
 (N'Suppliers.View',          N'عرض الموردين',                        N'Purchases', @Sys),
 (N'Suppliers.Manage',        N'إدارة الموردين',                      N'Purchases', @Sys),
 (N'Purchases.Orders',        N'أوامر الشراء',                        N'Purchases', @Sys),
 (N'Purchases.Invoice',       N'فواتير المشتريات',                    N'Purchases', @Sys),
 (N'Purchases.Return',        N'مرتجع المشتريات',                     N'Purchases', @Sys),
 (N'SupplierPayments.Create', N'تسجيل سداد مورد',                     N'Purchases', @Sys),
 (N'Sales.Create',            N'إتمام عملية بيع',                     N'Sales', @Sys),
 (N'Sales.Discount',          N'منح خصم',                             N'Sales', @Sys),
 (N'Sales.Void',              N'إلغاء فاتورة',                        N'Sales', @Sys),
 (N'Sales.Return',            N'مرتجع مبيعات',                        N'Sales', @Sys),
 (N'Sales.Reprint',           N'عرض أو طباعة فاتورة سابقة',           N'Sales', @Sys),
 (N'Shifts.Operate',          N'فتح وإغلاق الوردية',                  N'Shifts', @Sys),
 (N'Shifts.CashMovement',     N'حركات الصندوق (إيداع/سحب)',           N'Shifts', @Sys),
 (N'Shifts.ApproveDifference',N'اعتماد فرق إغلاق الوردية',            N'Shifts', @Sys),
 (N'Customers.View',          N'عرض العملاء',                         N'Customers', @Sys),
 (N'Customers.Manage',        N'إدارة العملاء وسقف الدين',            N'Customers', @Sys),
 (N'Customers.CreditOverride',N'تجاوز سقف الدين',                     N'Customers', @Sys),
 (N'CustomerPayments.Create', N'تسجيل سداد عميل',                     N'Customers', @Sys),
 (N'Currencies.Manage',       N'إدارة العملات',                       N'Currency', @Sys),
 (N'ExchangeRates.Manage',    N'تحديث أسعار الصرف',                   N'Currency', @Sys),
 (N'ExchangeRates.Override',  N'تعديل سعر الصرف داخل السند',          N'Currency', @Sys),
 (N'Expenses.Manage',         N'تسجيل المصروفات',                     N'Finance', @Sys),
 (N'Finance.View',            N'الملخص المالي والأرباح',              N'Finance', @Sys),
 (N'Reports.Sales',           N'تقارير المبيعات',                     N'Reports', @Sys),
 (N'Reports.Profit',          N'تقارير الأرباح',                      N'Reports', @Sys),
 (N'Reports.Inventory',       N'تقارير المخزون والصلاحية',            N'Reports', @Sys),
 (N'Reports.Debts',           N'تقارير الديون',                       N'Reports', @Sys),
 (N'Reports.BelowCost',       N'تقرير البيع بأقل من التكلفة',         N'Reports', @Sys),
 (N'Reports.Export',          N'تصدير التقارير',                      N'Reports', @Sys),
 (N'Audit.View',              N'مراجعة سجل التدقيق',                  N'System', @Sys),
 (N'Backup.Manage',           N'النسخ الاحتياطي والاستعادة',          N'System', @Sys),
 (N'Settings.Manage',         N'ضبط إعدادات النظام',                  N'System', @Sys);

-- الأدوار الأساسية
INSERT dbo.Roles (Name, Description, IsSystem, CreatedBy) VALUES
 (N'المالك',      N'كل الصلاحيات',                                   1, @Sys),
 (N'المدير',      N'الإدارة اليومية والتقارير دون إدارة المستخدمين',  1, @Sys),
 (N'الكاشير',     N'البيع والمرتجعات والوردية',                      1, @Sys),
 (N'أمين المخزن', N'المنتجات والمشتريات والجرد والتسويات',           1, @Sys),
 (N'المحاسب',     N'الديون والمصروفات وأسعار الصرف والتقارير المالية', 1, @Sys);

-- المالك: كل الأذونات
INSERT dbo.RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, @Sys FROM dbo.Roles r CROSS JOIN dbo.Permissions p WHERE r.Name = N'المالك';

-- المدير: كل الأذونات عدا إدارة المستخدمين والأدوار والنسخ والإعدادات وسجل التدقيق
INSERT dbo.RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, @Sys FROM dbo.Roles r CROSS JOIN dbo.Permissions p
WHERE r.Name = N'المدير'
  AND p.Code NOT IN (N'Users.Manage', N'Roles.Manage', N'Backup.Manage', N'Settings.Manage', N'Audit.View');

-- بقية الأدوار
DECLARE @RP TABLE (RoleName nvarchar(100), PermCode nvarchar(100));
INSERT @RP VALUES
 (N'الكاشير', N'Products.View'), (N'الكاشير', N'Customers.View'), (N'الكاشير', N'Sales.Create'),
 (N'الكاشير', N'Sales.Return'), (N'الكاشير', N'Sales.Reprint'), (N'الكاشير', N'Shifts.Operate'),
 (N'الكاشير', N'Shifts.CashMovement'),

 (N'أمين المخزن', N'Products.View'), (N'أمين المخزن', N'Products.Manage'),
 (N'أمين المخزن', N'Products.ImportExport'), (N'أمين المخزن', N'Categories.Manage'),
 (N'أمين المخزن', N'Inventory.View'), (N'أمين المخزن', N'Inventory.Count'),
 (N'أمين المخزن', N'Inventory.Adjust'), (N'أمين المخزن', N'Suppliers.View'),
 (N'أمين المخزن', N'Suppliers.Manage'), (N'أمين المخزن', N'Purchases.Orders'),
 (N'أمين المخزن', N'Purchases.Invoice'), (N'أمين المخزن', N'Purchases.Return'),
 (N'أمين المخزن', N'Reports.Inventory'),

 (N'المحاسب', N'Products.View'), (N'المحاسب', N'Customers.View'), (N'المحاسب', N'Customers.Manage'),
 (N'المحاسب', N'CustomerPayments.Create'), (N'المحاسب', N'Suppliers.View'),
 (N'المحاسب', N'SupplierPayments.Create'), (N'المحاسب', N'ExchangeRates.Manage'),
 (N'المحاسب', N'Expenses.Manage'), (N'المحاسب', N'Finance.View'), (N'المحاسب', N'Reports.Sales'),
 (N'المحاسب', N'Reports.Profit'), (N'المحاسب', N'Reports.Debts'), (N'المحاسب', N'Reports.Export');

INSERT dbo.RolePermissions (RoleId, PermissionId, CreatedBy)
SELECT r.Id, p.Id, @Sys
FROM @RP x
JOIN dbo.Roles r ON r.Name = x.RoleName
JOIN dbo.Permissions p ON p.Code = x.PermCode;

-- الإعدادات (القيم المعلّمة «افتراضي» يضبطها المالك)
INSERT dbo.Settings ([Key], [Value], Description, CreatedBy) VALUES
 (N'StoreName',                  N'البقالة',       N'اسم المحل',                                      @Sys),
 (N'BaseCurrencyCode',           N'YER',           N'العملة الأساسية',                                @Sys),
 (N'CostMethod',                 N'WeightedAverage', N'طريقة التكلفة المعتمدة (D-03)',                @Sys),
 (N'ExpiryAlertDays',            N'30',            N'عدد الأيام للتنبيه قبل انتهاء الصلاحية (افتراضي)', @Sys),
 (N'FxDeviationAlertPercent',    N'5',             N'حد تنبيه انحراف سعر الصرف % (D-05)',             @Sys),
 (N'ShiftDifferenceLimit',       N'0',             N'أقصى فرق مقبول عند إغلاق الوردية بالعملة الأساسية، وما فوقه يحتاج موافقة (D-06، افتراضي)', @Sys),
 (N'PurchasePriceAlertPercent',  N'20',            N'حد تنبيه تغير سعر الشراء % (FR-SUP-06، افتراضي)', @Sys),
 (N'LargeDiscountPercent',       N'10',            N'خصم يتطلب موافقة مدير % (FR-AUTH-06، افتراضي)',  @Sys),
 (N'MaxFailedLogins',            N'5',             N'عدد محاولات الدخول الفاشلة قبل القفل (FR-AUTH-07)', @Sys),
 (N'LockoutMinutes',             N'15',            N'مدة قفل الحساب بالدقائق',                        @Sys),
 (N'TaxEnabled',                 N'false',         N'تفعيل الضريبة (معطّلة افتراضيًا، NFR-13)',       @Sys),
 (N'TaxRatePercent',             N'0',             N'نسبة الضريبة %',                                 @Sys),
 (N'TaxNumber',                  N'',              N'الرقم الضريبي',                                  @Sys),
 (N'ReceiptFooter',              N'شكرًا لزيارتكم', N'تذييل الفاتورة',                                 @Sys),
 (N'AutoBackupTime',             N'02:00',         N'وقت النسخ الاحتياطي اليومي',                     @Sys),
 (N'BackupFolder',               N'',              N'مجلد حفظ النسخ الاحتياطية',                      @Sys);

-- تسلسل المستندات
INSERT dbo.DocumentSequences (Name, Prefix, PadLength, CreatedBy) VALUES
 (N'SalesInvoice',    N'INV-', 6, @Sys),
 (N'SalesReturn',     N'SRT-', 6, @Sys),
 (N'CustomerReceipt', N'RCT-', 6, @Sys),
 (N'PurchaseOrder',   N'PO-',  6, @Sys),
 (N'PurchaseInvoice', N'PI-',  6, @Sys),
 (N'PurchaseReturn',  N'PRT-', 6, @Sys),
 (N'SupplierPayment', N'PAY-', 6, @Sys),
 (N'StockCount',      N'CNT-', 6, @Sys),
 (N'StockAdjustment', N'ADJ-', 6, @Sys);

-- وحدات وتصنيف افتراضي ومصروفات
INSERT dbo.Units (Name, CreatedBy) VALUES
 (N'حبة', @Sys), (N'كرتون', @Sys), (N'دزينة', @Sys), (N'كيلو', @Sys),
 (N'لتر', @Sys), (N'علبة', @Sys), (N'كيس', @Sys), (N'باكت', @Sys);

INSERT dbo.Categories (ParentId, Name, CreatedBy) VALUES (NULL, N'عام', @Sys);

INSERT dbo.ExpenseCategories (Name, CreatedBy) VALUES
 (N'إيجار', @Sys), (N'كهرباء', @Sys), (N'مياه', @Sys), (N'رواتب', @Sys),
 (N'نقل', @Sys), (N'صيانة', @Sys), (N'أخرى', @Sys);
GO

/* =====================================================================
   12) حساب التطبيق محدود الصلاحيات (NFR-05) — قالب للتفعيل يدويًا
       لا تضع كلمة المرور في الكود (NFR-06)؛ خزّنها في إعدادات آمنة للتطبيق.
   ===================================================================== */
/*
USE master;
CREATE LOGIN GroceryApp WITH PASSWORD = N'<ضع كلمة مرور قوية هنا>', CHECK_POLICY = ON;
GO
USE GroceryPOS;
CREATE USER GroceryApp FOR LOGIN GroceryApp;
ALTER ROLE db_datareader ADD MEMBER GroceryApp;
ALTER ROLE db_datawriter ADD MEMBER GroceryApp;
ALTER ROLE db_backupoperator ADD MEMBER GroceryApp;   -- للنسخ الاحتياطي من التطبيق
GRANT EXECUTE ON SCHEMA::dbo TO GroceryApp;
-- الاستعادة (Restore) تحتاج صلاحية أعلى: نفّذها بحساب إداري منفصل لا بحساب التطبيق.
*/
GO

SET NOEXEC OFF;
GO

-- فحص سريع: المتوقع 46 جدولًا
SELECT COUNT(*) AS TablesCount FROM sys.tables WHERE schema_id = SCHEMA_ID(N'dbo');
GO
