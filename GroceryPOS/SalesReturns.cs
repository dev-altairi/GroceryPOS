using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace GroceryPOS
{
    internal sealed class SaleReturnInvoice
    {
        public long Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime SaleDate { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Total { get; set; }
        public decimal ReturnedTotal { get; set; }

        public override string ToString()
        {
            return InvoiceNo;
        }
    }

    internal sealed class SaleReturnLine
    {
        public long InvoiceLineId { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal ReturnedTotal { get; set; }
        public decimal GoodQuantity { get; set; }
        public decimal DamagedQuantity { get; set; }
    }

    internal sealed class SaleReturnLineDraft
    {
        public long InvoiceLineId { get; set; }
        public decimal Quantity { get; set; }
        public string ItemCondition { get; set; }
    }

    internal sealed class SaleReturnDraft
    {
        public long InvoiceId { get; set; }
        public string Reason { get; set; }
        public string IdempotencyKey { get; set; }
        public IList<SaleReturnLineDraft> Lines { get; set; }
    }

    internal sealed class SaleReturnReceipt
    {
        public string ReturnNo { get; set; }
        public decimal Total { get; set; }
        public string CurrencyCode { get; set; }
    }

    internal static class SalesReturnsService
    {
        private const decimal MaximumMoney = 99999999999999.9999m;
        private const decimal MaximumQuantity = 999999999999999.999m;

        private sealed class ReturnInvoiceDetails
        {
            internal string InvoiceNo;
            internal int CurrencyId;
            internal SaleCurrency Currency;
        }

        private sealed class ReturnLineDetails
        {
            internal int ProductId;
            internal string ProductName;
            internal string UnitName;
            internal decimal Quantity;
            internal decimal QuantityBase;
            internal decimal UnitPrice;
            internal decimal LineTotal;
            internal decimal FactorToBase;
            internal decimal ReturnedQuantity;
            internal decimal ReturnedQuantityBase;
            internal decimal ReturnedTotal;
        }

        private sealed class ReturnBatch
        {
            internal long BatchId;
            internal decimal RemainingQuantityBase;
            internal decimal UnitCost;
        }

        internal static List<SaleReturnInvoice> LoadReturnableInvoices(AuthenticatedUser user)
        {
            RequireReturnPermission(user);
            var invoices = new List<SaleReturnInvoice>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT TOP (200) i.Id, i.InvoiceNo, i.CreatedAt, c.Code, i.Total,
                         ISNULL(returned.Total, 0)
                  FROM dbo.SalesInvoices i
                  INNER JOIN dbo.Currencies c ON c.Id = i.CurrencyId
                  OUTER APPLY
                  (
                      SELECT SUM(r.Total) AS Total
                      FROM dbo.SalesReturns r
                      WHERE r.InvoiceId = i.Id
                  ) returned
                  WHERE i.Status = N'Completed'
                    AND i.CreditAmount = 0
                    AND i.Total <= ISNULL
                    (
                        (SELECT SUM(sp.AmountInInvoiceCurrency)
                         FROM dbo.SalesPayments sp
                         WHERE sp.InvoiceId = i.Id),
                        0
                    )
                    AND EXISTS
                    (
                        SELECT 1
                        FROM dbo.SalesInvoiceLines l
                        OUTER APPLY
                        (
                            SELECT SUM(rl.Qty) AS Qty
                            FROM dbo.SalesReturnLines rl
                            WHERE rl.InvoiceLineId = l.Id
                        ) returnedLine
                        WHERE l.InvoiceId = i.Id
                          AND l.Qty > ISNULL(returnedLine.Qty, 0)
                    )
                  ORDER BY i.CreatedAt DESC, i.Id DESC;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    invoices.Add(new SaleReturnInvoice
                    {
                        Id = reader.GetInt64(0),
                        InvoiceNo = reader.GetString(1),
                        SaleDate = reader.GetDateTime(2),
                        CurrencyCode = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        ReturnedTotal = reader.GetDecimal(5)
                    });
                }
            }

            return invoices;
        }

        internal static List<SaleReturnLine> LoadReturnableLines(
            AuthenticatedUser user,
            long invoiceId)
        {
            RequireReturnPermission(user);
            if (invoiceId <= 0)
            {
                throw new ArgumentException("رقم فاتورة البيع غير صالح.");
            }

            var lines = new List<SaleReturnLine>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT l.Id, p.Name, u.Name, l.Qty,
                         ISNULL(returned.Qty, 0), l.Qty - ISNULL(returned.Qty, 0),
                         l.UnitPrice, l.LineTotal, ISNULL(returned.Total, 0)
                  FROM dbo.SalesInvoiceLines l
                  INNER JOIN dbo.ProductUnits pu ON pu.Id = l.ProductUnitId
                  INNER JOIN dbo.Products p ON p.Id = pu.ProductId
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  OUTER APPLY
                  (
                      SELECT SUM(rl.Qty) AS Qty, SUM(rl.LineTotal) AS Total
                      FROM dbo.SalesReturnLines rl
                      WHERE rl.InvoiceLineId = l.Id
                  ) returned
                  INNER JOIN dbo.SalesInvoices i ON i.Id = l.InvoiceId
                  WHERE l.InvoiceId = @InvoiceId
                    AND i.Status = N'Completed'
                    AND i.CreditAmount = 0
                    AND i.Total <= ISNULL
                    (
                        (SELECT SUM(sp.AmountInInvoiceCurrency)
                         FROM dbo.SalesPayments sp
                         WHERE sp.InvoiceId = i.Id),
                        0
                    )
                    AND l.Qty > ISNULL(returned.Qty, 0)
                  ORDER BY l.Id;",
                connection))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lines.Add(new SaleReturnLine
                        {
                            InvoiceLineId = reader.GetInt64(0),
                            ProductName = reader.GetString(1),
                            UnitName = reader.GetString(2),
                            QuantitySold = reader.GetDecimal(3),
                            QuantityReturned = reader.GetDecimal(4),
                            RemainingQuantity = reader.GetDecimal(5),
                            UnitPrice = reader.GetDecimal(6),
                            LineTotal = reader.GetDecimal(7),
                            ReturnedTotal = reader.GetDecimal(8)
                        });
                    }
                }
            }

            return lines;
        }

        internal static SaleReturnReceipt PostReturn(
            AuthenticatedUser user,
            SaleReturnDraft draft)
        {
            RequireReturnPermission(user);
            ValidateDraft(draft);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                int shiftId = LockCashierShift(connection, transaction, user.Id);
                ReturnInvoiceDetails invoice = LoadInvoice(
                    connection,
                    transaction,
                    draft.InvoiceId);
                EnsureRefundCurrencyInShift(
                    connection,
                    transaction,
                    shiftId,
                    invoice.CurrencyId);

                string temporaryReturnNo =
                    "TMP-" + Guid.NewGuid().ToString("N").Substring(0, 26);
                long returnId = InsertReturn(
                    connection,
                    transaction,
                    user.Id,
                    shiftId,
                    invoice,
                    draft,
                    temporaryReturnNo);

                decimal total = 0m;
                foreach (SaleReturnLineDraft line in draft.Lines
                    .OrderBy(item => item.InvoiceLineId)
                    .ThenBy(item => item.ItemCondition == "Good" ? 0 : 1))
                {
                    total = checked(total + ProcessReturnLine(
                        connection,
                        transaction,
                        user.Id,
                        returnId,
                        draft.InvoiceId,
                        line,
                        invoice.Currency));
                    if (total > MaximumMoney)
                    {
                        throw new ArgumentException("إجمالي المرتجع يتجاوز الحد المسموح.");
                    }
                }

                if (total > 0)
                {
                    ShiftCashAmount cash = ShiftsService.LoadExpectedCash(
                            connection,
                            transaction,
                            shiftId)
                        .FirstOrDefault(item => item.CurrencyId == invoice.CurrencyId);
                    if (cash == null)
                    {
                        throw new InvalidOperationException("عملة رد المبلغ غير مسجلة في الوردية الحالية.");
                    }

                    if (total > cash.ExpectedAmount)
                    {
                        throw new InvalidOperationException(
                            "رصيد الصندوق غير كافٍ لرد مبلغ " +
                            total.ToString("N4", CultureInfo.CurrentCulture) + " " +
                            invoice.Currency.Code + ".");
                    }

                    InsertCashRefund(
                        connection,
                        transaction,
                        user.Id,
                        shiftId,
                        invoice.CurrencyId,
                        total,
                        invoice.InvoiceNo);
                }

                string returnNo = GetNextReturnNumber(connection, transaction);
                UpdateReturn(
                    connection,
                    transaction,
                    returnId,
                    returnNo,
                    total);
                InsertReturnAudit(
                    connection,
                    transaction,
                    user.Id,
                    returnId,
                    invoice.InvoiceNo,
                    returnNo,
                    total,
                    invoice.Currency.Code,
                    draft.Lines.Count,
                    draft.Reason);
                transaction.Commit();

                return new SaleReturnReceipt
                {
                    ReturnNo = returnNo,
                    Total = total,
                    CurrencyCode = invoice.Currency.Code
                };
            }
        }

        private static decimal ProcessReturnLine(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long returnId,
            long invoiceId,
            SaleReturnLineDraft draft,
            SaleCurrency currency)
        {
            ReturnLineDetails line = LoadReturnLine(
                connection,
                transaction,
                invoiceId,
                draft.InvoiceLineId);
            decimal quantityBase = decimal.Round(
                checked(draft.Quantity * line.FactorToBase),
                3,
                MidpointRounding.AwayFromZero);
            decimal remainingQuantity = line.Quantity - line.ReturnedQuantity;
            decimal remainingQuantityBase = line.QuantityBase - line.ReturnedQuantityBase;
            decimal remainingValue = line.LineTotal - line.ReturnedTotal;
            if (quantityBase <= 0 || quantityBase > remainingQuantityBase ||
                draft.Quantity > remainingQuantity)
            {
                throw new InvalidOperationException(
                    "كمية الإرجاع للصنف " + line.ProductName + " تتجاوز الكمية المتبقية من الفاتورة.");
            }

            decimal lineTotal = draft.Quantity == remainingQuantity
                ? remainingValue
                : SalesService.RoundToCurrency(
                    checked(remainingValue * (draft.Quantity / remainingQuantity)),
                    currency);
            if (lineTotal < 0 || lineTotal > remainingValue)
            {
                throw new InvalidOperationException("تعذر حساب قيمة مرتجع الصنف " + line.ProductName + ".");
            }

            List<ReturnBatch> batches = LoadReturnBatches(
                connection,
                transaction,
                draft.InvoiceLineId);
            decimal availableQuantityBase = batches.Sum(batch => batch.RemainingQuantityBase);
            if (quantityBase > availableQuantityBase)
            {
                throw new InvalidOperationException(
                    "تعذر ربط الكمية المرتجعة من " + line.ProductName + " بدفعات الفاتورة الأصلية.");
            }

            decimal remainingToAllocate = quantityBase;
            decimal remainingValueToAllocate = lineTotal;
            var allocations = new List<Tuple<ReturnBatch, decimal, decimal>>();
            foreach (ReturnBatch batch in batches)
            {
                if (remainingToAllocate <= 0)
                {
                    break;
                }

                decimal allocated = Math.Min(remainingToAllocate, batch.RemainingQuantityBase);
                decimal allocatedValue = allocated == remainingToAllocate
                    ? remainingValueToAllocate
                    : Math.Min(
                        remainingValueToAllocate,
                        SalesService.RoundToCurrency(
                            checked(lineTotal * (allocated / quantityBase)),
                            currency));

                allocations.Add(Tuple.Create(batch, allocated, allocatedValue));
                remainingToAllocate -= allocated;
                remainingValueToAllocate -= allocatedValue;
            }

            if (remainingToAllocate != 0 || remainingValueToAllocate != 0 || allocations.Count == 0)
            {
                throw new InvalidOperationException("تعذر توزيع كامل كمية أو قيمة المرتجع على دفعات البيع.");
            }

            decimal remainingSaleUnitQuantity = draft.Quantity;
            for (int index = 0; index < allocations.Count; index++)
            {
                ReturnBatch batch = allocations[index].Item1;
                decimal allocated = allocations[index].Item2;
                decimal allocatedValue = allocations[index].Item3;
                decimal allocatedSaleUnitQuantity = index == allocations.Count - 1
                    ? remainingSaleUnitQuantity
                    : decimal.Round(
                        checked(draft.Quantity * (allocated / quantityBase)),
                        3,
                        MidpointRounding.AwayFromZero);
                if (allocatedSaleUnitQuantity <= 0 ||
                    allocatedSaleUnitQuantity > remainingSaleUnitQuantity)
                {
                    throw new InvalidOperationException(
                        "تعذر تمثيل كمية الإرجاع بدقة وحدة البيع لكل دفعة أصلية.");
                }

                if (string.Equals(draft.ItemCondition, "Good", StringComparison.Ordinal))
                {
                    RestockBatch(
                        connection,
                        transaction,
                        userId,
                        batch.BatchId,
                        allocated);
                    InsertReturnStockMovement(
                        connection,
                        transaction,
                        userId,
                        returnId,
                        line.ProductId,
                        batch.BatchId,
                        allocated,
                        batch.UnitCost);
                }

                InsertReturnLine(
                    connection,
                    transaction,
                    userId,
                    returnId,
                    draft.InvoiceLineId,
                    batch.BatchId,
                    allocatedSaleUnitQuantity,
                    allocated,
                    line.UnitPrice,
                    allocatedValue,
                    draft.ItemCondition);
                remainingSaleUnitQuantity -= allocatedSaleUnitQuantity;
            }

            if (remainingSaleUnitQuantity != 0)
            {
                throw new InvalidOperationException("تعذر تثبيت كامل كمية الإرجاع بوحدة البيع.");
            }

            return lineTotal;
        }

        private static ReturnLineDetails LoadReturnLine(
            SqlConnection connection,
            SqlTransaction transaction,
            long invoiceId,
            long invoiceLineId)
        {
            using (var command = new SqlCommand(
                @"SELECT pu.ProductId, p.Name, u.Name, l.Qty, l.QtyBase,
                         l.UnitPrice, l.LineTotal, pu.FactorToBase,
                         ISNULL(returned.Qty, 0), ISNULL(returned.QtyBase, 0),
                         ISNULL(returned.Total, 0)
                  FROM dbo.SalesInvoiceLines l WITH (UPDLOCK, HOLDLOCK)
                  INNER JOIN dbo.ProductUnits pu ON pu.Id = l.ProductUnitId
                  INNER JOIN dbo.Products p ON p.Id = pu.ProductId
                  INNER JOIN dbo.Units u ON u.Id = pu.UnitId
                  OUTER APPLY
                  (
                      SELECT SUM(rl.Qty) AS Qty, SUM(rl.QtyBase) AS QtyBase,
                             SUM(rl.LineTotal) AS Total
                      FROM dbo.SalesReturnLines rl WITH (UPDLOCK, HOLDLOCK)
                      WHERE rl.InvoiceLineId = l.Id
                  ) returned
                  WHERE l.Id = @InvoiceLineId AND l.InvoiceId = @InvoiceId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                command.Parameters.Add("@InvoiceLineId", SqlDbType.BigInt).Value = invoiceLineId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("أحد أصناف الفاتورة المحددة غير موجود.");
                    }

                    return new ReturnLineDetails
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        UnitName = reader.GetString(2),
                        Quantity = reader.GetDecimal(3),
                        QuantityBase = reader.GetDecimal(4),
                        UnitPrice = reader.GetDecimal(5),
                        LineTotal = reader.GetDecimal(6),
                        FactorToBase = reader.GetDecimal(7),
                        ReturnedQuantity = reader.GetDecimal(8),
                        ReturnedQuantityBase = reader.GetDecimal(9),
                        ReturnedTotal = reader.GetDecimal(10)
                    };
                }
            }
        }

        private static List<ReturnBatch> LoadReturnBatches(
            SqlConnection connection,
            SqlTransaction transaction,
            long invoiceLineId)
        {
            var batches = new List<ReturnBatch>();
            using (var command = new SqlCommand(
                @"WITH Sold AS
                  (
                      SELECT BatchId, SUM(QtyBase) AS QtyBase, MAX(UnitCost) AS UnitCost,
                             MIN(Id) AS AllocationOrder
                      FROM dbo.SalesLineBatches WITH (UPDLOCK, HOLDLOCK)
                      WHERE LineId = @InvoiceLineId
                      GROUP BY BatchId
                  ),
                  Returned AS
                  (
                      SELECT BatchId, SUM(QtyBase) AS QtyBase
                      FROM dbo.SalesReturnLines WITH (UPDLOCK, HOLDLOCK)
                      WHERE InvoiceLineId = @InvoiceLineId
                      GROUP BY BatchId
                  )
                  SELECT s.BatchId, s.QtyBase - ISNULL(r.QtyBase, 0), s.UnitCost
                  FROM Sold s
                  LEFT JOIN Returned r ON r.BatchId = s.BatchId
                  WHERE s.QtyBase > ISNULL(r.QtyBase, 0)
                  ORDER BY s.AllocationOrder;",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceLineId", SqlDbType.BigInt).Value = invoiceLineId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        batches.Add(new ReturnBatch
                        {
                            BatchId = reader.GetInt64(0),
                            RemainingQuantityBase = reader.GetDecimal(1),
                            UnitCost = reader.GetDecimal(2)
                        });
                    }
                }
            }

            return batches;
        }

        private static ReturnInvoiceDetails LoadInvoice(
            SqlConnection connection,
            SqlTransaction transaction,
            long invoiceId)
        {
            using (var command = new SqlCommand(
                @"SELECT i.InvoiceNo, i.CurrencyId, i.Status,
                         c.Code, c.Name, c.DecimalPlaces, c.RoundingMode, c.IsBase,
                         i.CreditAmount, i.Total,
                         ISNULL
                         (
                             (SELECT SUM(sp.AmountInInvoiceCurrency)
                              FROM dbo.SalesPayments sp WITH (HOLDLOCK)
                              WHERE sp.InvoiceId = i.Id),
                             0
                         )
                  FROM dbo.SalesInvoices i WITH (UPDLOCK, HOLDLOCK)
                  INNER JOIN dbo.Currencies c WITH (UPDLOCK, HOLDLOCK) ON c.Id = i.CurrencyId
                  WHERE i.Id = @InvoiceId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = invoiceId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read() || !string.Equals(
                        reader.GetString(2),
                        "Completed",
                        StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("فاتورة البيع غير موجودة أو ليست مكتملة.");
                    }

                    if (reader.GetDecimal(8) > 0)
                    {
                        throw new InvalidOperationException(
                            "لا يمكن رد المبلغ نقدًا لفاتورة بيع آجلة؛ يلزم دعم تسوية دين العميل أولاً.");
                    }

                    if (reader.GetDecimal(10) < reader.GetDecimal(9))
                    {
                        throw new InvalidOperationException(
                            "لا يمكن رد فاتورة لم يُسدد كامل إجماليها نقدًا.");
                    }

                    int currencyId = reader.GetInt32(1);
                    var currency = new SaleCurrency(
                        currencyId,
                        reader.GetString(3),
                        reader.GetString(4),
                        reader.GetInt32(5),
                        reader.GetString(6),
                        reader.GetBoolean(7));
                    return new ReturnInvoiceDetails
                    {
                        InvoiceNo = reader.GetString(0),
                        CurrencyId = currencyId,
                        Currency = currency
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
                    throw new InvalidOperationException("افتح وردية قبل تسجيل مرتجع نقدي.");
                }

                return Convert.ToInt32(value);
            }
        }

        private static void EnsureRefundCurrencyInShift(
            SqlConnection connection,
            SqlTransaction transaction,
            int shiftId,
            int currencyId)
        {
            using (var command = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.ShiftCashCounts WITH (UPDLOCK, HOLDLOCK)
                  WHERE ShiftId = @ShiftId AND CurrencyId = @CurrencyId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currencyId;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException(
                        "عملة فاتورة البيع غير موجودة في أرصدة الوردية الحالية؛ تعذر رد المبلغ نقدًا.");
                }
            }
        }

        private static long InsertReturn(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int shiftId,
            ReturnInvoiceDetails invoice,
            SaleReturnDraft draft,
            string temporaryReturnNo)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.SalesReturns
                    (ReturnNo, InvoiceId, ShiftId, Total, RefundMethod, RefundCurrencyId,
                     Reason, IdempotencyKey, CreatedBy)
                  VALUES
                    (@ReturnNo, @InvoiceId, @ShiftId, 0, N'Cash', @CurrencyId,
                     @Reason, @IdempotencyKey, @CreatedBy);
                  SELECT CAST(SCOPE_IDENTITY() AS bigint);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ReturnNo", SqlDbType.NVarChar, 30).Value = temporaryReturnNo;
                command.Parameters.Add("@InvoiceId", SqlDbType.BigInt).Value = draft.InvoiceId;
                command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = invoice.CurrencyId;
                command.Parameters.Add("@Reason", SqlDbType.NVarChar, 300).Value =
                    string.IsNullOrWhiteSpace(draft.Reason)
                        ? (object)DBNull.Value
                        : draft.Reason.Trim();
                command.Parameters.Add("@IdempotencyKey", SqlDbType.NVarChar, 64).Value =
                    draft.IdempotencyKey;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                return Convert.ToInt64(command.ExecuteScalar());
            }
        }

        private static void InsertReturnLine(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long returnId,
            long invoiceLineId,
            long batchId,
            decimal quantity,
            decimal quantityBase,
            decimal unitPrice,
            decimal lineTotal,
            string itemCondition)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.SalesReturnLines
                    (ReturnId, InvoiceLineId, BatchId, Qty, QtyBase, UnitPrice,
                     LineTotal, ItemCondition, CreatedBy)
                  VALUES
                    (@ReturnId, @InvoiceLineId, @BatchId, @Quantity, @QuantityBase,
                     @UnitPrice, @LineTotal, @ItemCondition, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ReturnId", SqlDbType.BigInt).Value = returnId;
                command.Parameters.Add("@InvoiceLineId", SqlDbType.BigInt).Value = invoiceLineId;
                command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
                AddDecimal(command, "@Quantity", 18, 3, quantity);
                AddDecimal(command, "@QuantityBase", 18, 3, quantityBase);
                AddDecimal(command, "@UnitPrice", 18, 4, unitPrice);
                AddDecimal(command, "@LineTotal", 18, 4, lineTotal);
                command.Parameters.Add("@ItemCondition", SqlDbType.NVarChar, 10).Value = itemCondition;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void RestockBatch(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long batchId,
            decimal quantityBase)
        {
            using (var command = new SqlCommand(
                @"UPDATE dbo.Batches
                  SET QtyRemaining = QtyRemaining + @Quantity,
                      UpdatedAt = SYSDATETIME(),
                      UpdatedBy = @UpdatedBy
                  WHERE Id = @BatchId
                    AND QtyRemaining <= @MaximumQuantity - @Quantity;",
                connection,
                transaction))
            {
                AddDecimal(command, "@Quantity", 18, 3, quantityBase);
                AddDecimal(command, "@MaximumQuantity", 18, 3, MaximumQuantity);
                command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("رصيد دفعة المخزون خارج الحد المسموح لإرجاع الكمية.");
                }
            }
        }

        private static void InsertReturnStockMovement(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long returnId,
            int productId,
            long batchId,
            decimal quantityBase,
            decimal unitCost)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.StockMovements
                    (ProductId, BatchId, MovementType, Qty, UnitCost, RefType, RefId, CreatedBy)
                  VALUES
                    (@ProductId, @BatchId, N'SaleReturn', @Quantity, @UnitCost,
                     N'SalesReturn', @ReturnId, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                command.Parameters.Add("@BatchId", SqlDbType.BigInt).Value = batchId;
                AddDecimal(command, "@Quantity", 18, 3, quantityBase);
                AddDecimal(command, "@UnitCost", 18, 4, unitCost);
                command.Parameters.Add("@ReturnId", SqlDbType.BigInt).Value = returnId;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static void InsertCashRefund(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int shiftId,
            int currencyId,
            decimal amount,
            string invoiceNo)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.CashMovements
                    (ShiftId, CurrencyId, Direction, Amount, Reason, CreatedBy)
                  VALUES
                    (@ShiftId, @CurrencyId, N'Out', @Amount, @Reason, @CreatedBy);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currencyId;
                AddDecimal(command, "@Amount", 18, 4, amount);
                command.Parameters.Add("@Reason", SqlDbType.NVarChar, 300).Value =
                    "رد نقدي للفاتورة " + invoiceNo;
                command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static string GetNextReturnNumber(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            using (var command = new SqlCommand("dbo.usp_GetNextDocumentNumber", connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = "SalesReturn";
                command.Parameters.Add("@Number", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                SqlParameter formatted = command.Parameters.Add("@Formatted", SqlDbType.NVarChar, 50);
                formatted.Direction = ParameterDirection.Output;
                command.ExecuteNonQuery();
                if (formatted.Value == null || formatted.Value == DBNull.Value)
                {
                    throw new InvalidOperationException("تعذر إنشاء رقم مرتجع المبيعات.");
                }

                return Convert.ToString(formatted.Value, CultureInfo.InvariantCulture);
            }
        }

        private static void UpdateReturn(
            SqlConnection connection,
            SqlTransaction transaction,
            long returnId,
            string returnNo,
            decimal total)
        {
            using (var command = new SqlCommand(
                @"UPDATE dbo.SalesReturns
                  SET ReturnNo = @ReturnNo, Total = @Total
                  WHERE Id = @ReturnId;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ReturnNo", SqlDbType.NVarChar, 30).Value = returnNo;
                AddDecimal(command, "@Total", 18, 4, total);
                command.Parameters.Add("@ReturnId", SqlDbType.BigInt).Value = returnId;
                if (command.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("تعذر تثبيت بيانات مرتجع المبيعات.");
                }
            }
        }

        private static void InsertReturnAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            long returnId,
            string invoiceNo,
            string returnNo,
            decimal total,
            string currencyCode,
            int lineCount,
            string reason)
        {
            string newValues = string.Format(
                CultureInfo.InvariantCulture,
                "{{\"ReturnNo\":\"{0}\",\"InvoiceNo\":\"{1}\",\"Total\":{2},\"Currency\":\"{3}\",\"LineCount\":{4},\"Reason\":\"{5}\"}}",
                EscapeJson(returnNo),
                EscapeJson(invoiceNo),
                total,
                EscapeJson(currencyCode),
                lineCount,
                EscapeJson(reason));
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs (UserId, Action, EntityName, EntityId, NewValues)
                  VALUES (@UserId, N'SaleReturned', N'SalesReturns', @ReturnId, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@ReturnId", SqlDbType.NVarChar, 100).Value =
                    returnId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static void ValidateDraft(SaleReturnDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException("draft");
            }

            if (draft.InvoiceId <= 0)
            {
                throw new ArgumentException("اختر فاتورة البيع المراد إرجاعها.");
            }

            if (string.IsNullOrWhiteSpace(draft.IdempotencyKey) ||
                draft.IdempotencyKey.Length > 64)
            {
                throw new ArgumentException("مفتاح منع تكرار المرتجع غير صالح.");
            }

            if (draft.Reason != null && draft.Reason.Trim().Length > 300)
            {
                throw new ArgumentException("سبب الإرجاع لا يتجاوز 300 حرف.");
            }

            if (draft.Lines == null || draft.Lines.Count == 0 || draft.Lines.Count > 500)
            {
                throw new ArgumentException("حدد صنفًا واحدًا على الأقل لإرجاعه.");
            }

            if (draft.Lines.Any(line =>
                line == null ||
                line.InvoiceLineId <= 0 ||
                line.Quantity <= 0 ||
                line.Quantity > MaximumQuantity ||
                decimal.Round(line.Quantity, 3) != line.Quantity ||
                (line.ItemCondition != "Good" && line.ItemCondition != "Damaged")))
            {
                throw new ArgumentException("توجد كمية أو حالة صنف غير صالحة في المرتجع.");
            }

            if (draft.Lines.GroupBy(line => new { line.InvoiceLineId, line.ItemCondition })
                .Any(group => group.Count() > 1))
            {
                throw new ArgumentException("لا تكرر الصنف والحالة نفسيهما في المرتجع.");
            }
        }

        private static void RequireReturnPermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Sales.Return"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية تسجيل مرتجعات المبيعات.");
            }
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
    }
}
