using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace GroceryPOS
{
    internal sealed class CategoryRecord
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
    }

    internal sealed class ProductUnitRecord
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal FactorToBase { get; set; }
        public string Barcode { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public bool IsDefaultSaleUnit { get; set; }
        public bool IsActive { get; set; }
    }

    internal sealed class ProductUnitDraft
    {
        public int UnitId { get; set; }
        public decimal FactorToBase { get; set; }
        public string Barcode { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public bool IsDefaultSaleUnit { get; set; }
        public bool IsActive { get; set; }
    }

    internal static class CategoriesService
    {
        internal static List<CategoryRecord> Load(AuthenticatedUser user)
        {
            RequireManagePermission(user);
            var categories = new List<CategoryRecord>();

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT c.Id, c.ParentId, c.Name, parent.Name, c.IsActive,
                         (SELECT COUNT(*) FROM dbo.Products p WHERE p.CategoryId = c.Id) AS ProductCount
                  FROM dbo.Categories c
                  LEFT JOIN dbo.Categories parent ON parent.Id = c.ParentId
                  ORDER BY c.IsActive DESC, parent.Name, c.Name;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    categories.Add(new CategoryRecord
                    {
                        Id = reader.GetInt32(0),
                        ParentId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                        Name = reader.GetString(2),
                        ParentName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        IsActive = reader.GetBoolean(4),
                        ProductCount = reader.GetInt32(5)
                    });
                }
            }

            return categories;
        }

        internal static void Save(
            AuthenticatedUser user,
            int? categoryId,
            string name,
            int? parentId)
        {
            RequireManagePermission(user);
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0 || name.Length > 150)
            {
                throw new ArgumentException("اسم التصنيف مطلوب ولا يتجاوز 150 حرفًا.");
            }

            if (categoryId.HasValue && parentId == categoryId)
            {
                throw new ArgumentException("لا يمكن جعل التصنيف تابعًا لنفسه.");
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                CategoryRecord previous = categoryId.HasValue
                    ? LoadOne(connection, transaction, categoryId.Value, true)
                    : null;
                if (categoryId.HasValue && previous == null)
                {
                    throw new InvalidOperationException("التصنيف المطلوب تعديله غير موجود.");
                }

                if (parentId.HasValue)
                {
                    CategoryRecord parent = LoadOne(connection, transaction, parentId.Value, true);
                    bool keepingInactiveParent =
                        previous != null && previous.ParentId == parentId && parent != null;
                    if (parent == null || (!parent.IsActive && !keepingInactiveParent))
                    {
                        throw new InvalidOperationException("التصنيف الرئيسي غير موجود أو غير نشط.");
                    }

                    if (categoryId.HasValue && IsDescendant(connection, transaction, parentId.Value, categoryId.Value))
                    {
                        throw new InvalidOperationException("لا يمكن نقل التصنيف إلى أحد تصنيفاته الفرعية.");
                    }
                }

                EnsureNameAvailable(connection, transaction, categoryId, name, parentId);

                int savedId;
                if (previous == null)
                {
                    using (var command = new SqlCommand(
                        @"INSERT dbo.Categories (ParentId, Name, CreatedBy, IsActive)
                          VALUES (@ParentId, @Name, @UserId, 1);
                          SELECT CAST(SCOPE_IDENTITY() AS int);",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@ParentId", SqlDbType.Int).Value =
                            parentId.HasValue ? (object)parentId.Value : DBNull.Value;
                        command.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = name;
                        command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                        savedId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                else
                {
                    using (var command = new SqlCommand(
                        @"UPDATE dbo.Categories
                          SET ParentId = @ParentId, Name = @Name,
                              UpdatedAt = SYSDATETIME(), UpdatedBy = @UserId
                          WHERE Id = @Id;",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@ParentId", SqlDbType.Int).Value =
                            parentId.HasValue ? (object)parentId.Value : DBNull.Value;
                        command.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = name;
                        command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = categoryId.Value;
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("تعذر حفظ تعديلات التصنيف.");
                        }

                        savedId = categoryId.Value;
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    savedId,
                    previous == null ? null : CategoryJson(previous),
                    CategoryJson(name, parentId, true));
                transaction.Commit();
            }
        }

        internal static void SetActive(AuthenticatedUser user, int categoryId, bool isActive)
        {
            RequireManagePermission(user);
            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                CategoryRecord previous = LoadOne(connection, transaction, categoryId, true);
                if (previous == null)
                {
                    throw new InvalidOperationException("التصنيف غير موجود.");
                }

                if (previous.IsActive == isActive)
                {
                    transaction.Commit();
                    return;
                }

                if (isActive && previous.ParentId.HasValue)
                {
                    CategoryRecord parent = LoadOne(connection, transaction, previous.ParentId.Value, true);
                    if (parent == null || !parent.IsActive)
                    {
                        throw new InvalidOperationException("فعّل التصنيف الرئيسي أولًا قبل تفعيل هذا التصنيف.");
                    }
                }

                if (!isActive)
                {
                    using (var command = new SqlCommand(
                        "SELECT COUNT(*) FROM dbo.Categories WHERE ParentId = @Id AND IsActive = 1;",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = categoryId;
                        if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                        {
                            throw new InvalidOperationException(
                                "عطّل التصنيفات الفرعية النشطة أولًا قبل تعطيل هذا التصنيف.");
                        }
                    }
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Categories
                      SET IsActive = @IsActive, UpdatedAt = SYSDATETIME(), UpdatedBy = @UserId
                      WHERE Id = @Id;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = categoryId;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تعذر تغيير حالة التصنيف.");
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    categoryId,
                    CategoryJson(previous),
                    CategoryJson(previous.Name, previous.ParentId, isActive));
                transaction.Commit();
            }
        }

        private static void RequireManagePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Categories.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة التصنيفات.");
            }
        }

        private static CategoryRecord LoadOne(
            SqlConnection connection,
            SqlTransaction transaction,
            int id,
            bool lockRow)
        {
            string hint = lockRow ? " WITH (UPDLOCK, HOLDLOCK)" : string.Empty;
            using (var command = new SqlCommand(
                @"SELECT c.Id, c.ParentId, c.Name, parent.Name, c.IsActive,
                         (SELECT COUNT(*) FROM dbo.Products p WHERE p.CategoryId = c.Id)
                  FROM dbo.Categories c" + hint + @"
                  LEFT JOIN dbo.Categories parent ON parent.Id = c.ParentId
                  WHERE c.Id = @Id;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new CategoryRecord
                    {
                        Id = reader.GetInt32(0),
                        ParentId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                        Name = reader.GetString(2),
                        ParentName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        IsActive = reader.GetBoolean(4),
                        ProductCount = reader.GetInt32(5)
                    };
                }
            }
        }

        private static bool IsDescendant(
            SqlConnection connection,
            SqlTransaction transaction,
            int potentialChildId,
            int ancestorId)
        {
            using (var command = new SqlCommand(
                @"WITH Ancestors AS
                  (
                      SELECT Id, ParentId FROM dbo.Categories WHERE Id = @ChildId
                      UNION ALL
                      SELECT c.Id, c.ParentId
                      FROM dbo.Categories c
                      INNER JOIN Ancestors a ON a.ParentId = c.Id
                  )
                  SELECT COUNT(*) FROM Ancestors WHERE Id = @AncestorId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ChildId", SqlDbType.Int).Value = potentialChildId;
                command.Parameters.Add("@AncestorId", SqlDbType.Int).Value = ancestorId;
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void EnsureNameAvailable(
            SqlConnection connection,
            SqlTransaction transaction,
            int? categoryId,
            string name,
            int? parentId)
        {
            using (var command = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.Categories WITH (UPDLOCK, HOLDLOCK)
                  WHERE Name = @Name
                    AND ((ParentId = @ParentId) OR (ParentId IS NULL AND @ParentId IS NULL))
                    AND (@Id IS NULL OR Id <> @Id);",
                connection,
                transaction))
            {
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = name;
                command.Parameters.Add("@ParentId", SqlDbType.Int).Value =
                    parentId.HasValue ? (object)parentId.Value : DBNull.Value;
                command.Parameters.Add("@Id", SqlDbType.Int).Value =
                    categoryId.HasValue ? (object)categoryId.Value : DBNull.Value;
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    throw new InvalidOperationException("يوجد تصنيف بالاسم نفسه ضمن هذا المستوى.");
                }
            }
        }

        private static void InsertAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int categoryId,
            string oldValues,
            string newValues)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, OldValues, NewValues)
                  VALUES
                    (@UserId, @Action, N'Categories', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value =
                    oldValues == null ? "CategoryCreated" : "CategoryUpdated";
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    categoryId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value =
                    oldValues == null ? (object)DBNull.Value : oldValues;
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static string CategoryJson(CategoryRecord category)
        {
            return CategoryJson(category.Name, category.ParentId, category.IsActive);
        }

        private static string CategoryJson(string name, int? parentId, bool isActive)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{\"Name\":\"{0}\",\"ParentId\":{1},\"IsActive\":{2}}}",
                EscapeJson(name),
                parentId.HasValue ? parentId.Value.ToString(CultureInfo.InvariantCulture) : "null",
                isActive ? "true" : "false");
        }

        private static string EscapeJson(string value)
        {
            var escaped = new StringBuilder();
            foreach (char character in value)
            {
                switch (character)
                {
                    case '"': escaped.Append("\\\""); break;
                    case '\\': escaped.Append("\\\\"); break;
                    case '\b': escaped.Append("\\b"); break;
                    case '\f': escaped.Append("\\f"); break;
                    case '\n': escaped.Append("\\n"); break;
                    case '\r': escaped.Append("\\r"); break;
                    case '\t': escaped.Append("\\t"); break;
                    default:
                        if (character < 0x20)
                        {
                            escaped.Append("\\u").Append(((int)character).ToString("x4"));
                        }
                        else
                        {
                            escaped.Append(character);
                        }

                        break;
                }
            }

            return escaped.ToString();
        }
    }

    internal static class ProductUnitsService
    {
        internal static List<ProductUnitRecord> Load(AuthenticatedUser user, int productId)
        {
            RequireViewPermission(user);
            var rows = new List<ProductUnitRecord>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT pu.ProductId, pu.UnitId, u.Name, pu.FactorToBase, pu.Barcode,
                         pu.RetailPrice, pu.WholesalePrice, pu.IsDefaultSaleUnit, pu.IsActive
                  FROM dbo.ProductUnits pu
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  WHERE pu.ProductId = @ProductId
                  ORDER BY pu.FactorToBase, u.Name;",
                connection))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(new ProductUnitRecord
                        {
                            ProductId = reader.GetInt32(0),
                            UnitId = reader.GetInt32(1),
                            UnitName = reader.GetString(2),
                            FactorToBase = reader.GetDecimal(3),
                            Barcode = reader.IsDBNull(4) ? null : reader.GetString(4),
                            RetailPrice = reader.GetDecimal(5),
                            WholesalePrice = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                            IsDefaultSaleUnit = reader.GetBoolean(7),
                            IsActive = reader.GetBoolean(8)
                        });
                    }
                }
            }

            return rows;
        }

        internal static List<CatalogOption> LoadChoices(int productId)
        {
            var choices = new List<CatalogOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT u.Id, u.Name
                  FROM dbo.Units u
                  WHERE u.IsActive = 1
                     OR EXISTS (SELECT 1 FROM dbo.ProductUnits pu
                                WHERE pu.UnitId = u.Id AND pu.ProductId = @ProductId)
                  ORDER BY u.IsActive DESC, u.Name;",
                connection))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        choices.Add(new CatalogOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            if (choices.Count == 0)
            {
                throw new InvalidOperationException("لا توجد وحدات معرفة في قاعدة البيانات.");
            }

            return choices;
        }

        internal static void Save(
            AuthenticatedUser user,
            int productId,
            IList<ProductUnitDraft> drafts)
        {
            if (user == null || !user.HasPermission("Products.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة وحدات المنتجات.");
            }

            if (drafts == null || drafts.Count == 0)
            {
                throw new ArgumentException("يجب أن يحتوي المنتج على وحدته الأساسية على الأقل.");
            }

            ValidateDrafts(drafts);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                bool productActive;
                int baseUnitId;
                using (var command = new SqlCommand(
                    @"SELECT BaseUnitId, IsActive
                      FROM dbo.Products WITH (UPDLOCK, HOLDLOCK)
                      WHERE Id = @Id;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = productId;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("المنتج لم يعد موجودًا.");
                        }

                        baseUnitId = reader.GetInt32(0);
                        productActive = reader.GetBoolean(1);
                    }
                }

                var existing = LoadExistingUnits(connection, transaction, productId);
                ProductUnitDraft baseUnit = FindDraft(drafts, baseUnitId);
                if (baseUnit == null || baseUnit.FactorToBase != 1m ||
                    baseUnit.IsActive != productActive)
                {
                    throw new ArgumentException(
                        "يجب تضمين الوحدة الأساسية بمعامل تحويل 1، وتطابق حالة نشاط المنتج.");
                }

                if (productActive)
                {
                    int defaultCount = 0;
                    foreach (ProductUnitDraft draft in drafts)
                    {
                        if (draft.IsActive && draft.IsDefaultSaleUnit)
                        {
                            defaultCount++;
                        }
                    }

                    if (defaultCount != 1)
                    {
                        throw new ArgumentException("حدد وحدة بيع افتراضية نشطة واحدة فقط.");
                    }
                }
                else
                {
                    foreach (ProductUnitDraft draft in drafts)
                    {
                        if (draft.IsActive || draft.IsDefaultSaleUnit)
                        {
                            throw new ArgumentException("وحدات المنتج غير النشط يجب أن تبقى غير نشطة.");
                        }
                    }
                }

                foreach (ProductUnitDraft draft in drafts)
                {
                    bool existingForProduct = existing.ContainsKey(draft.UnitId);
                    EnsureUnitAvailable(
                        connection,
                        transaction,
                        draft.UnitId,
                        existingForProduct,
                        draft.IsActive);
                    if (draft.IsDefaultSaleUnit && !draft.IsActive)
                    {
                        throw new ArgumentException("لا يمكن جعل وحدة غير نشطة وحدة بيع افتراضية.");
                    }
                }

                EnsureUniqueBarcodes(drafts);
                EnsurePricesMayChange(user, existing, drafts);
                SaveUnitRows(connection, transaction, user.Id, productId, drafts);
                DeactivateRemovedUnits(connection, transaction, user.Id, productId, drafts);
                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    productId,
                    Serialize(existing.Values),
                    Serialize(drafts));
                transaction.Commit();
            }
        }

        private static void ValidateDrafts(IList<ProductUnitDraft> drafts)
        {
            var seenUnits = new HashSet<int>();
            foreach (ProductUnitDraft draft in drafts)
            {
                if (draft == null || draft.UnitId <= 0 || !seenUnits.Add(draft.UnitId))
                {
                    throw new ArgumentException("اختر وحدة واحدة فقط لكل صف واحذف الصفوف المكررة.");
                }

                draft.Barcode = string.IsNullOrWhiteSpace(draft.Barcode)
                    ? null
                    : draft.Barcode.Trim();
                if (draft.Barcode != null && draft.Barcode.Length > 50)
                {
                    throw new ArgumentException("الباركود لا يتجاوز 50 حرفًا.");
                }

                if (draft.FactorToBase <= 0 || draft.FactorToBase > 999999999999.999999m ||
                    decimal.Round(draft.FactorToBase, 6) != draft.FactorToBase)
                {
                    throw new ArgumentException("معامل التحويل موجب ويقبل 6 خانات عشرية كحد أقصى.");
                }

                if (draft.RetailPrice < 0 || draft.RetailPrice > 99999999999999.9999m ||
                    (draft.WholesalePrice.HasValue &&
                     (draft.WholesalePrice.Value < 0 || draft.WholesalePrice.Value > 99999999999999.9999m)))
                {
                    throw new ArgumentException("أسعار الوحدة يجب أن تكون غير سالبة وضمن حدود قاعدة البيانات.");
                }

                if (decimal.Round(draft.RetailPrice, 4) != draft.RetailPrice ||
                    (draft.WholesalePrice.HasValue &&
                     decimal.Round(draft.WholesalePrice.Value, 4) != draft.WholesalePrice.Value))
                {
                    throw new ArgumentException("أسعار الوحدات تقبل 4 خانات عشرية كحد أقصى.");
                }
            }
        }

        private static ProductUnitDraft FindDraft(IList<ProductUnitDraft> drafts, int unitId)
        {
            foreach (ProductUnitDraft draft in drafts)
            {
                if (draft.UnitId == unitId)
                {
                    return draft;
                }
            }

            return null;
        }

        private static Dictionary<int, ProductUnitRecord> LoadExistingUnits(
            SqlConnection connection,
            SqlTransaction transaction,
            int productId)
        {
            var rows = new Dictionary<int, ProductUnitRecord>();
            using (var command = new SqlCommand(
                @"SELECT pu.UnitId, u.Name, pu.FactorToBase, pu.Barcode,
                         pu.RetailPrice, pu.WholesalePrice, pu.IsDefaultSaleUnit, pu.IsActive
                  FROM dbo.ProductUnits pu WITH (UPDLOCK, HOLDLOCK)
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  WHERE pu.ProductId = @ProductId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new ProductUnitRecord
                        {
                            ProductId = productId,
                            UnitId = reader.GetInt32(0),
                            UnitName = reader.GetString(1),
                            FactorToBase = reader.GetDecimal(2),
                            Barcode = reader.IsDBNull(3) ? null : reader.GetString(3),
                            RetailPrice = reader.GetDecimal(4),
                            WholesalePrice = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                            IsDefaultSaleUnit = reader.GetBoolean(6),
                            IsActive = reader.GetBoolean(7)
                        };
                        rows.Add(row.UnitId, row);
                    }
                }
            }

            return rows;
        }

        private static void EnsureUnitAvailable(
            SqlConnection connection,
            SqlTransaction transaction,
            int unitId,
            bool alreadyAssigned,
            bool productUnitActive)
        {
            using (var command = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.Units
                  WHERE Id = @Id
                    AND (IsActive = 1 OR (@AlreadyAssigned = 1 AND @ProductUnitActive = 0));",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = unitId;
                command.Parameters.Add("@AlreadyAssigned", SqlDbType.Bit).Value = alreadyAssigned;
                command.Parameters.Add("@ProductUnitActive", SqlDbType.Bit).Value = productUnitActive;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException("الوحدة المحددة غير موجودة أو غير نشطة.");
                }
            }
        }

        private static void EnsureUniqueBarcodes(IList<ProductUnitDraft> drafts)
        {
            var barcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ProductUnitDraft draft in drafts)
            {
                if (draft.Barcode != null && !barcodes.Add(draft.Barcode))
                {
                    throw new ArgumentException("لا يمكن تكرار الباركود داخل المنتج نفسه.");
                }
            }
        }

        private static void EnsurePricesMayChange(
            AuthenticatedUser user,
            IDictionary<int, ProductUnitRecord> existing,
            IList<ProductUnitDraft> drafts)
        {
            foreach (ProductUnitDraft draft in drafts)
            {
                ProductUnitRecord previous;
                if (!existing.TryGetValue(draft.UnitId, out previous))
                {
                    if ((draft.RetailPrice != 0m || draft.WholesalePrice.HasValue) &&
                        !user.HasPermission("Products.EditPrice"))
                    {
                        throw new UnauthorizedAccessException("تحديد أسعار البيع يتطلب صلاحية Products.EditPrice.");
                    }
                }
                else if ((previous.RetailPrice != draft.RetailPrice ||
                          previous.WholesalePrice != draft.WholesalePrice) &&
                         !user.HasPermission("Products.EditPrice"))
                {
                    throw new UnauthorizedAccessException("تعديل أسعار البيع يتطلب صلاحية Products.EditPrice.");
                }
            }
        }

        private static void SaveUnitRows(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            IList<ProductUnitDraft> drafts)
        {
            foreach (ProductUnitDraft draft in drafts)
            {
                using (var command = new SqlCommand(
                    @"IF EXISTS
                        (SELECT 1 FROM dbo.ProductUnits WHERE ProductId = @ProductId AND UnitId = @UnitId)
                      BEGIN
                        UPDATE dbo.ProductUnits
                        SET FactorToBase = @FactorToBase, Barcode = @Barcode,
                            RetailPrice = @RetailPrice, WholesalePrice = @WholesalePrice,
                            IsDefaultSaleUnit = @IsDefaultSaleUnit, IsActive = @IsActive,
                            UpdatedAt = SYSDATETIME(), UpdatedBy = @UserId
                        WHERE ProductId = @ProductId AND UnitId = @UnitId;
                      END
                      ELSE
                      BEGIN
                        INSERT dbo.ProductUnits
                            (ProductId, UnitId, FactorToBase, Barcode, RetailPrice,
                             WholesalePrice, IsDefaultSaleUnit, IsActive, CreatedBy)
                        VALUES
                            (@ProductId, @UnitId, @FactorToBase, @Barcode, @RetailPrice,
                             @WholesalePrice, @IsDefaultSaleUnit, @IsActive, @UserId);
                      END",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                    command.Parameters.Add("@UnitId", SqlDbType.Int).Value = draft.UnitId;
                    AddDecimal(command, "@FactorToBase", draft.FactorToBase, 18, 6);
                    command.Parameters.Add("@Barcode", SqlDbType.NVarChar, 50).Value =
                        draft.Barcode == null ? (object)DBNull.Value : draft.Barcode;
                    AddDecimal(command, "@RetailPrice", draft.RetailPrice, 18, 4);
                    AddNullableDecimal(command, "@WholesalePrice", draft.WholesalePrice, 18, 4);
                    command.Parameters.Add("@IsDefaultSaleUnit", SqlDbType.Bit).Value = draft.IsDefaultSaleUnit;
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = draft.IsActive;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void DeactivateRemovedUnits(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            IList<ProductUnitDraft> drafts)
        {
            var retained = new HashSet<int>();
            foreach (ProductUnitDraft draft in drafts)
            {
                retained.Add(draft.UnitId);
            }

            foreach (int unitId in LoadExistingUnitIds(connection, transaction, productId))
            {
                if (!retained.Contains(unitId))
                {
                    using (var command = new SqlCommand(
                        @"UPDATE dbo.ProductUnits
                          SET IsActive = 0, IsDefaultSaleUnit = 0,
                              UpdatedAt = SYSDATETIME(), UpdatedBy = @UserId
                          WHERE ProductId = @ProductId AND UnitId = @UnitId;",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                        command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                        command.Parameters.Add("@UnitId", SqlDbType.Int).Value = unitId;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static List<int> LoadExistingUnitIds(
            SqlConnection connection,
            SqlTransaction transaction,
            int productId)
        {
            var ids = new List<int>();
            using (var command = new SqlCommand(
                "SELECT UnitId FROM dbo.ProductUnits WHERE ProductId = @ProductId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt32(0));
                    }
                }
            }

            return ids;
        }

        private static void AddDecimal(
            SqlCommand command,
            string name,
            decimal value,
            byte precision,
            byte scale)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value;
        }

        private static void AddNullableDecimal(
            SqlCommand command,
            string name,
            decimal? value,
            byte precision,
            byte scale)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void InsertAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            string oldValues,
            string newValues)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, OldValues, NewValues)
                  VALUES
                    (@UserId, N'ProductUnitsUpdated', N'Products', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    productId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value = oldValues;
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static string Serialize(IEnumerable<ProductUnitRecord> rows)
        {
            var output = new List<ProductUnitDraft>();
            foreach (ProductUnitRecord row in rows)
            {
                output.Add(new ProductUnitDraft
                {
                    UnitId = row.UnitId,
                    FactorToBase = row.FactorToBase,
                    Barcode = row.Barcode,
                    RetailPrice = row.RetailPrice,
                    WholesalePrice = row.WholesalePrice,
                    IsDefaultSaleUnit = row.IsDefaultSaleUnit,
                    IsActive = row.IsActive
                });
            }

            return Serialize(output);
        }

        private static string Serialize(IEnumerable<ProductUnitDraft> rows)
        {
            var json = new StringBuilder("[");
            bool first = true;
            foreach (ProductUnitDraft row in rows)
            {
                if (!first)
                {
                    json.Append(',');
                }

                json.Append("{\"UnitId\":").Append(row.UnitId.ToString(CultureInfo.InvariantCulture))
                    .Append(",\"FactorToBase\":").Append(row.FactorToBase.ToString(CultureInfo.InvariantCulture))
                    .Append(",\"Barcode\":");
                if (row.Barcode == null)
                {
                    json.Append("null");
                }
                else
                {
                    json.Append('"').Append(EscapeJson(row.Barcode)).Append('"');
                }

                json.Append(",\"RetailPrice\":").Append(row.RetailPrice.ToString(CultureInfo.InvariantCulture))
                    .Append(",\"WholesalePrice\":");
                json.Append(row.WholesalePrice.HasValue
                    ? row.WholesalePrice.Value.ToString(CultureInfo.InvariantCulture)
                    : "null");
                json.Append(",\"IsDefaultSaleUnit\":").Append(row.IsDefaultSaleUnit ? "true" : "false")
                    .Append(",\"IsActive\":").Append(row.IsActive ? "true" : "false").Append('}');
                first = false;
            }

            return json.Append(']').ToString();
        }

        private static string EscapeJson(string value)
        {
            var escaped = new StringBuilder();
            foreach (char character in value)
            {
                switch (character)
                {
                    case '"': escaped.Append("\\\""); break;
                    case '\\': escaped.Append("\\\\"); break;
                    case '\b': escaped.Append("\\b"); break;
                    case '\f': escaped.Append("\\f"); break;
                    case '\n': escaped.Append("\\n"); break;
                    case '\r': escaped.Append("\\r"); break;
                    case '\t': escaped.Append("\\t"); break;
                    default:
                        if (character < 0x20)
                        {
                            escaped.Append("\\u").Append(((int)character).ToString("x4"));
                        }
                        else
                        {
                            escaped.Append(character);
                        }

                        break;
                }
            }

            return escaped.ToString();
        }

        private static void RequireViewPermission(AuthenticatedUser user)
        {
            if (user == null ||
                (!user.HasPermission("Products.View") && !user.HasPermission("Products.Manage")))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية عرض وحدات المنتجات.");
            }
        }
    }
}
