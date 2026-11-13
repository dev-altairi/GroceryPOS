using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace GroceryPOS
{
    internal sealed class SaleCurrency
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public int DecimalPlaces { get; private set; }
        internal string RoundingMode { get; private set; }
        internal bool IsBase { get; private set; }
        internal int? ExchangeRateId { get; set; }
        internal decimal RateToBase { get; set; }

        internal SaleCurrency(
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
            RateToBase = isBase ? 1m : 0m;
        }

        public override string ToString()
        {
            return Code + " - " + Name;
        }
    }

    internal sealed class SaleUnitOption
    {
        public int ProductUnitId { get; private set; }
        public int ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string UnitName { get; private set; }
        public string Barcode { get; private set; }
        public decimal FactorToBase { get; private set; }
        public decimal RetailPrice { get; private set; }
        public decimal AverageCost { get; private set; }
        public decimal AvailableQuantity { get; private set; }
        public bool TracksExpiry { get; private set; }
        public string DisplayName { get; private set; }

        internal SaleUnitOption(
            int productUnitId,
            int productId,
            string productName,
            string unitName,
            string barcode,
            decimal factorToBase,
            decimal retailPrice,
            decimal averageCost,
            decimal availableQuantity,
            bool tracksExpiry)
        {
            ProductUnitId = productUnitId;
            ProductId = productId;
            ProductName = productName;
            UnitName = unitName;
            Barcode = barcode;
            FactorToBase = factorToBase;
            RetailPrice = retailPrice;
            AverageCost = averageCost;
            AvailableQuantity = availableQuantity;
            TracksExpiry = tracksExpiry;
            DisplayName = productName + " — " + unitName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    internal sealed class SaleLineDraft
    {
        public int ProductUnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Discount { get; set; }
        public bool BelowCostConfirmed { get; set; }
    }

    internal sealed class SalePaymentDraft
    {
        public int CurrencyId { get; set; }
        public decimal AmountPaid { get; set; }
    }

    internal sealed class SaleDraft
    {
        internal IList<SaleLineDraft> Lines { get; set; }
        internal IList<SalePaymentDraft> Payments { get; set; }
        internal int? ChangeCurrencyId { get; set; }
        internal string IdempotencyKey { get; set; }
    }

    internal sealed class SaleReceipt
    {
        public string InvoiceNo { get; set; }
        public decimal Total { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ChangeAmount { get; set; }
        public string ChangeCurrencyCode { get; set; }
    }

    internal static class SalesService
    {
        private const decimal MaximumMoney = 99999999999999.9999m;
        private const decimal MaximumQuantity = 999999999999999.999m;

        private sealed class PreparedLine
        {
            internal SaleLineDraft Draft;
            internal int ProductId;
            internal string ProductName;
            internal string UnitName;
            internal decimal FactorToBase;
            internal decimal QtyBase;
            internal decimal UnitPrice;
            internal decimal Gross;
            internal decimal Discount;
            internal decimal LineTotal;
            internal decimal AverageCost;
            internal bool BelowCost;
        }

        internal static List<SaleUnitOption> SearchUnits(AuthenticatedUser user, string searchText)
        {
            RequireSalesPermission(user);
            string search = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim();
            if (search != null && search.Length > 200)
            {
                search = search.Substring(0, 200);
            }

            var units = new List<SaleUnitOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT TOP (100) pu.Id, p.Id, p.Name, u.Name, pu.Barcode, pu.FactorToBase,
                         pu.RetailPrice, p.AvgCost, ISNULL(stock.TotalQtyBase, 0), p.TracksExpiry
                  FROM dbo.ProductUnits pu
                  INNER JOIN dbo.Products p ON p.Id = pu.ProductId AND p.IsActive = 1
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  LEFT JOIN dbo.vw_ProductStock stock ON stock.ProductId = p.Id
                  WHERE pu.IsActive = 1
                    AND (@Search IS NULL OR pu.Barcode = @Search
                         OR p.Name LIKE N'%' + @Search + N'%'
                         OR p.Sku = @Search)
                  ORDER BY CASE WHEN pu.Barcode = @Search THEN 0 ELSE 1 END,
                           p.Name, pu.FactorToBase, u.Name;",
                connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value =
                    search == null ? (object)DBNull.Value : search;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        units.Add(new SaleUnitOption(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            reader.GetDecimal(5),
                            reader.GetDecimal(6),
                            reader.GetDecimal(7),
                            reader.GetDecimal(8),
                            reader.GetBoolean(9)));
                    }
                }
            }

            return units;
        }

        internal static List<SaleCurrency> LoadPaymentCurrencies(AuthenticatedUser user)
        {
            RequireSalesPermission(user);
            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                SaleCurrency baseCurrency = LoadBaseCurrency(connection, transaction);
                List<SaleCurrency> currencies = LoadPaymentRates(
                    connection,
                    transaction,
                    baseCurrency,
                    DateTime.Today);
                transaction.Commit();

                if (currencies.Count == 0 || currencies.Count(currency => currency.IsBase) != 1)
                {
                    throw new InvalidOperationException("يجب تفعيل عملة أساسية واحدة قبل البيع.");
                }

                return currencies;
            }
        }

        internal static SaleReceipt PostSale(AuthenticatedUser user, SaleDraft draft)
        {
            RequireSalesPermission(user);
            ValidateDraft(user, draft);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                int shiftId = LockCashierShift(connection, transaction, user.Id);
                SaleCurrency baseCurrency = LoadBaseCurrency(connection, transaction);
                List<SaleCurrency> paymentCurrencies =
                    LoadPaymentRates(connection, transaction, baseCurrency, DateTime.Today);
                List<PreparedLine> lines = new List<PreparedLine>();
                foreach (SaleLineDraft line in draft.Lines.OrderBy(item => item.ProductUnitId))
                {
                    lines.Add(PrepareLine(connection, transaction, user, line, baseCurrency));
                }

                decimal subtotal = 0m;
                decimal discount = 0m;
                decimal total = 0m;
                foreach (PreparedLine line in lines)
                {
                    subtotal = checked(subtotal + line.Gross);
                    discount = checked(discount + line.Discount);
                    total = checked(total + line.LineTotal);
                }

                subtotal = RoundToCurrency(subtotal, baseCurrency);
                discount = RoundToCurrency(discount, baseCurrency);
                total = RoundToCurrency(total, baseCurrency);
                if (total <= 0 || subtotal > MaximumMoney || discount > subtotal ||
                    total != subtotal - discount)
                {
                    throw new ArgumentException("إجمالي الفاتورة غير صالح أو يتجاوز الحد المسموح.");
                }

                decimal paidInBase = 0m;
                var preparedPayments = new List<Tuple<SalePaymentDraft, SaleCurrency, decimal>>();
                foreach (SalePaymentDraft payment in draft.Payments)
                {
                    SaleCurrency currency = paymentCurrencies.FirstOrDefault(item => item.Id == payment.CurrencyId);
                    if (currency == null)
                    {
                        throw new InvalidOperationException("عملة أحد مبالغ الدفع غير نشطة.");
                    }

                    ValidateCurrencyAmount(payment.AmountPaid, currency, "مبلغ الدفع");
                    if (payment.AmountPaid <= 0)
                    {
                        continue;
                    }

                    decimal amountInBase = RoundToCurrency(
                        checked(payment.AmountPaid * currency.RateToBase),
                        baseCurrency);
                    if (amountInBase <= 0 || amountInBase > MaximumMoney)
                    {
                        throw new ArgumentException("مبلغ الدفع بعد التحويل يتجاوز حدود العملة الأساسية.");
                    }

                    paidInBase = checked(paidInBase + amountInBase);
                    if (paidInBase > MaximumMoney)
                    {
                        throw new ArgumentException("مجموع المدفوعات يتجاوز الحد المسموح.");
                    }

                    preparedPayments.Add(Tuple.Create(payment, currency, amountInBase));
                }

                if (paidInBase < total)
                {
                    throw new ArgumentException("مجموع المدفوعات أقل من إجمالي الفاتورة.");
                }

                decimal overpayment = paidInBase - total;
                decimal changeAmount = 0m;
                SaleCurrency changeCurrency = null;
                if (overpayment > 0)
                {
                    if (!draft.ChangeCurrencyId.HasValue)
                    {
                        throw new ArgumentException("اختر عملة صرف الباقي.");
                    }

                    changeCurrency = paymentCurrencies.FirstOrDefault(
                        currency => currency.Id == draft.ChangeCurrencyId.Value);
                    if (changeCurrency == null)
                    {
                        throw new InvalidOperationException("عملة صرف الباقي غير نشطة.");
                    }

                    changeAmount = RoundToCurrency(
                        overpayment / changeCurrency.RateToBase,
                        changeCurrency);
                    decimal changeInBase = RoundToCurrency(
                        checked(changeAmount * changeCurrency.RateToBase),
                        baseCurrency);
                    if (changeAmount <= 0 || changeInBase < overpayment)
                    {
                        throw new ArgumentException(
                            "تعذر صرف الباقي كاملاً بدقة هذه العملة. غيّر عملة الباقي أو أدخل مبلغًا أقرب.");
                    }
                }

                var invoiceTotals = new SaleReceipt { Total = total, CurrencyCode = baseCurrency.Code };
                string temporaryInvoiceNumber =
                    "TMP-" + Guid.NewGuid().ToString("N").Substring(0, 26);
                long invoiceId = InsertInvoice(
                    connection,
                    transaction,
                    user.Id,
                    shiftId,
                    temporaryInvoiceNumber,
                    subtotal,
                    discount,
                    total,
                    changeAmount,
                    changeCurrency,
                    draft.IdempotencyKey);

                foreach (PreparedLine line in lines)
                {
                    long invoiceLineId = InsertInvoiceLine(
                        connection,
                        transaction,
                        user.Id,
                        invoiceId,
                        line);
                    AllocateBatches(
                        connection,
                        transaction,
                        user,
                        invoiceId,
                        invoiceLineId,
                        line);
                }

                foreach (Tuple<SalePaymentDraft, SaleCurrency, decimal> payment in preparedPayments)
                {
                    InsertPayment(
                        connection,
                        transaction,
                        user.Id,
                        invoiceId,
                        payment.Item1,
                        payment.Item2,
                        payment.Item3);
                }

                string invoiceNumber = GetNextInvoiceNumber(connection, transaction);
                UpdateInvoiceNumber(connection, transaction, invoiceId, invoiceNumber);
                InsertSaleAudit(
                    connection,
                    transaction,
                    user.Id,
                    invoiceId,
                    invoiceNumber,
                    total,
                    baseCurrency.Code,
                    lines.Count,
                    preparedPayments.Count);
                transaction.Commit();

                invoiceTotals.InvoiceNo = invoiceNumber;
                invoiceTotals.ChangeAmount = changeAmount;
                invoiceTotals.ChangeCurrencyCode = changeCurrency == null ? null : changeCurrency.Code;
                return invoiceTotals;
            }
        }

        private static PreparedLine PrepareLine(
            SqlConnection connection,
            SqlTransaction transaction,
            AuthenticatedUser user,
            SaleLineDraft draft,
            SaleCurrency baseCurrency)
        {
            using (var command = new SqlCommand(
                @"SELECT pu.ProductId, p.Name, u.Name, pu.FactorToBase, pu.RetailPrice,
                         p.AvgCost, p.TracksExpiry
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
                        throw new InvalidOperationException("أحد المنتجات أو وحدات البيع لم يعد نشطًا.");
                    }

                    int productId = reader.GetInt32(0);
                    string productName = reader.GetString(1);
                    string unitName = reader.GetString(2);
                    decimal factor = reader.GetDecimal(3);
                    decimal unitPrice = reader.GetDecimal(4);
                    decimal averageCost = reader.GetDecimal(5);
                    decimal qtyBase;
                    decimal gross;
                    try
                    {
                        qtyBase = decimal.Round(
                            checked(draft.Quantity * factor),
                            3,
                            MidpointRounding.AwayFromZero);
                        gross = RoundToCurrency(checked(unitPrice * draft.Quantity), baseCurrency);
                    }
                    catch (OverflowException)
                    {
                        throw new ArgumentException("كمية أو قيمة المنتج " + productName + " تتجاوز الحد المسموح.");
                    }

                    if (qtyBase <= 0 || qtyBase > MaximumQuantity)
                    {
                        throw new ArgumentException("كمية المنتج " + productName + " خارج النطاق.");
                    }

                    decimal discount = draft.Discount;
                    ValidateCurrencyAmount(discount, baseCurrency, "الخصم");
                    if (discount < 0 || discount > gross)
                    {
                        throw new ArgumentException("خصم المنتج " + productName + " يجب ألا يتجاوز إجماليه.");
                    }

                    if (discount > 0 && !user.HasPermission("Sales.Discount"))
                    {
                        throw new UnauthorizedAccessException("لا تملك صلاحية منح خصم على المبيعات.");
                    }

                    decimal lineTotal = gross - discount;
                    bool belowCost = (lineTotal / draft.Quantity / factor) < averageCost;
                    if (belowCost &&
                        (!user.HasPermission("Sales.Discount") || !draft.BelowCostConfirmed))
                    {
                        throw new InvalidOperationException(
                            "سعر " + productName +
                            " بعد الخصم أقل من متوسط التكلفة. يلزم تأكيد البيع بسعر أقل من التكلفة وصلاحية الخصم.");
                    }

                    return new PreparedLine
                    {
                        Draft = draft,
                        ProductId = productId,
                        ProductName = productName,
                        UnitName = unitName,
                        FactorToBase = factor,
                        QtyBase = qtyBase,
                        UnitPrice = unitPrice,
                        Gross = gross,
                        Discount = discount,
                        LineTotal = lineTotal,
                        AverageCost = averageCost,
                        BelowCost = belowCost
                    };
                }
            }
        }

        private static int LockCashierShift(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId)
        {
            using (var command = new SqlCommand(
                @"SELECT TOP (1) Id
                  FROM dbo.Shifts WITH (UPDLOCK, HOLDLOCK)
                  WHERE CashierId = @CashierId AND Status = N'Open'
                  ORDER BY Id DESC;",
                connection,
                transaction))
            {
                command.Parameters.Add("@CashierId", SqlDbType.Int).Value = userId;
                object value = command.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                {
                    throw new InvalidOperationException("افتح وردية قبل إتمام عملية البيع.");
                }

                return Convert.ToInt32(value);
            }
        }

        private static SaleCurrency LoadBaseCurrency(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            using (var command = new SqlCommand(
                @"SELECT Id, Code, Name, DecimalPlaces, RoundingMode
                  FROM dbo.Currencies WITH (UPDLOCK, HOLDLOCK)
                  WHERE IsBase = 1 AND IsActive = 1;",
                connection,
                transaction))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new InvalidOperationException("تعذر تحديد العملة الأساسية النشطة.");
                }

                return new SaleCurrency(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt32(3),
                    reader.GetString(4),
                    true);
            }
        }

        private static List<SaleCurrency> LoadPaymentRates(
            SqlConnection connection,
            SqlTransaction transaction,
            SaleCurrency baseCurrency,
            DateTime paymentDate)
        {
            var currencies = new List<SaleCurrency>();
            using (var command = new SqlCommand(
                @"SELECT Id, Code, Name, DecimalPlaces, RoundingMode, IsBase
                  FROM dbo.Currencies WITH (UPDLOCK, HOLDLOCK)
                  WHERE IsActive = 1
                  ORDER BY IsBase DESC, Code;",
                connection,
                transaction))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    currencies.Add(new SaleCurrency(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3),
                        reader.GetString(4),
                        reader.GetBoolean(5)));
                }
            }

            foreach (SaleCurrency currency in currencies)
            {
                if (currency.IsBase)
                {
                    currency.RateToBase = 1m;
                    continue;
                }

                using (var command = new SqlCommand(
                    @"SELECT TOP (1) Id, RateToBase
                      FROM dbo.ExchangeRates WITH (UPDLOCK, HOLDLOCK)
                      WHERE CurrencyId = @CurrencyId
                        AND EffectiveFrom < DATEADD(DAY, 1, @PaymentDate)
                      ORDER BY EffectiveFrom DESC, Id DESC;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currency.Id;
                    command.Parameters.Add("@PaymentDate", SqlDbType.Date).Value = paymentDate.Date;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            currency.RateToBase = 0m;
                            continue;
                        }

                        currency.ExchangeRateId = reader.GetInt32(0);
                        currency.RateToBase = reader.GetDecimal(1);
                    }
                }
            }

            currencies.RemoveAll(currency => !currency.IsBase && currency.RateToBase <= 0);
            if (currencies.All(currency => currency.Id != baseCurrency.Id))
            {
                throw new InvalidOperationException("تغيرت العملات النشطة أثناء تسجيل البيع.");
            }

            return currencies;
        }

        private static long InsertInvoice(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int shiftId,
            string invoiceNumber,
            decimal subtotal,
            decimal discount,
            decimal total,
            decimal changeAmount,
            SaleCurrency changeCurrency,
            string idempotencyKey)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.SalesInvoices
                    (InvoiceNo, ShiftId, CustomerId, CurrencyId, ExchangeRateId, RateUsed,
                     Subtotal, Discount, Total, TotalBase, CreditAmount, CreditSettled,
                     ChangeAmount, ChangeCurrencyId, Status, IdempotencyKey, CreatedBy)
                  VALUES
                    (@InvoiceNo, @ShiftId, NULL, @CurrencyId, NULL, 1,
                     @Subtotal, @Discount, @Total, @Total, 0, 0,
                     @ChangeAmount, @ChangeCurrencyId, N'Completed', @IdempotencyKey, @CreatedBy);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 30).Value = invoiceNumber;
                command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value =
                    GetBaseCurrencyId(connection, transaction);
                AddDecimal(command, "@Subtotal", 18, 4, subtotal);
                AddDecimal(command, "@Discount", 18, 4, discount);
                AddDecimal(command, "@Total", 18, 4, total);
                AddDecimal(command, "@ChangeAmount", 18, 4, changeAmount);
                command.Parameters.Add("@ChangeCurrencyId", SqlDbType.Int).Value =
                    changeCurrency == null ? (object)DBNull.Value : changeCurrency.Id;
                command.Parameters.Add("@IdempotencyKey", SqlDbType.NVarChar, 64).Value = idempotencyKey;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private static void UpdateInvoiceNumber(
            SqlConnection connection,
            SqlTransaction transaction,
            long invoiceId,
            string invoiceNumber)
        {
            using (var command = new SqlCommand(
                @"UPDATE dbo.SalesInvoices
                  SET InvoiceNo = @InvoiceNo
                  WHERE Id = @InvoiceId AND Status = N'Completed';",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 30).Value = invoiceNumber;
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("تعذر تثبيت رقم فاتورة البيع.");
                }
            }
        }

        private static int GetBaseCurrencyId(SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(
                "SELECT Id FROM dbo.Currencies WITH (HOLDLOCK) WHERE IsBase=1 AND IsActive=1;",
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

        private static long InsertInvoiceLine(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            PreparedLine line)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.SalesInvoiceLines
                    (InvoiceId, ProductUnitId, Qty, QtyBase, UnitPrice, Discount, LineTotal,
                     BelowCostConfirmed, CreatedBy)
                  VALUES
                    (@InvoiceId, @ProductUnitId, @Qty, @QtyBase, @UnitPrice, @Discount, @LineTotal,
                     @BelowCostConfirmed, @CreatedBy);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@ProductUnitId", SqlDbType.Int).Value = line.Draft.ProductUnitId;
                AddDecimal(command, "@Qty", 18, 3, line.Draft.Quantity);
                AddDecimal(command, "@QtyBase", 18, 3, line.QtyBase);
                AddDecimal(command, "@UnitPrice", 18, 4, line.UnitPrice);
                AddDecimal(command, "@Discount", 18, 4, line.Discount);
                AddDecimal(command, "@LineTotal", 18, 4, line.LineTotal);
                command.Parameters.Add("@BelowCostConfirmed", SqlDbType.Bit).Value = line.BelowCost;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private static void AllocateBatches(
            SqlConnection connection,
            SqlTransaction transaction,
            AuthenticatedUser user,
            long invoiceId,
            long invoiceLineId,
            PreparedLine line)
        {
            bool allowExpired = user.HasPermission("Inventory.SellExpired");
            var batches = new List<Tuple<long, decimal, decimal>>();
            using (var command = new SqlCommand(
                @"SELECT Id, QtyRemaining, UnitCost
                  FROM dbo.Batches WITH (UPDLOCK, ROWLOCK)
                  WHERE ProductId = @ProductId
                    AND QtyRemaining > 0
                    AND (@AllowExpired = 1 OR ExpiryDate IS NULL OR ExpiryDate >= @Today)
                  ORDER BY CASE WHEN ExpiryDate IS NULL THEN 1 ELSE 0 END,
                           ExpiryDate, CreatedAt, Id;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = line.ProductId;
                command.Parameters.Add("@AllowExpired", SqlDbType.Bit).Value = allowExpired;
                command.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        batches.Add(Tuple.Create(
                            reader.GetInt64(0),
                            reader.GetDecimal(1),
                            reader.GetDecimal(2)));
                    }
                }
            }

            decimal available = batches.Sum(batch => batch.Item2);
            if (available < line.QtyBase)
            {
                throw new InvalidOperationException(
                    "المخزون الصالح للمنتج " + line.ProductName +
                    " غير كافٍ. المطلوب " + line.QtyBase.ToString("N3", CultureInfo.CurrentCulture) +
                    " والمتاح " + available.ToString("N3", CultureInfo.CurrentCulture) + ".");
            }

            decimal remaining = line.QtyBase;
            foreach (Tuple<long, decimal, decimal> batch in batches)
            {
                if (remaining <= 0)
                {
                    break;
                }

                decimal allocated = Math.Min(remaining, batch.Item2);
                using (var command = new SqlCommand(
                    @"UPDATE dbo.Batches
                      SET QtyRemaining = QtyRemaining - @Quantity,
                          UpdatedAt = SYSDATETIME(),
                          UpdatedBy = @UpdatedBy
                      WHERE Id = @BatchId AND QtyRemaining >= @Quantity;",
                    connection,
                    transaction))
                {
                    AddDecimal(command, "@Quantity", 18, 3, allocated);
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batch.Item1;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تغير رصيد دفعة أثناء البيع؛ حدّث السلة وأعد المحاولة.");
                    }
                }

                InsertSaleBatchAllocation(
                    connection,
                    transaction,
                    user.Id,
                    invoiceLineId,
                    batch.Item1,
                    allocated,
                    line.AverageCost,
                    batch.Item3);
                InsertSaleStockMovement(
                    connection,
                    transaction,
                    user.Id,
                    line.ProductId,
                    batch.Item1,
                    invoiceId,
                    allocated,
                    line.AverageCost);
                remaining -= allocated;
            }

            if (remaining != 0)
            {
                throw new InvalidOperationException("تعذر توزيع كامل كمية البيع على دفعات المخزون.");
            }
        }

        private static void InsertSaleBatchAllocation(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long lineId,
            long batchId,
            decimal quantity,
            decimal averageCost,
            decimal batchCost)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.SalesLineBatches
                    (LineId, BatchId, QtyBase, UnitCost, BatchUnitCost, CreatedBy)
                  VALUES
                    (@LineId, @BatchId, @Quantity, @AverageCost, @BatchCost, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@LineId", SqlDbType.BigInt).Value = lineId;
                command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
                AddDecimal(command, "@Quantity", 18, 3, quantity);
                AddDecimal(command, "@AverageCost", 18, 4, averageCost);
                AddDecimal(command, "@BatchCost", 18, 4, batchCost);
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void InsertSaleStockMovement(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int productId,
            long batchId,
            long invoiceId,
            decimal quantity,
            decimal averageCost)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.StockMovements
                    (ProductId, BatchId, MovementType, Qty, UnitCost, RefType, RefId, CreatedBy)
                  VALUES
                    (@ProductId, @BatchId, N'Sale', @Quantity, @AverageCost,
                     N'SalesInvoice', @InvoiceId, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
                AddDecimal(command, "@Quantity", 18, 3, -quantity);
                AddDecimal(command, "@AverageCost", 18, 4, averageCost);
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void InsertPayment(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            SalePaymentDraft payment,
            SaleCurrency currency,
            decimal amountInBase)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.SalesPayments
                    (InvoiceId, PaymentCurrencyId, AmountPaid, ExchangeRateId, RateUsed,
                     AmountInInvoiceCurrency, CreatedBy)
                  VALUES
                    (@InvoiceId, @CurrencyId, @AmountPaid, @ExchangeRateId, @RateUsed,
                     @AmountInInvoice, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currency.Id;
                AddDecimal(command, "@AmountPaid", 18, 4, payment.AmountPaid);
                command.Parameters.Add("@ExchangeRateId", SqlDbType.Int).Value =
                    currency.ExchangeRateId.HasValue
                        ? (object)currency.ExchangeRateId.Value
                        : DBNull.Value;
                AddDecimal(command, "@RateUsed", 18, 6, currency.RateToBase);
                AddDecimal(command, "@AmountInInvoice", 18, 4, amountInBase);
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static string GetNextInvoiceNumber(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            using (var command = new SqlCommand("dbo.usp_GetNextDocumentNumber", connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = "SalesInvoice";
                command.Parameters.Add("@Number", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                SqlParameter formatted = command.Parameters.Add("@Formatted", SqlDbType.NVarChar, 50);
                formatted.Direction = ParameterDirection.Output;
                command.ExecuteNonQuery();
                if (formatted.Value == null || formatted.Value == DBNull.Value)
                {
                    throw new InvalidOperationException("تعذر إنشاء رقم فاتورة البيع.");
                }

                return Convert.ToString(formatted.Value, CultureInfo.InvariantCulture);
            }
        }

        private static void InsertSaleAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long invoiceId,
            string invoiceNumber,
            decimal total,
            string currencyCode,
            int lineCount,
            int paymentCount)
        {
            string newValues = string.Format(
                CultureInfo.InvariantCulture,
                "{{\"InvoiceNo\":\"{0}\",\"Total\":{1},\"Currency\":\"{2}\",\"LineCount\":{3},\"PaymentCount\":{4}}}",
                EscapeJson(invoiceNumber),
                total,
                EscapeJson(currencyCode),
                lineCount,
                paymentCount);
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, NewValues)
                  VALUES
                    (@UserId, N'SaleCompleted', N'SalesInvoices', @InvoiceId, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@InvoiceId", SqlDbType.NVarChar, 100).Value =
                    invoiceId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static void ValidateDraft(AuthenticatedUser user, SaleDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            if (string.IsNullOrWhiteSpace(draft.IdempotencyKey) ||
                draft.IdempotencyKey.Length > 64)
            {
                throw new ArgumentException("مفتاح منع تكرار الفاتورة غير صالح.");
            }

            if (draft.Lines == null || draft.Lines.Count == 0 || draft.Lines.Count > 250)
            {
                throw new ArgumentException("أضف من منتج واحد إلى 250 منتجًا إلى السلة.");
            }

            var productUnits = new HashSet<int>();
            foreach (SaleLineDraft line in draft.Lines)
            {
                if (line == null || line.ProductUnitId <= 0 ||
                    !productUnits.Add(line.ProductUnitId))
                {
                    throw new ArgumentException("توجد وحدة منتج غير صالحة أو مكررة في السلة.");
                }

                if (line.Quantity <= 0 || line.Quantity > MaximumQuantity ||
                    decimal.Round(line.Quantity, 3) != line.Quantity)
                {
                    throw new ArgumentException("كمية البيع يجب أن تكون موجبة وبحد أقصى 3 خانات عشرية.");
                }

                if (line.Discount < 0 || line.Discount > MaximumMoney ||
                    decimal.Round(line.Discount, 4) != line.Discount)
                {
                    throw new ArgumentException("الخصم يجب أن يكون غير سالب وبحد أقصى 4 خانات عشرية.");
                }

                if (line.BelowCostConfirmed && !user.HasPermission("Sales.Discount"))
                {
                    throw new UnauthorizedAccessException("تأكيد البيع بأقل من التكلفة يتطلب صلاحية الخصم.");
                }
            }

            if (draft.Payments == null || draft.Payments.Count == 0 ||
                draft.Payments.Count > 10 ||
                draft.Payments.Any(payment => payment == null || payment.CurrencyId <= 0))
            {
                throw new ArgumentException("أدخل طريقة دفع واحدة على الأقل، وبحد أقصى 10 عملات.");
            }

            if (draft.Payments.Select(payment => payment.CurrencyId).Distinct().Count() !=
                draft.Payments.Count)
            {
                throw new ArgumentException("لا تكرر العملة في قائمة المدفوعات.");
            }

            if (draft.ChangeCurrencyId.HasValue && draft.ChangeCurrencyId.Value <= 0)
            {
                throw new ArgumentException("عملة الباقي غير صالحة.");
            }
        }

        private static void ValidateCurrencyAmount(
            decimal amount,
            SaleCurrency currency,
            string label)
        {
            if (amount < 0 || amount > MaximumMoney ||
                decimal.Round(amount, 4) != amount ||
                decimal.Round(amount, currency.DecimalPlaces) != amount)
            {
                throw new ArgumentException(
                    label + " يجب أن يطابق دقة العملة " + currency.Code + ".");
            }
        }

        internal static decimal RoundToCurrency(decimal amount, SaleCurrency currency)
        {
            switch (currency.RoundingMode)
            {
                case "AwayFromZero":
                    return decimal.Round(amount, currency.DecimalPlaces, MidpointRounding.AwayFromZero);
                case "ToEven":
                    return decimal.Round(amount, currency.DecimalPlaces, MidpointRounding.ToEven);
                case "ToZero":
                    return decimal.Truncate(amount * Pow10(currency.DecimalPlaces)) /
                           Pow10(currency.DecimalPlaces);
                case "ToPositiveInfinity":
                    return decimal.Ceiling(amount * Pow10(currency.DecimalPlaces)) /
                           Pow10(currency.DecimalPlaces);
                case "ToNegativeInfinity":
                    return decimal.Floor(amount * Pow10(currency.DecimalPlaces)) /
                           Pow10(currency.DecimalPlaces);
                default:
                    throw new InvalidOperationException("طريقة تقريب العملة غير مدعومة: " + currency.RoundingMode);
            }
        }

        private static decimal Pow10(int decimalPlaces)
        {
            decimal value = 1m;
            for (int i = 0; i < decimalPlaces; i++)
            {
                value *= 10m;
            }

            return value;
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
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private static void RequireSalesPermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Sales.Create"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إتمام عمليات البيع.");
            }
        }
    }
}
