using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace GroceryPOS
{
    internal sealed class CatalogOption
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    internal sealed class ProductRecord
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int BaseUnitId { get; set; }
        public string BaseUnitName { get; set; }
        public string Barcode { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public decimal MinStock { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal AverageCost { get; set; }
        public bool TracksExpiry { get; set; }
        public bool IsActive { get; set; }
    }

    internal sealed class ProductDraft
    {
        internal int? Id { get; set; }
        internal string Sku { get; set; }
        internal string Name { get; set; }
        internal int CategoryId { get; set; }
        internal int BaseUnitId { get; set; }
        internal string Barcode { get; set; }
        internal decimal RetailPrice { get; set; }
        internal decimal? WholesalePrice { get; set; }
        internal decimal MinStock { get; set; }
        internal bool TracksExpiry { get; set; }
        internal bool IsActive { get; set; }
    }

    internal static class ProductsService
    {
        internal static List<ProductRecord> Search(AuthenticatedUser user, string searchText)
        {
            RequireCatalogAccess(user);
            var products = new List<ProductRecord>();
            string search = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT p.Id, p.Sku, p.Name, p.CategoryId, c.Name,
                         p.BaseUnitId, u.Name, pu.Barcode, pu.RetailPrice,
                         pu.WholesalePrice, p.MinStock, stock.TotalQtyBase,
                         p.AvgCost, p.TracksExpiry, p.IsActive
                  FROM dbo.Products AS p
                  INNER JOIN dbo.Categories AS c ON c.Id = p.CategoryId
                  INNER JOIN dbo.Units AS u ON u.Id = p.BaseUnitId
                  LEFT JOIN dbo.ProductUnits AS pu
                    ON pu.ProductId = p.Id AND pu.UnitId = p.BaseUnitId
                  LEFT JOIN dbo.vw_ProductStock AS stock ON stock.ProductId = p.Id
                  WHERE @Search IS NULL
                     OR p.Name LIKE N'%' + @Search + N'%'
                     OR p.Sku LIKE N'%' + @Search + N'%'
                     OR EXISTS
                        (SELECT 1
                         FROM dbo.ProductUnits AS searchUnit
                         WHERE searchUnit.ProductId = p.Id
                           AND searchUnit.Barcode LIKE N'%' + @Search + N'%')
                  ORDER BY p.IsActive DESC, p.Name, p.Id;",
                connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value =
                    search == null ? (object)DBNull.Value : search;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new ProductRecord
                        {
                            Id = reader.GetInt32(0),
                            Sku = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Name = reader.GetString(2),
                            CategoryId = reader.GetInt32(3),
                            CategoryName = reader.GetString(4),
                            BaseUnitId = reader.GetInt32(5),
                            BaseUnitName = reader.GetString(6),
                            Barcode = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                            RetailPrice = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                            WholesalePrice = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                            MinStock = reader.GetDecimal(10),
                            StockQuantity = reader.IsDBNull(11) ? 0m : reader.GetDecimal(11),
                            AverageCost = reader.GetDecimal(12),
                            TracksExpiry = reader.GetBoolean(13),
                            IsActive = reader.GetBoolean(14)
                        });
                    }
                }
            }

            return products;
        }

        internal static List<CatalogOption> LoadCategories(int? selectedId)
        {
            return LoadOptions(
                @"SELECT Id, Name FROM dbo.Categories
                  WHERE IsActive = 1 OR Id = @SelectedId
                  ORDER BY IsActive DESC, Name;",
                selectedId,
                "التصنيفات");
        }

        internal static List<CatalogOption> LoadUnits(int? selectedId)
        {
            return LoadOptions(
                @"SELECT Id, Name FROM dbo.Units
                  WHERE IsActive = 1 OR Id = @SelectedId
                  ORDER BY IsActive DESC, Name;",
                selectedId,
                "الوحدات");
        }

        internal static void Save(AuthenticatedUser user, ProductDraft draft)
        {
            if (user == null || !user.HasPermission("Products.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة المنتجات.");
            }

            Validate(draft);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                ProductRecord previous = draft.Id.HasValue
                    ? LoadExisting(connection, transaction, draft.Id.Value)
                    : null;

                if (draft.Id.HasValue && previous == null)
                {
                    throw new InvalidOperationException("المنتج المطلوب تعديله لم يعد موجودًا.");
                }

                EnsureActiveLookup(
                    connection,
                    transaction,
                    "dbo.Categories",
                    draft.CategoryId,
                    "التصنيف",
                    previous != null && previous.CategoryId == draft.CategoryId);
                EnsureActiveLookup(
                    connection,
                    transaction,
                    "dbo.Units",
                    draft.BaseUnitId,
                    "الوحدة الأساسية",
                    previous != null && previous.BaseUnitId == draft.BaseUnitId);

                if (previous != null && previous.BaseUnitId != draft.BaseUnitId)
                {
                    throw new InvalidOperationException(
                        "لا يمكن تغيير الوحدة الأساسية لمنتج موجود؛ أنشئ منتجًا جديدًا لتجنب تغيير معنى كميات المخزون.");
                }

                if (previous != null &&
                    (previous.RetailPrice != draft.RetailPrice ||
                     previous.WholesalePrice != draft.WholesalePrice) &&
                    !user.HasPermission("Products.EditPrice"))
                {
                    throw new UnauthorizedAccessException("تعديل أسعار البيع يتطلب صلاحية Products.EditPrice.");
                }

                int productId = previous == null
                    ? InsertProduct(connection, transaction, user.Id, draft)
                    : UpdateProduct(connection, transaction, user.Id, draft);

                SaveBaseUnit(connection, transaction, user.Id, productId, draft);
                InsertAudit(connection, transaction, user.Id, productId, previous, draft);
                transaction.Commit();
            }
        }

        private static List<CatalogOption> LoadOptions(
            string query,
            int? selectedId,
            string label)
        {
            var options = new List<CatalogOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@SelectedId", SqlDbType.Int).Value =
                    selectedId.HasValue ? (object)selectedId.Value : DBNull.Value;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        options.Add(new CatalogOption
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            if (options.Count == 0)
            {
                throw new InvalidOperationException("لا توجد " + label + " نشطة في قاعدة البيانات.");
            }

            return options;
        }

        private static void RequireCatalogAccess(AuthenticatedUser user)
        {
            if (user == null ||
                (!user.HasPermission("Products.View") && !user.HasPermission("Products.Manage")))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية عرض المنتجات.");
            }
        }

        private static void Validate(ProductDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            draft.Name = (draft.Name ?? string.Empty).Trim();
            draft.Sku = CleanOptional(draft.Sku);
            draft.Barcode = CleanOptional(draft.Barcode);

            if (draft.Name.Length == 0 || draft.Name.Length > 200)
            {
                throw new ArgumentException("اسم المنتج مطلوب ولا يتجاوز 200 حرف.");
            }

            if (draft.Sku != null && draft.Sku.Length > 50)
            {
                throw new ArgumentException("رمز SKU لا يتجاوز 50 حرفًا.");
            }

            if (draft.Barcode != null && draft.Barcode.Length > 50)
            {
                throw new ArgumentException("الباركود لا يتجاوز 50 حرفًا.");
            }

            if (draft.CategoryId <= 0 || draft.BaseUnitId <= 0)
            {
                throw new ArgumentException("اختر التصنيف والوحدة الأساسية.");
            }

            if (draft.MinStock < 0 || draft.MinStock > 999999999999999m ||
                draft.RetailPrice < 0 || draft.RetailPrice > 99999999999999.9999m ||
                (draft.WholesalePrice.HasValue &&
                 (draft.WholesalePrice.Value < 0 || draft.WholesalePrice.Value > 99999999999999.9999m)))
            {
                throw new ArgumentException("الكميات والأسعار يجب أن تكون أرقامًا غير سالبة وضمن حدود قاعدة البيانات.");
            }

            if (decimal.Round(draft.MinStock, 3) != draft.MinStock ||
                decimal.Round(draft.RetailPrice, 4) != draft.RetailPrice ||
                (draft.WholesalePrice.HasValue &&
                 decimal.Round(draft.WholesalePrice.Value, 4) != draft.WholesalePrice.Value))
            {
                throw new ArgumentException("الحد الأدنى يقبل 3 خانات عشرية، والأسعار تقبل 4 خانات كحد أقصى.");
            }
        }

        private static string CleanOptional(string value)
        {
            string trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static void EnsureActiveLookup(
            SqlConnection connection,
            SqlTransaction transaction,
            string tableName,
            int id,
            string label,
            bool allowInactive)
        {
            string query = "SELECT COUNT(*) FROM " + tableName +
                           " WHERE Id = @Id AND (IsActive = 1 OR @AllowInactive = 1);";
            using (var command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                command.Parameters.Add("@AllowInactive", SqlDbType.Bit).Value = allowInactive;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException("العنصر المحدد في " + label + " غير موجود أو غير نشط.");
                }
            }
        }

        private static ProductRecord LoadExisting(
            SqlConnection connection,
            SqlTransaction transaction,
            int productId)
        {
            using (var command = new SqlCommand(
                @"SELECT p.Id, p.Sku, p.Name, p.CategoryId, c.Name,
                         p.BaseUnitId, u.Name, pu.Barcode, pu.RetailPrice,
                         pu.WholesalePrice, p.MinStock, p.AvgCost, p.TracksExpiry, p.IsActive
                  FROM dbo.Products AS p WITH (UPDLOCK, ROWLOCK)
                  INNER JOIN dbo.Categories AS c ON c.Id = p.CategoryId
                  INNER JOIN dbo.Units AS u ON u.Id = p.BaseUnitId
                  LEFT JOIN dbo.ProductUnits AS pu
                    ON pu.ProductId = p.Id AND pu.UnitId = p.BaseUnitId
                  WHERE p.Id = @Id;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = productId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new ProductRecord
                    {
                        Id = reader.GetInt32(0),
                        Sku = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Name = reader.GetString(2),
                        CategoryId = reader.GetInt32(3),
                        CategoryName = reader.GetString(4),
                        BaseUnitId = reader.GetInt32(5),
                        BaseUnitName = reader.GetString(6),
                        Barcode = reader.IsDBNull(7) ? null : reader.GetString(7),
                        RetailPrice = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                        WholesalePrice = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                        MinStock = reader.GetDecimal(10),
                        AverageCost = reader.GetDecimal(11),
                        TracksExpiry = reader.GetBoolean(12),
                        IsActive = reader.GetBoolean(13)
                    };
                }
            }
        }

        private static int InsertProduct(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            ProductDraft draft)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.Products
                    (Sku, Name, CategoryId, BaseUnitId, TracksExpiry, MinStock, CreatedBy, IsActive)
                  VALUES
                    (@Sku, @Name, @CategoryId, @BaseUnitId, @TracksExpiry, @MinStock, @UserId, @IsActive);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                connection,
                transaction))
            {
                AddProductParameters(command, userId, draft);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int UpdateProduct(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            ProductDraft draft)
        {
            using (var command = new SqlCommand(
                @"UPDATE dbo.Products
                  SET Sku = @Sku, Name = @Name, CategoryId = @CategoryId,
                      TracksExpiry = @TracksExpiry, MinStock = @MinStock,
                      IsActive = @IsActive, UpdatedAt = SYSDATETIME(), UpdatedBy = @UserId
                  WHERE Id = @Id;",
                connection,
                transaction))
            {
                AddProductParameters(command, userId, draft);
                command.Parameters.Add("@Id", SqlDbType.Int).Value = draft.Id.Value;
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("تعذر حفظ تعديلات المنتج.");
                }

                return draft.Id.Value;
            }
        }

        private static void AddProductParameters(
            SqlCommand command,
            int userId,
            ProductDraft draft)
        {
            command.Parameters.Add("@Sku", SqlDbType.NVarChar, 50).Value =
                draft.Sku == null ? (object)DBNull.Value : draft.Sku;
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = draft.Name;
            command.Parameters.Add("@CategoryId", SqlDbType.Int).Value = draft.CategoryId;
            command.Parameters.Add("@BaseUnitId", SqlDbType.Int).Value = draft.BaseUnitId;
            command.Parameters.Add("@TracksExpiry", SqlDbType.Bit).Value = draft.TracksExpiry;
            SqlParameter minimum = command.Parameters.Add("@MinStock", SqlDbType.Decimal);
            minimum.Precision = 18;
            minimum.Scale = 3;
            minimum.Value = draft.MinStock;
            command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = draft.IsActive;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        }

        private static void SaveBaseUnit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            ProductDraft draft)
        {
            using (var command = new SqlCommand(
                @"IF EXISTS
                    (SELECT 1 FROM dbo.ProductUnits WHERE ProductId = @ProductId AND UnitId = @UnitId)
                  BEGIN
                    UPDATE dbo.ProductUnits
                    SET Barcode = @Barcode, RetailPrice = @RetailPrice,
                        WholesalePrice = @WholesalePrice, IsActive = @IsActive,
                        UpdatedAt = SYSDATETIME(), UpdatedBy = @UserId
                    WHERE ProductId = @ProductId AND UnitId = @UnitId;
                  END
                  ELSE
                  BEGIN
                    INSERT dbo.ProductUnits
                        (ProductId, UnitId, FactorToBase, Barcode, RetailPrice,
                         WholesalePrice, IsDefaultSaleUnit, IsActive, CreatedBy)
                    VALUES
                        (@ProductId, @UnitId, 1, @Barcode, @RetailPrice,
                         @WholesalePrice, 1, @IsActive, @UserId);
                  END",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                command.Parameters.Add("@UnitId", SqlDbType.Int).Value = draft.BaseUnitId;
                command.Parameters.Add("@Barcode", SqlDbType.NVarChar, 50).Value =
                    draft.Barcode == null ? (object)DBNull.Value : draft.Barcode;
                AddMoney(command, "@RetailPrice", draft.RetailPrice);
                AddNullableMoney(command, "@WholesalePrice", draft.WholesalePrice);
                command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = draft.IsActive;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void AddMoney(SqlCommand command, string name, decimal value)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = 18;
            parameter.Scale = 4;
            parameter.Value = value;
        }

        private static void AddNullableMoney(SqlCommand command, string name, decimal? value)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = 18;
            parameter.Scale = 4;
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void InsertAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            ProductRecord previous,
            ProductDraft draft)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, OldValues, NewValues)
                  VALUES
                    (@UserId, @Action, N'Products', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value =
                    previous == null ? "ProductCreated" : "ProductUpdated";
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    productId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value =
                    previous == null ? (object)DBNull.Value : ToJson(previous);
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = ToJson(draft);
                command.ExecuteNonQuery();
            }
        }

        private static string ToJson(ProductRecord product)
        {
            var values = new Dictionary<string, object>
            {
                { "Sku", product.Sku },
                { "Name", product.Name },
                { "CategoryId", product.CategoryId },
                { "BaseUnitId", product.BaseUnitId },
                { "Barcode", product.Barcode },
                { "RetailPrice", product.RetailPrice },
                { "WholesalePrice", product.WholesalePrice.HasValue ? (object)product.WholesalePrice.Value : null },
                { "MinStock", product.MinStock },
                { "TracksExpiry", product.TracksExpiry },
                { "IsActive", product.IsActive }
            };
            return ToJson(values);
        }

        private static string ToJson(ProductDraft product)
        {
            var values = new Dictionary<string, object>
            {
                { "Sku", product.Sku },
                { "Name", product.Name },
                { "CategoryId", product.CategoryId },
                { "BaseUnitId", product.BaseUnitId },
                { "Barcode", product.Barcode },
                { "RetailPrice", product.RetailPrice },
                { "WholesalePrice", product.WholesalePrice.HasValue ? (object)product.WholesalePrice.Value : null },
                { "MinStock", product.MinStock },
                { "TracksExpiry", product.TracksExpiry },
                { "IsActive", product.IsActive }
            };
            return ToJson(values);
        }

        private static string ToJson(IDictionary<string, object> values)
        {
            var json = new StringBuilder("{");
            bool first = true;
            foreach (KeyValuePair<string, object> value in values)
            {
                if (!first)
                {
                    json.Append(',');
                }

                json.Append('"').Append(EscapeJson(value.Key)).Append("\":");
                if (value.Value == null)
                {
                    json.Append("null");
                }
                else if (value.Value is bool)
                {
                    json.Append((bool)value.Value ? "true" : "false");
                }
                else if (value.Value is int || value.Value is decimal)
                {
                    json.Append(Convert.ToString(value.Value, CultureInfo.InvariantCulture));
                }
                else
                {
                    json.Append('"').Append(EscapeJson(Convert.ToString(value.Value, CultureInfo.InvariantCulture))).Append('"');
                }

                first = false;
            }

            return json.Append('}').ToString();
        }

        private static string EscapeJson(string value)
        {
            var escaped = new StringBuilder(value.Length);
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
}
