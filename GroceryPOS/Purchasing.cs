using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GroceryPOS
{
    internal sealed class SupplierRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }

    internal sealed class SupplierDraft
    {
        internal int? Id { get; set; }
        internal string Name { get; set; }
        internal string Phone { get; set; }
        internal string Address { get; set; }
        internal string Notes { get; set; }
    }

    internal sealed class SupplierOption
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        internal SupplierOption(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal static class SuppliersService
    {
        internal static List<SupplierRecord> Search(AuthenticatedUser user, string searchText)
        {
            RequireViewPermission(user);
            string search = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();
            var suppliers = new List<SupplierRecord>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT Id, Name, Phone, Address, Notes, IsActive
                  FROM dbo.Suppliers
                  WHERE @Search IS NULL OR Name LIKE N'%' + @Search + N'%'
                     OR Phone LIKE N'%' + @Search + N'%'
                     OR Address LIKE N'%' + @Search + N'%'
                  ORDER BY IsActive DESC, Name, Id;",
                connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value =
                    search == null ? (object)DBNull.Value : search;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(new SupplierRecord
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Address = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            IsActive = reader.GetBoolean(5)
                        });
                    }
                }
            }

            return suppliers;
        }

        internal static void Save(AuthenticatedUser user, SupplierDraft draft)
        {
            RequireManagePermission(user);
            Validate(draft);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                SupplierRecord previous = draft.Id.HasValue
                    ? LoadOne(connection, transaction, draft.Id.Value)
                    : null;
                if (draft.Id.HasValue && previous == null)
                {
                    throw new InvalidOperationException("المورد المطلوب تعديله غير موجود.");
                }

                int supplierId;
                if (previous == null)
                {
                    using (var command = new SqlCommand(
                        @"INSERT dbo.Suppliers (Name, Phone, Address, Notes, CreatedBy)
                          VALUES (@Name, @Phone, @Address, @Notes, @CreatedBy);
                          SELECT CAST(SCOPE_IDENTITY() AS int);",
                        connection,
                        transaction))
                    {
                        AddParameters(command, draft);
                        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                        supplierId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                else
                {
                    supplierId = previous.Id;
                    using (var command = new SqlCommand(
                        @"UPDATE dbo.Suppliers
                          SET Name = @Name, Phone = @Phone, Address = @Address, Notes = @Notes,
                              UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                          WHERE Id = @Id;",
                        connection,
                        transaction))
                    {
                        AddParameters(command, draft);
                        command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = supplierId;
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("تعذر حفظ تعديلات المورد.");
                        }
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    supplierId,
                    previous == null ? "SupplierCreated" : "SupplierUpdated",
                    previous == null ? null : ToJson(previous),
                    ToJson(draft, previous == null || previous.IsActive));
                transaction.Commit();
            }
        }

        internal static void SetActive(AuthenticatedUser user, int supplierId, bool isActive)
        {
            RequireManagePermission(user);
            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                SupplierRecord previous = LoadOne(connection, transaction, supplierId);
                if (previous == null)
                {
                    throw new InvalidOperationException("المورد غير موجود.");
                }

                if (previous.IsActive == isActive)
                {
                    transaction.Commit();
                    return;
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Suppliers
                      SET IsActive = @IsActive, UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                      WHERE Id = @Id;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = supplierId;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تعذر تغيير حالة المورد.");
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    supplierId,
                    isActive ? "SupplierActivated" : "SupplierDeactivated",
                    ToJson(previous),
                    ToJson(previous, isActive));
                transaction.Commit();
            }
        }

        private static SupplierRecord LoadOne(
            SqlConnection connection,
            SqlTransaction transaction,
            int supplierId)
        {
            using (var command = new SqlCommand(
                @"SELECT Id, Name, Phone, Address, Notes, IsActive
                  FROM dbo.Suppliers WITH (UPDLOCK, HOLDLOCK)
                  WHERE Id = @Id;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = supplierId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new SupplierRecord
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Phone = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Address = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Notes = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        IsActive = reader.GetBoolean(5)
                    };
                }
            }
        }

        private static void AddParameters(SqlCommand command, SupplierDraft draft)
        {
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = draft.Name;
            command.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value =
                OptionalValue(draft.Phone);
            command.Parameters.Add("@Address", SqlDbType.NVarChar, 300).Value =
                OptionalValue(draft.Address);
            command.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value =
                OptionalValue(draft.Notes);
        }

        private static void Validate(SupplierDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            draft.Name = (draft.Name ?? string.Empty).Trim();
            draft.Phone = CleanOptional(draft.Phone);
            draft.Address = CleanOptional(draft.Address);
            draft.Notes = CleanOptional(draft.Notes);
            if (draft.Name.Length == 0 || draft.Name.Length > 200)
            {
                throw new ArgumentException("اسم المورد مطلوب ولا يتجاوز 200 حرف.");
            }

            if ((draft.Phone != null && draft.Phone.Length > 30) ||
                (draft.Address != null && draft.Address.Length > 300) ||
                (draft.Notes != null && draft.Notes.Length > 500))
            {
                throw new ArgumentException("تجاوز أحد حقول المورد الحد الأقصى المسموح.");
            }
        }

        private static string CleanOptional(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static object OptionalValue(string value)
        {
            return value == null ? (object)DBNull.Value : value;
        }

        private static string ToJson(SupplierRecord supplier)
        {
            return ToJson(
                supplier.Name,
                supplier.Phone,
                supplier.Address,
                supplier.Notes,
                supplier.IsActive);
        }

        private static string ToJson(SupplierRecord supplier, bool isActive)
        {
            return ToJson(supplier.Name, supplier.Phone, supplier.Address, supplier.Notes, isActive);
        }

        private static string ToJson(SupplierDraft draft, bool isActive)
        {
            return ToJson(draft.Name, draft.Phone, draft.Address, draft.Notes, isActive);
        }

        private static string ToJson(
            string name,
            string phone,
            string address,
            string notes,
            bool isActive)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{\"Name\":\"{0}\",\"Phone\":{1},\"Address\":{2},\"Notes\":{3},\"IsActive\":{4}}}",
                EscapeJson(name),
                JsonValue(phone),
                JsonValue(address),
                JsonValue(notes),
                isActive ? "true" : "false");
        }

        private static string JsonValue(string value)
        {
            return value == null ? "null" : "\"" + EscapeJson(value) + "\"";
        }

        private static void InsertAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int supplierId,
            string action,
            string oldValues,
            string newValues)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, OldValues, NewValues)
                  VALUES
                    (@UserId, @Action, N'Suppliers', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = action;
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    supplierId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value =
                    oldValues == null ? (object)DBNull.Value : oldValues;
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static string EscapeJson(string value)
        {
            var result = new StringBuilder(value ?? string.Empty);
            return result
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .ToString();
        }

        private static void RequireViewPermission(AuthenticatedUser user)
        {
            if (user == null ||
                (!user.HasPermission("Suppliers.View") && !user.HasPermission("Suppliers.Manage")))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية عرض الموردين.");
            }
        }

        private static void RequireManagePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Suppliers.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة الموردين.");
            }
        }
    }

    internal sealed class CurrencyOption
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public int DecimalPlaces { get; private set; }
        public string RoundingMode { get; private set; }
        public bool IsBase { get; private set; }

        internal CurrencyOption(
            int id,
            string code,
            string name,
            int decimalPlaces,
            string roundingMode,
            bool isBase)
        {
            Id = id;
            Code = code;
            Name = name;
            DecimalPlaces = decimalPlaces;
            RoundingMode = roundingMode;
            IsBase = isBase;
        }

        public override string ToString()
        {
            return Code + " - " + Name;
        }
    }

    internal sealed class ProductUnitOption
    {
        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string UnitName { get; private set; }
        public decimal FactorToBase { get; private set; }
        public bool TracksExpiry { get; private set; }
        public string DisplayName { get; private set; }

        internal ProductUnitOption(
            int id,
            int productId,
            string productName,
            string unitName,
            decimal factorToBase,
            bool tracksExpiry)
        {
            Id = id;
            ProductId = productId;
            ProductName = productName;
            UnitName = unitName;
            FactorToBase = factorToBase;
            TracksExpiry = tracksExpiry;
            DisplayName = productName + " — " + unitName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    internal sealed class PurchaseInvoiceLineDraft
    {
        public int ProductUnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    internal sealed class PurchaseInvoiceDraft
    {
        internal int SupplierId { get; set; }
        internal int CurrencyId { get; set; }
        internal DateTime InvoiceDate { get; set; }
        internal string SupplierInvoiceNo { get; set; }
        internal string Notes { get; set; }
        internal IList<PurchaseInvoiceLineDraft> Lines { get; set; }
    }

    internal sealed class PurchaseInvoiceRecord
    {
        public long Id { get; set; }
        public string InvoiceNo { get; set; }
        public string SupplierInvoiceNo { get; set; }
        public string SupplierName { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public decimal TotalBase { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; }
        public int LineCount { get; set; }
    }

    internal sealed class PurchaseInvoiceLineRecord
    {
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public decimal QtyBase { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    internal static class PurchasesService
    {
        private const decimal MaximumMoney = 99999999999999.9999m;
        private const decimal MaximumQuantity = 999999999999999.999m;

        private sealed class CurrencyDetails
        {
            internal int Id;
            internal string Code;
            internal int DecimalPlaces;
            internal string RoundingMode;
            internal bool IsBase;
            internal int? ExchangeRateId;
            internal decimal RateToBase;
        }

        private sealed class PreparedLine
        {
            internal PurchaseInvoiceLineDraft Draft;
            internal int ProductId;
            internal string ProductName;
            internal string UnitName;
            internal decimal FactorToBase;
            internal decimal QtyBase;
            internal decimal UnitCostBase;
            internal decimal LineTotal;
        }

        internal static List<SupplierOption> LoadActiveSuppliers(AuthenticatedUser user)
        {
            RequireInvoicePermission(user);
            var suppliers = new List<SupplierOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                "SELECT Id, Name FROM dbo.Suppliers WHERE IsActive = 1 ORDER BY Name, Id;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    suppliers.Add(new SupplierOption(reader.GetInt32(0), reader.GetString(1)));
                }
            }

            if (suppliers.Count == 0)
            {
                throw new InvalidOperationException("أضف موردًا نشطًا قبل إنشاء فاتورة شراء.");
            }

            return suppliers;
        }

        internal static List<CurrencyOption> LoadActiveCurrencies(AuthenticatedUser user)
        {
            RequireInvoicePermission(user);
            var currencies = new List<CurrencyOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT Id, Code, Name, DecimalPlaces, RoundingMode, IsBase
                  FROM dbo.Currencies
                  WHERE IsActive = 1
                  ORDER BY IsBase DESC, Code;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    currencies.Add(new CurrencyOption(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3),
                        reader.GetString(4),
                        reader.GetBoolean(5)));
                }
            }

            if (currencies.Count == 0 || currencies.Count(currency => currency.IsBase) != 1)
            {
                throw new InvalidOperationException("يجب تفعيل عملة أساسية واحدة على الأقل قبل تسجيل المشتريات.");
            }

            return currencies;
        }

        internal static List<ProductUnitOption> LoadPurchaseUnits(AuthenticatedUser user)
        {
            RequireInvoicePermission(user);
            var units = new List<ProductUnitOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT pu.Id, p.Id, p.Name, u.Name, pu.FactorToBase, p.TracksExpiry
                  FROM dbo.ProductUnits pu
                  INNER JOIN dbo.Products p ON p.Id = pu.ProductId AND p.IsActive = 1
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  WHERE pu.IsActive = 1
                  ORDER BY p.Name, u.Name;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    units.Add(new ProductUnitOption(
                        reader.GetInt32(0),
                        reader.GetInt32(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetDecimal(4),
                        reader.GetBoolean(5)));
                }
            }

            if (units.Count == 0)
            {
                throw new InvalidOperationException("لا توجد منتجات ووحدات نشطة لإضافتها إلى فاتورة الشراء.");
            }

            return units;
        }

        internal static List<PurchaseInvoiceRecord> LoadInvoices(AuthenticatedUser user)
        {
            RequireInvoicePermission(user);
            var invoices = new List<PurchaseInvoiceRecord>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT i.Id, i.InvoiceNo, i.SupplierInvoiceNo, s.Name, c.Code,
                         i.InvoiceDate, i.Total, i.TotalBase, i.PaidAmount, i.Status,
                         (SELECT COUNT(*) FROM dbo.PurchaseInvoiceLines l WHERE l.InvoiceId = i.Id)
                  FROM dbo.PurchaseInvoices i
                  INNER JOIN dbo.Suppliers s ON s.Id = i.SupplierId
                  INNER JOIN dbo.Currencies c ON c.Id = i.CurrencyId
                  ORDER BY i.InvoiceDate DESC, i.Id DESC;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    invoices.Add(new PurchaseInvoiceRecord
                    {
                        Id = reader.GetInt64(0),
                        InvoiceNo = reader.GetString(1),
                        SupplierInvoiceNo = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        SupplierName = reader.GetString(3),
                        CurrencyCode = reader.GetString(4),
                        InvoiceDate = reader.GetDateTime(5),
                        Total = reader.GetDecimal(6),
                        TotalBase = reader.GetDecimal(7),
                        PaidAmount = reader.GetDecimal(8),
                        Status = reader.GetString(9),
                        LineCount = reader.GetInt32(10)
                    });
                }
            }

            return invoices;
        }

        internal static List<PurchaseInvoiceLineRecord> LoadInvoiceLines(
            AuthenticatedUser user,
            long invoiceId)
        {
            RequireInvoicePermission(user);
            var lines = new List<PurchaseInvoiceLineRecord>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT p.Name, u.Name, l.Qty, l.QtyBase, l.UnitPrice, l.LineTotal,
                         l.BatchNo, l.ExpiryDate
                  FROM dbo.PurchaseInvoiceLines l
                  INNER JOIN dbo.ProductUnits pu ON pu.Id = l.ProductUnitId
                  INNER JOIN dbo.Products p ON p.Id = pu.ProductId
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  WHERE l.InvoiceId = @InvoiceId
                  ORDER BY l.Id;",
                connection))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lines.Add(new PurchaseInvoiceLineRecord
                        {
                            ProductName = reader.GetString(0),
                            UnitName = reader.GetString(1),
                            Quantity = reader.GetDecimal(2),
                            QtyBase = reader.GetDecimal(3),
                            UnitPrice = reader.GetDecimal(4),
                            LineTotal = reader.GetDecimal(5),
                            BatchNo = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            ExpiryDate = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                        });
                    }
                }
            }

            return lines;
        }

        internal static string PostInvoice(AuthenticatedUser user, PurchaseInvoiceDraft draft)
        {
            RequireInvoicePermission(user);
            Validate(draft);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                EnsureActiveSupplier(connection, transaction, draft.SupplierId);
                CurrencyDetails currency = LoadCurrency(
                    connection,
                    transaction,
                    draft.CurrencyId,
                    draft.InvoiceDate.Date);

                List<PreparedLine> lines = new List<PreparedLine>();
                foreach (PurchaseInvoiceLineDraft line in draft.Lines)
                {
                    lines.Add(PrepareLine(connection, transaction, line, draft.InvoiceDate.Date, currency));
                }

                decimal total = 0m;
                foreach (PreparedLine line in lines)
                {
                    total = checked(total + line.LineTotal);
                }

                if (total < 0 || total > MaximumMoney)
                {
                    throw new ArgumentException("إجمالي الفاتورة يتجاوز الحد المسموح.");
                }

                if (total > 0 && currency.RateToBase > MaximumMoney / total)
                {
                    throw new ArgumentException("الإجمالي بالعملة الأساسية يتجاوز الحد المسموح.");
                }

                decimal totalBase = RoundToCurrency(
                    total * currency.RateToBase,
                    currency.IsBase ? currency.DecimalPlaces : GetBaseCurrencyDecimals(connection, transaction),
                    currency.IsBase ? currency.RoundingMode : GetBaseCurrencyRounding(connection, transaction));
                if (totalBase < 0 || totalBase > MaximumMoney)
                {
                    throw new ArgumentException("الإجمالي بالعملة الأساسية يتجاوز الحد المسموح.");
                }

                var incomingByProduct = new Dictionary<int, decimal[]>();
                foreach (PreparedLine line in lines)
                {
                    decimal[] aggregate;
                    if (!incomingByProduct.TryGetValue(line.ProductId, out aggregate))
                    {
                        aggregate = new decimal[2];
                        incomingByProduct.Add(line.ProductId, aggregate);
                    }

                    decimal totalIncomingQty = checked(aggregate[0] + line.QtyBase);
                    aggregate[1] +=
                        (line.UnitCostBase - aggregate[1]) * (line.QtyBase / totalIncomingQty);
                    aggregate[0] = totalIncomingQty;
                }

                long invoiceId;
                string invoiceNumber = GetNextInvoiceNumber(connection, transaction);
                using (var command = new SqlCommand(
                    @"INSERT dbo.PurchaseInvoices
                        (InvoiceNo, SupplierInvoiceNo, SupplierId, CurrencyId, ExchangeRateId,
                         RateUsed, InvoiceDate, Total, TotalBase, PaidAmount, Status, Notes,
                         IdempotencyKey, CreatedBy)
                      VALUES
                        (@InvoiceNo, @SupplierInvoiceNo, @SupplierId, @CurrencyId, @ExchangeRateId,
                         @RateUsed, @InvoiceDate, @Total, @TotalBase, 0, N'Posted', @Notes,
                         @IdempotencyKey, @CreatedBy);
                      SELECT CAST(SCOPE_IDENTITY() AS bigint);",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 30).Value = invoiceNumber;
                    command.Parameters.Add("@SupplierInvoiceNo", SqlDbType.NVarChar, 50).Value =
                        OptionalValue(draft.SupplierInvoiceNo);
                    command.Parameters.Add("@SupplierId", SqlDbType.Int).Value = draft.SupplierId;
                    command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currency.Id;
                    command.Parameters.Add("@ExchangeRateId", SqlDbType.Int).Value =
                        currency.ExchangeRateId.HasValue ? (object)currency.ExchangeRateId.Value : DBNull.Value;
                    AddDecimal(command, "@RateUsed", 18, 6, currency.RateToBase);
                    command.Parameters.Add("@InvoiceDate", SqlDbType.Date).Value = draft.InvoiceDate.Date;
                    AddDecimal(command, "@Total", 18, 4, total);
                    AddDecimal(command, "@TotalBase", 18, 4, totalBase);
                    command.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = OptionalValue(draft.Notes);
                    command.Parameters.Add("@IdempotencyKey", SqlDbType.NVarChar, 64).Value =
                        Guid.NewGuid().ToString("N");
                    command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                    invoiceId = Convert.ToInt64(command.ExecuteScalar());
                }

                foreach (KeyValuePair<int, decimal[]> product in incomingByProduct.OrderBy(item => item.Key))
                {
                    UpdateWeightedAverageCost(
                        connection,
                        transaction,
                        user.Id,
                        product.Key,
                        product.Value[0],
                        product.Value[1]);
                }

                foreach (PreparedLine line in lines)
                {
                    long lineId = InsertInvoiceLine(connection, transaction, user.Id, invoiceId, line);
                    long batchId = InsertBatch(connection, transaction, user.Id, lineId, line);
                    InsertStockMovement(connection, transaction, user.Id, invoiceId, batchId, line);
                }

                UpdateSupplierBalance(connection, transaction, user.Id, draft.SupplierId, currency.Id, total);
                InsertSupplierLedger(connection, transaction, user.Id, invoiceId, draft.SupplierId, currency.Id, total);
                InsertInvoiceAudit(connection, transaction, user.Id, invoiceId, invoiceNumber, draft, currency, total, totalBase, lines.Count);
                transaction.Commit();
                return invoiceNumber;
            }
        }

        private static PreparedLine PrepareLine(
            SqlConnection connection,
            SqlTransaction transaction,
            PurchaseInvoiceLineDraft draft,
            DateTime invoiceDate,
            CurrencyDetails currency)
        {
            using (var command = new SqlCommand(
                @"SELECT pu.ProductId, p.Name, u.Name, pu.FactorToBase, p.TracksExpiry
                  FROM dbo.ProductUnits pu WITH (UPDLOCK, HOLDLOCK)
                  INNER JOIN dbo.Products p WITH (UPDLOCK, HOLDLOCK)
                    ON p.Id = pu.ProductId AND p.IsActive = 1
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  WHERE pu.Id = @Id AND pu.IsActive = 1;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = draft.ProductUnitId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("أحد المنتجات أو وحدات الشراء المحددة غير نشط.");
                    }

                    int productId = reader.GetInt32(0);
                    string productName = reader.GetString(1);
                    string unitName = reader.GetString(2);
                    decimal factor = reader.GetDecimal(3);
                    bool tracksExpiry = reader.GetBoolean(4);
                    decimal qtyBase = decimal.Round(
                        draft.Quantity * factor,
                        3,
                        MidpointRounding.AwayFromZero);

                    if (qtyBase <= 0 || qtyBase > MaximumQuantity)
                    {
                        throw new ArgumentException("كمية الوحدة الأساسية للمنتج " + productName + " خارج النطاق.");
                    }

                    if (draft.UnitPrice > 0 &&
                        draft.UnitPrice > (MaximumMoney * factor / currency.RateToBase))
                    {
                        throw new ArgumentException("تكلفة الوحدة الأساسية للمنتج " + productName + " تتجاوز الحد المسموح.");
                    }

                    if (tracksExpiry && !draft.ExpiryDate.HasValue)
                    {
                        throw new ArgumentException("تاريخ الصلاحية مطلوب للمنتج " + productName + ".");
                    }

                    if (draft.ExpiryDate.HasValue && draft.ExpiryDate.Value.Date < invoiceDate)
                    {
                        throw new ArgumentException("لا يمكن استلام دفعة منتهية الصلاحية للمنتج " + productName + ".");
                    }

                    decimal unitCostBase;
                    try
                    {
                        unitCostBase = decimal.Round(
                            draft.UnitPrice * currency.RateToBase / factor,
                            4,
                            MidpointRounding.AwayFromZero);
                    }
                    catch (OverflowException)
                    {
                        throw new ArgumentException("تكلفة الوحدة الأساسية للمنتج " + productName + " تتجاوز الحد المسموح.");
                    }

                    if (draft.UnitPrice > 0 && draft.Quantity > MaximumMoney / draft.UnitPrice)
                    {
                        throw new ArgumentException("إجمالي المنتج " + productName + " يتجاوز الحد المسموح.");
                    }

                    decimal lineTotal = RoundToCurrency(
                        draft.Quantity * draft.UnitPrice,
                        currency.DecimalPlaces,
                        currency.RoundingMode);
                    if (unitCostBase < 0 || unitCostBase > MaximumMoney ||
                        lineTotal < 0 || lineTotal > MaximumMoney)
                    {
                        throw new ArgumentException("سعر المنتج " + productName + " يتجاوز الحد المسموح.");
                    }

                    return new PreparedLine
                    {
                        Draft = draft,
                        ProductId = productId,
                        ProductName = productName,
                        UnitName = unitName,
                        FactorToBase = factor,
                        QtyBase = qtyBase,
                        UnitCostBase = unitCostBase,
                        LineTotal = lineTotal
                    };
                }
            }
        }

        private static CurrencyDetails LoadCurrency(
            SqlConnection connection,
            SqlTransaction transaction,
            int currencyId,
            DateTime invoiceDate)
        {
            CurrencyDetails currency;
            using (var command = new SqlCommand(
                @"SELECT Id, Code, DecimalPlaces, RoundingMode, IsBase
                  FROM dbo.Currencies WITH (UPDLOCK, HOLDLOCK)
                  WHERE Id = @Id AND IsActive = 1;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = currencyId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("عملة الفاتورة غير موجودة أو غير نشطة.");
                    }

                    currency = new CurrencyDetails
                    {
                        Id = reader.GetInt32(0),
                        Code = reader.GetString(1),
                        DecimalPlaces = reader.GetInt32(2),
                        RoundingMode = reader.GetString(3),
                        IsBase = reader.GetBoolean(4),
                        RateToBase = 1m
                    };
                }
            }

            if (!currency.IsBase)
            {
                using (var command = new SqlCommand(
                    @"SELECT TOP (1) Id, RateToBase
                      FROM dbo.ExchangeRates WITH (UPDLOCK, HOLDLOCK)
                      WHERE CurrencyId = @CurrencyId
                        AND EffectiveFrom < DATEADD(DAY, 1, @InvoiceDate)
                      ORDER BY EffectiveFrom DESC, Id DESC;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currency.Id;
                    command.Parameters.Add("@InvoiceDate", SqlDbType.Date).Value = invoiceDate;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException(
                                "لا يوجد سعر صرف مسجل لعملة " + currency.Code + " بتاريخ الفاتورة.");
                        }

                        currency.ExchangeRateId = reader.GetInt32(0);
                        currency.RateToBase = reader.GetDecimal(1);
                    }
                }
            }

            return currency;
        }

        private static int GetBaseCurrencyDecimals(SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(
                "SELECT DecimalPlaces FROM dbo.Currencies WITH (HOLDLOCK) WHERE IsBase = 1 AND IsActive = 1;",
                connection,
                transaction))
            {
                object value = command.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                {
                    throw new InvalidOperationException("تعذر تحديد العملة الأساسية النشطة.");
                }

                return Convert.ToInt32(value);
            }
        }

        private static string GetBaseCurrencyRounding(SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(
                "SELECT RoundingMode FROM dbo.Currencies WITH (HOLDLOCK) WHERE IsBase = 1 AND IsActive = 1;",
                connection,
                transaction))
            {
                object value = command.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                {
                    throw new InvalidOperationException("تعذر تحديد طريقة تقريب العملة الأساسية.");
                }

                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private static long InsertInvoiceLine(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            PreparedLine line)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.PurchaseInvoiceLines
                    (InvoiceId, ProductUnitId, Qty, QtyBase, UnitPrice, UnitCostBase,
                     LineTotal, BatchNo, ExpiryDate, CreatedBy)
                  VALUES
                    (@InvoiceId, @ProductUnitId, @Qty, @QtyBase, @UnitPrice, @UnitCostBase,
                     @LineTotal, @BatchNo, @ExpiryDate, @CreatedBy);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@ProductUnitId", SqlDbType.Int).Value = line.Draft.ProductUnitId;
                AddDecimal(command, "@Qty", 18, 3, line.Draft.Quantity);
                AddDecimal(command, "@QtyBase", 18, 3, line.QtyBase);
                AddDecimal(command, "@UnitPrice", 18, 4, line.Draft.UnitPrice);
                AddDecimal(command, "@UnitCostBase", 18, 4, line.UnitCostBase);
                AddDecimal(command, "@LineTotal", 18, 4, line.LineTotal);
                command.Parameters.Add("@BatchNo", SqlDbType.NVarChar, 50).Value =
                    OptionalValue(line.Draft.BatchNo);
                command.Parameters.Add("@ExpiryDate", SqlDbType.Date).Value =
                    line.Draft.ExpiryDate.HasValue
                        ? (object)line.Draft.ExpiryDate.Value.Date
                        : DBNull.Value;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private static long InsertBatch(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceLineId,
            PreparedLine line)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.Batches
                    (ProductId, PurchaseInvoiceLineId, BatchNo, ExpiryDate,
                     QtyInitial, QtyRemaining, UnitCost, CreatedBy)
                  VALUES
                    (@ProductId, @PurchaseLineId, @BatchNo, @ExpiryDate,
                     @Qty, @Qty, @UnitCost, @CreatedBy);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
                command.Parameters.Add("@PurchaseLineId", SqlDbType.BigInt).Value = invoiceLineId;
                command.Parameters.Add("@BatchNo", SqlDbType.NVarChar, 50).Value =
                    OptionalValue(line.Draft.BatchNo);
                command.Parameters.Add("@ExpiryDate", SqlDbType.Date).Value =
                    line.Draft.ExpiryDate.HasValue
                        ? (object)line.Draft.ExpiryDate.Value.Date
                        : DBNull.Value;
                AddDecimal(command, "@Qty", 18, 3, line.QtyBase);
                AddDecimal(command, "@UnitCost", 18, 4, line.UnitCostBase);
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private static void InsertStockMovement(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            long batchId,
            PreparedLine line)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.StockMovements
                    (ProductId, BatchId, MovementType, Qty, UnitCost, RefType, RefId, CreatedBy)
                  VALUES
                    (@ProductId, @BatchId, N'Purchase', @Qty, @UnitCost,
                     N'PurchaseInvoice', @InvoiceId, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
                command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
                AddDecimal(command, "@Qty", 18, 3, line.QtyBase);
                AddDecimal(command, "@UnitCost", 18, 4, line.UnitCostBase);
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateWeightedAverageCost(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            decimal incomingQty,
            decimal incomingAverage)
        {
            decimal currentQty;
            decimal currentCost;
            using (var command = new SqlCommand(
                @"SELECT p.AvgCost, ISNULL(SUM(b.QtyRemaining), 0)
                  FROM dbo.Products p WITH (UPDLOCK, HOLDLOCK)
                  LEFT JOIN dbo.Batches b WITH (HOLDLOCK) ON b.ProductId = p.Id
                  WHERE p.Id = @ProductId
                  GROUP BY p.AvgCost;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("تعذر تحديث متوسط تكلفة المنتج.");
                    }

                    currentCost = reader.GetDecimal(0);
                    currentQty = reader.GetDecimal(1);
                }
            }

            decimal totalQty = checked(currentQty + incomingQty);
            decimal weightedCost = currentQty == 0
                ? incomingAverage
                : currentCost + (incomingAverage - currentCost) * (incomingQty / totalQty);
            decimal newAverage = decimal.Round(weightedCost, 4, MidpointRounding.AwayFromZero);
            if (newAverage < 0 || newAverage > MaximumMoney)
            {
                throw new ArgumentException("متوسط تكلفة المنتج يتجاوز الحد المسموح.");
            }

            using (var command = new SqlCommand(
                @"UPDATE dbo.Products
                  SET AvgCost = @AverageCost, UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                  WHERE Id = @ProductId AND IsActive = 1;",
                connection,
                transaction))
            {
                AddDecimal(command, "@AverageCost", 18, 4, newAverage);
                command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("تعذر حفظ متوسط تكلفة المنتج.");
                }
            }
        }

        private static void UpdateSupplierBalance(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int supplierId,
            int currencyId,
            decimal amount)
        {
            using (var command = new SqlCommand(
                @"UPDATE dbo.SupplierBalances WITH (UPDLOCK, HOLDLOCK)
                  SET Balance = Balance + @Amount, UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                  WHERE SupplierId = @SupplierId AND CurrencyId = @CurrencyId;
                  IF @@ROWCOUNT = 0
                    INSERT dbo.SupplierBalances (SupplierId, CurrencyId, Balance, CreatedBy)
                    VALUES (@SupplierId, @CurrencyId, @Amount, @UpdatedBy);",
                connection,
                transaction))
            {
                AddDecimal(command, "@Amount", 18, 4, amount);
                command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@SupplierId", SqlDbType.Int).Value = supplierId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currencyId;
                command.ExecuteNonQuery();
            }
        }

        private static void InsertSupplierLedger(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            int supplierId,
            int currencyId,
            decimal amount)
        {
            if (amount <= 0)
            {
                return;
            }

            using (var command = new SqlCommand(
                @"INSERT dbo.SupplierLedger
                    (SupplierId, CurrencyId, Debit, Credit, RefType, RefId, Description, CreatedBy)
                  VALUES
                    (@SupplierId, @CurrencyId, 0, @Credit, N'PurchaseInvoice',
                     @InvoiceId, N'فاتورة شراء', @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@SupplierId", SqlDbType.Int).Value = supplierId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currencyId;
                AddDecimal(command, "@Credit", 18, 4, amount);
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void InsertInvoiceAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            string invoiceNumber,
            PurchaseInvoiceDraft draft,
            CurrencyDetails currency,
            decimal total,
            decimal totalBase,
            int lineCount)
        {
            string newValues = string.Format(
                CultureInfo.InvariantCulture,
                "{{\"InvoiceNo\":\"{0}\",\"SupplierId\":{1},\"Currency\":\"{2}\",\"RateUsed\":{3},\"Total\":{4},\"TotalBase\":{5},\"LineCount\":{6}}}",
                EscapeJson(invoiceNumber),
                draft.SupplierId,
                EscapeJson(currency.Code),
                currency.RateToBase,
                total,
                totalBase,
                lineCount);
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, NewValues)
                  VALUES
                    (@UserId, N'PurchaseInvoicePosted', N'PurchaseInvoices', @EntityId, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    invoiceId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static string GetNextInvoiceNumber(SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand("dbo.usp_GetNextDocumentNumber", connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = "PurchaseInvoice";
                SqlParameter number = command.Parameters.Add("@Number", SqlDbType.BigInt);
                number.Direction = ParameterDirection.Output;
                SqlParameter formatted = command.Parameters.Add("@Formatted", SqlDbType.NVarChar, 50);
                formatted.Direction = ParameterDirection.Output;
                command.ExecuteNonQuery();
                if (formatted.Value == null || formatted.Value == DBNull.Value)
                {
                    throw new InvalidOperationException("تعذر إنشاء رقم فاتورة الشراء.");
                }

                return Convert.ToString(formatted.Value, CultureInfo.InvariantCulture);
            }
        }

        private static void EnsureActiveSupplier(
            SqlConnection connection,
            SqlTransaction transaction,
            int supplierId)
        {
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Suppliers WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id AND IsActive = 1;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = supplierId;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException("المورد المحدد غير موجود أو غير نشط.");
                }
            }
        }

        private static void Validate(PurchaseInvoiceDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            draft.SupplierInvoiceNo = CleanOptional(draft.SupplierInvoiceNo);
            draft.Notes = CleanOptional(draft.Notes);
            if (draft.SupplierId <= 0 || draft.CurrencyId <= 0)
            {
                throw new ArgumentException("اختر المورد وعملة الفاتورة.");
            }

            if (draft.InvoiceDate == DateTime.MinValue || draft.InvoiceDate.Date > DateTime.Today)
            {
                throw new ArgumentException("تاريخ الفاتورة غير صالح أو يقع في المستقبل.");
            }

            if ((draft.SupplierInvoiceNo != null && draft.SupplierInvoiceNo.Length > 50) ||
                (draft.Notes != null && draft.Notes.Length > 500))
            {
                throw new ArgumentException("تجاوز رقم فاتورة المورد أو الملاحظات الحد الأقصى.");
            }

            if (draft.Lines == null || draft.Lines.Count == 0 || draft.Lines.Count > 250)
            {
                throw new ArgumentException("أضف من منتج واحد إلى 250 منتجًا للفاتورة.");
            }

            var usedProductUnits = new HashSet<int>();
            foreach (PurchaseInvoiceLineDraft line in draft.Lines)
            {
                if (line == null || line.ProductUnitId <= 0 ||
                    !usedProductUnits.Add(line.ProductUnitId))
                {
                    throw new ArgumentException("اختر منتجًا صالحًا، ولا تكرر وحدة المنتج في الفاتورة.");
                }

                if (line.Quantity <= 0 || line.Quantity > MaximumQuantity ||
                    decimal.Round(line.Quantity, 3) != line.Quantity)
                {
                    throw new ArgumentException("الكمية يجب أن تكون موجبة وبحد أقصى 3 خانات عشرية.");
                }

                if (line.UnitPrice < 0 || line.UnitPrice > MaximumMoney ||
                    decimal.Round(line.UnitPrice, 4) != line.UnitPrice)
                {
                    throw new ArgumentException("سعر الوحدة يجب أن يكون غير سالب وبحد أقصى 4 خانات عشرية.");
                }

                line.BatchNo = CleanOptional(line.BatchNo);
                if (line.BatchNo != null && line.BatchNo.Length > 50)
                {
                    throw new ArgumentException("رقم الدفعة لا يتجاوز 50 حرفًا.");
                }
            }
        }

        private static decimal RoundToCurrency(decimal amount, int decimalPlaces, string roundingMode)
        {
            decimal factor = 1m;
            for (int i = 0; i < decimalPlaces; i++)
            {
                factor *= 10m;
            }

            switch (roundingMode)
            {
                case "AwayFromZero":
                    return decimal.Round(amount, decimalPlaces, MidpointRounding.AwayFromZero);
                case "ToEven":
                    return decimal.Round(amount, decimalPlaces, MidpointRounding.ToEven);
                case "ToZero":
                    return decimal.Truncate(amount * factor) / factor;
                case "ToPositiveInfinity":
                    return decimal.Ceiling(amount * factor) / factor;
                case "ToNegativeInfinity":
                    return decimal.Floor(amount * factor) / factor;
                default:
                    throw new InvalidOperationException("طريقة تقريب العملة غير مدعومة: " + roundingMode);
            }
        }

        private static string CleanOptional(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static object OptionalValue(string value)
        {
            return value == null ? (object)DBNull.Value : value;
        }

        private static void AddDecimal(
            SqlCommand command,
            string name,
            byte precision,
            byte scale,
            decimal value)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value;
        }

        private static string EscapeJson(string value)
        {
            var result = new StringBuilder(value ?? string.Empty);
            return result
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .ToString();
        }

        private static void RequireInvoicePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Purchases.Invoice"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة فواتير المشتريات.");
            }
        }
    }
}
