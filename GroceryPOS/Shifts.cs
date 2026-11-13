using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace GroceryPOS
{
    internal sealed class ShiftCurrency
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public int DecimalPlaces { get; private set; }

        internal ShiftCurrency(int id, string code, string name, int decimalPlaces)
        {
            Id = id;
            Code = code;
            Name = name;
            DecimalPlaces = decimalPlaces;
        }

        public override string ToString()
        {
            return Code + " - " + Name;
        }
    }

    internal sealed class ShiftCashAmount
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public int DecimalPlaces { get; set; }
        public decimal OpeningAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal? ActualAmount { get; set; }
        public decimal? Difference { get; set; }
    }

    internal sealed class ShiftRecord
    {
        public int Id { get; set; }
        public int CashierId { get; set; }
        public string CashierName { get; set; }
        public string DeviceName { get; set; }
        public DateTime OpenedAt { get; set; }
        public string Status { get; set; }
        public IList<ShiftCashAmount> CashAmounts { get; set; }
    }

    internal sealed class ShiftOpeningAmount
    {
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
    }

    internal sealed class ShiftClosingAmount
    {
        public int CurrencyId { get; set; }
        public decimal ActualAmount { get; set; }
        public string DifferenceReason { get; set; }
    }

    internal static class ShiftsService
    {
        private const decimal MaximumAmount = 99999999999999.9999m;

        internal static List<ShiftCurrency> LoadActiveCurrencies(AuthenticatedUser user)
        {
            RequireOperatePermission(user);
            var currencies = new List<ShiftCurrency>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT Id, Code, Name, DecimalPlaces
                  FROM dbo.Currencies
                  WHERE IsActive = 1
                  ORDER BY IsBase DESC, Code;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    currencies.Add(new ShiftCurrency(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetInt32(3)));
                }
            }

            if (currencies.Count == 0)
            {
                throw new InvalidOperationException("فعّل عملة واحدة على الأقل قبل فتح الوردية.");
            }

            return currencies;
        }

        internal static List<ShiftRecord> LoadOpenShifts(AuthenticatedUser user)
        {
            RequireOperatePermission(user);
            var shifts = new List<ShiftRecord>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT s.Id, s.CashierId, u.FullName, s.DeviceName, s.OpenedAt,
                         sc.CurrencyId, c.Code, c.Name, c.DecimalPlaces, sc.OpeningAmount
                  FROM dbo.Shifts s
                  INNER JOIN dbo.Users u ON u.Id = s.CashierId
                  LEFT JOIN dbo.ShiftCashCounts sc ON sc.ShiftId = s.Id
                  LEFT JOIN dbo.Currencies c ON c.Id = sc.CurrencyId
                  WHERE s.Status = N'Open'
                    AND (@CanApprove = 1 OR s.CashierId = @UserId)
                  ORDER BY s.OpenedAt, s.Id, c.IsBase DESC, c.Code;",
                connection))
            {
                command.Parameters.Add("@CanApprove", SqlDbType.Bit).Value =
                    user.HasPermission("Shifts.ApproveDifference");
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int shiftId = reader.GetInt32(0);
                        ShiftRecord shift = shifts.FirstOrDefault(item => item.Id == shiftId);
                        if (shift == null)
                        {
                            shift = new ShiftRecord
                            {
                                Id = shiftId,
                                CashierId = reader.GetInt32(1),
                                CashierName = reader.GetString(2),
                                DeviceName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                OpenedAt = reader.GetDateTime(4),
                                Status = "Open",
                                CashAmounts = new List<ShiftCashAmount>()
                            };
                            shifts.Add(shift);
                        }

                        if (!reader.IsDBNull(5))
                        {
                            shift.CashAmounts.Add(new ShiftCashAmount
                            {
                                CurrencyId = reader.GetInt32(5),
                                CurrencyCode = reader.GetString(6),
                                CurrencyName = reader.GetString(7),
                                DecimalPlaces = reader.GetInt32(8),
                                OpeningAmount = reader.GetDecimal(9)
                            });
                        }
                    }
                }
            }

            return shifts;
        }

        internal static void OpenShift(
            AuthenticatedUser user,
            IList<ShiftOpeningAmount> openingAmounts)
        {
            RequireOperatePermission(user);
            ValidateOpeningAmounts(openingAmounts);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                using (var command = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.Shifts WITH (UPDLOCK, HOLDLOCK) WHERE CashierId = @UserId AND Status = N'Open';",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                    if (Convert.ToInt32(command.ExecuteScalar()) != 0)
                    {
                        throw new InvalidOperationException("لديك وردية مفتوحة بالفعل.");
                    }
                }

                EnsureActiveCurrencies(connection, transaction, openingAmounts);

                int shiftId;
                using (var command = new SqlCommand(
                    @"INSERT dbo.Shifts (CashierId, DeviceName, CreatedBy)
                      VALUES (@CashierId, @DeviceName, @CreatedBy);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@CashierId", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@DeviceName", SqlDbType.NVarChar, 100).Value =
                        Environment.MachineName;
                    command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                    shiftId = Convert.ToInt32(command.ExecuteScalar());
                }

                foreach (ShiftOpeningAmount opening in openingAmounts)
                {
                    using (var command = new SqlCommand(
                        @"INSERT dbo.ShiftCashCounts (ShiftId, CurrencyId, OpeningAmount, CreatedBy)
                          VALUES (@ShiftId, @CurrencyId, @OpeningAmount, @CreatedBy);",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                        command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = opening.CurrencyId;
                        AddAmount(command, "@OpeningAmount", opening.Amount);
                        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                        command.ExecuteNonQuery();
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    shiftId,
                    "ShiftOpened",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{{\"OpeningCurrencies\":{0}}}",
                        openingAmounts.Count));
                transaction.Commit();
            }
        }

        internal static void AddCashMovement(
            AuthenticatedUser user,
            int shiftId,
            int currencyId,
            bool isCashIn,
            decimal amount,
            string reason)
        {
            RequireOperatePermission(user);
            if (user == null || !user.HasPermission("Shifts.CashMovement"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية تسجيل حركة على الصندوق.");
            }

            reason = (reason ?? string.Empty).Trim();
            ValidateAmount(amount, "مبلغ الحركة");
            if (reason.Length == 0 || reason.Length > 300)
            {
                throw new ArgumentException("سبب الحركة مطلوب ولا يتجاوز 300 حرف.");
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                EnsureOpenShiftAccess(connection, transaction, user, shiftId, false);
                using (var command = new SqlCommand(
                    @"SELECT DecimalPlaces FROM dbo.Currencies WITH (UPDLOCK, HOLDLOCK)
                      WHERE Id = @CurrencyId AND IsActive = 1;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currencyId;
                    object decimalPlaces = command.ExecuteScalar();
                    if (decimalPlaces == null || decimalPlaces == DBNull.Value)
                    {
                        throw new InvalidOperationException("عملة الحركة غير موجودة أو غير نشطة.");
                    }

                    ValidateCurrencyPrecision(
                        amount,
                        Convert.ToInt32(decimalPlaces),
                        "مبلغ الحركة");
                }

                ShiftCashAmount cash = LoadExpectedCash(connection, transaction, shiftId)
                    .FirstOrDefault(currencyAmount => currencyAmount.CurrencyId == currencyId);
                if (cash == null)
                {
                    throw new InvalidOperationException("عملة الحركة غير مسجلة في أرصدة الوردية.");
                }

                if (!isCashIn && amount > cash.ExpectedAmount)
                {
                    throw new ArgumentException("مبلغ السحب يتجاوز الرصيد النقدي المتوقع في الصندوق.");
                }

                if (isCashIn && amount > MaximumAmount - cash.ExpectedAmount)
                {
                    throw new ArgumentException("مبلغ الإيداع يتجاوز الحد الأقصى لرصيد العملة.");
                }

                using (var command = new SqlCommand(
                    @"INSERT dbo.CashMovements
                        (ShiftId, CurrencyId, Direction, Amount, Reason, CreatedBy)
                      VALUES
                        (@ShiftId, @CurrencyId, @Direction, @Amount, @Reason, @CreatedBy);",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                    command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = currencyId;
                    command.Parameters.Add("@Direction", SqlDbType.NVarChar, 3).Value =
                        isCashIn ? "In" : "Out";
                    AddAmount(command, "@Amount", amount);
                    command.Parameters.Add("@Reason", SqlDbType.NVarChar, 300).Value = reason;
                    command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                    command.ExecuteNonQuery();
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    shiftId,
                    isCashIn ? "ShiftCashIn" : "ShiftCashOut",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{{\"CurrencyId\":{0},\"Amount\":{1},\"Reason\":\"{2}\"}}",
                        currencyId,
                        amount,
                        EscapeJson(reason)));
                transaction.Commit();
            }
        }

        internal static IList<ShiftCashAmount> LoadExpectedCash(AuthenticatedUser user, int shiftId)
        {
            RequireOperatePermission(user);
            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                EnsureOpenShiftAccess(connection, transaction, user, shiftId, true);
                IList<ShiftCashAmount> result = LoadExpectedCash(connection, transaction, shiftId);
                transaction.Commit();
                return result;
            }
        }

        internal static void CloseShift(
            AuthenticatedUser user,
            int shiftId,
            IList<ShiftClosingAmount> closingAmounts,
            string closeNotes)
        {
            RequireOperatePermission(user);
            closeNotes = (closeNotes ?? string.Empty).Trim();
            if (closeNotes.Length > 500)
            {
                throw new ArgumentException("ملاحظات الإغلاق لا تتجاوز 500 حرف.");
            }

            ValidateClosingAmounts(closingAmounts);
            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                EnsureOpenShiftAccess(connection, transaction, user, shiftId, true);
                IList<ShiftCashAmount> expectedAmounts =
                    LoadExpectedCash(connection, transaction, shiftId);
                EnsureClosingCurrencies(expectedAmounts, closingAmounts);

                bool hasDifference = false;
                foreach (ShiftCashAmount expected in expectedAmounts)
                {
                    ShiftClosingAmount actual = closingAmounts.First(item => item.CurrencyId == expected.CurrencyId);
                    ValidateCurrencyPrecision(
                        actual.ActualAmount,
                        expected.DecimalPlaces,
                        "العدّ الفعلي بعملة " + expected.CurrencyCode);
                    decimal difference = actual.ActualAmount - expected.ExpectedAmount;
                    actual.DifferenceReason = (actual.DifferenceReason ?? string.Empty).Trim();
                    if (difference != 0)
                    {
                        hasDifference = true;
                        if (actual.DifferenceReason.Length == 0 || actual.DifferenceReason.Length > 300)
                        {
                            throw new ArgumentException("أدخل سببًا لاختلاف الصندوق لكل عملة يظهر فيها فرق.");
                        }
                    }
                    else if (actual.DifferenceReason.Length > 300)
                    {
                        throw new ArgumentException("سبب فرق الصندوق لا يتجاوز 300 حرف.");
                    }

                    using (var command = new SqlCommand(
                        @"UPDATE dbo.ShiftCashCounts
                          SET ExpectedAmount = @ExpectedAmount,
                              ActualAmount = @ActualAmount,
                              Difference = @Difference,
                              DifferenceReason = @DifferenceReason,
                              UpdatedAt = SYSDATETIME(),
                              UpdatedBy = @UpdatedBy
                          WHERE ShiftId = @ShiftId AND CurrencyId = @CurrencyId;",
                        connection,
                        transaction))
                    {
                        AddAmount(command, "@ExpectedAmount", expected.ExpectedAmount);
                        AddAmount(command, "@ActualAmount", actual.ActualAmount);
                        AddAmount(command, "@Difference", difference);
                        command.Parameters.Add("@DifferenceReason", SqlDbType.NVarChar, 300).Value =
                            actual.DifferenceReason.Length == 0
                                ? (object)DBNull.Value
                                : actual.DifferenceReason;
                        command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                        command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                        command.Parameters.Add("@CurrencyId", SqlDbType.Int).Value = expected.CurrencyId;
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("تعذر حفظ عدّ النقد لإحدى العملات.");
                        }
                    }
                }

                bool approvedDifference = hasDifference && user.HasPermission("Shifts.ApproveDifference");
                if (hasDifference && !approvedDifference)
                {
                    throw new UnauthorizedAccessException(
                        "يوجد فرق في الصندوق. يلزم أن يراجع مدير مخوّل العدّ ويعتمد إغلاق الوردية.");
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Shifts
                      SET Status = N'Closed',
                          ClosedAt = SYSDATETIME(),
                          CloseNotes = @CloseNotes,
                          DifferenceApprovedBy = @ApprovedBy,
                          UpdatedAt = SYSDATETIME(),
                          UpdatedBy = @UpdatedBy
                      WHERE Id = @ShiftId AND Status = N'Open';",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@CloseNotes", SqlDbType.NVarChar, 500).Value =
                        closeNotes.Length == 0 ? (object)DBNull.Value : closeNotes;
                    command.Parameters.Add("@ApprovedBy", SqlDbType.Int).Value =
                        approvedDifference ? (object)user.Id : DBNull.Value;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("الوردية لم تعد مفتوحة.");
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    shiftId,
                    "ShiftClosed",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{{\"CurrencyCount\":{0},\"HasDifference\":{1},\"DifferenceApproved\":{2}}}",
                        expectedAmounts.Count,
                        hasDifference ? "true" : "false",
                        approvedDifference ? "true" : "false"));
                transaction.Commit();
            }
        }

        private static IList<ShiftCashAmount> LoadExpectedCash(
            SqlConnection connection,
            SqlTransaction transaction,
            int shiftId)
        {
            using (var command = new SqlCommand(
                @"IF EXISTS
                  (
                      SELECT 1
                      FROM dbo.SalesReturns r WITH (HOLDLOCK)
                      WHERE r.ShiftId = @ShiftId AND r.RefundMethod = N'Cash'
                  )
                      THROW 51020, N'إغلاق وردية بها مرتجعات نقدية غير مدعوم حتى اكتمال احتساب مبالغ الرد بعملة الصندوق.', 1;

                  SELECT sc.CurrencyId, c.Code, c.Name, c.DecimalPlaces, sc.OpeningAmount,
                         CONVERT(decimal(28,4), sc.OpeningAmount
                         + ISNULL(movement.Amount, 0)
                         + ISNULL(sale.AmountPaid, 0)
                         - ISNULL(change.Amount, 0)
                         + ISNULL(receipt.AmountPaid, 0))
                  FROM dbo.ShiftCashCounts sc WITH (UPDLOCK, HOLDLOCK)
                  INNER JOIN dbo.Currencies c ON c.Id = sc.CurrencyId
                  OUTER APPLY
                  (
                      SELECT SUM(CASE WHEN cm.Direction = N'In' THEN cm.Amount ELSE -cm.Amount END) AS Amount
                      FROM dbo.CashMovements cm WITH (HOLDLOCK)
                      WHERE cm.ShiftId = sc.ShiftId AND cm.CurrencyId = sc.CurrencyId
                  ) movement
                  OUTER APPLY
                  (
                      SELECT SUM(sp.AmountPaid) AS AmountPaid
                      FROM dbo.SalesPayments sp WITH (HOLDLOCK)
                      INNER JOIN dbo.SalesInvoices si ON si.Id = sp.InvoiceId AND si.Status = N'Completed'
                      WHERE si.ShiftId = sc.ShiftId AND sp.PaymentCurrencyId = sc.CurrencyId
                  ) sale
                  OUTER APPLY
                  (
                      SELECT SUM(si.ChangeAmount) AS Amount
                      FROM dbo.SalesInvoices si WITH (HOLDLOCK)
                      WHERE si.ShiftId = sc.ShiftId
                        AND si.Status = N'Completed'
                        AND si.ChangeCurrencyId = sc.CurrencyId
                  ) change
                  OUTER APPLY
                  (
                      SELECT SUM(CASE WHEN cp.Status = N'Reversal' THEN -cp.AmountPaid ELSE cp.AmountPaid END) AS AmountPaid
                      FROM dbo.CustomerPayments cp WITH (HOLDLOCK)
                      WHERE cp.ShiftId = sc.ShiftId
                        AND cp.PaidCurrencyId = sc.CurrencyId
                        AND cp.Status IN (N'Posted', N'Reversal')
                  ) receipt
                  WHERE sc.ShiftId = @ShiftId
                  ORDER BY c.IsBase DESC, c.Code;",
                connection,
                transaction))
            {
                command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                var amounts = new List<ShiftCashAmount>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal expected = reader.GetDecimal(5);
                        if (expected < 0 || expected > MaximumAmount)
                        {
                            throw new InvalidOperationException(
                                "الرصيد النقدي المتوقع لعملة " + reader.GetString(1) +
                                " خارج النطاق؛ راجع حركات الوردية قبل الإغلاق.");
                        }

                        ValidateCurrencyPrecision(
                            expected,
                            reader.GetInt32(3),
                            "الرصيد المتوقع بعملة " + reader.GetString(1));
                        amounts.Add(new ShiftCashAmount
                        {
                            CurrencyId = reader.GetInt32(0),
                            CurrencyCode = reader.GetString(1),
                            CurrencyName = reader.GetString(2),
                            DecimalPlaces = reader.GetInt32(3),
                            OpeningAmount = reader.GetDecimal(4),
                            ExpectedAmount = expected
                        });
                    }
                }

                if (amounts.Count == 0)
                {
                    throw new InvalidOperationException("لا توجد أرصدة نقدية مسجلة لهذه الوردية.");
                }

                return amounts;
            }
        }

        private static void EnsureOpenShiftAccess(
            SqlConnection connection,
            SqlTransaction transaction,
            AuthenticatedUser user,
            int shiftId,
            bool allowApproval)
        {
            using (var command = new SqlCommand(
                @"SELECT CashierId
                  FROM dbo.Shifts WITH (UPDLOCK, HOLDLOCK)
                  WHERE Id = @ShiftId AND Status = N'Open';",
                connection,
                transaction))
            {
                command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
                object cashierId = command.ExecuteScalar();
                if (cashierId == null || cashierId == DBNull.Value)
                {
                    throw new InvalidOperationException("الوردية غير موجودة أو مغلقة.");
                }

                if (Convert.ToInt32(cashierId) != user.Id &&
                    (!allowApproval || !user.HasPermission("Shifts.ApproveDifference")))
                {
                    throw new UnauthorizedAccessException("لا تملك صلاحية تشغيل هذه الوردية.");
                }
            }
        }

        private static void EnsureActiveCurrencies(
            SqlConnection connection,
            SqlTransaction transaction,
            IList<ShiftOpeningAmount> amounts)
        {
            using (var command = new SqlCommand(
                "SELECT Id, DecimalPlaces FROM dbo.Currencies WITH (UPDLOCK, HOLDLOCK) WHERE IsActive = 1;",
                connection,
                transaction))
            {
                var activeCurrencies = new Dictionary<int, int>();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        activeCurrencies.Add(reader.GetInt32(0), reader.GetInt32(1));
                    }
                }

                if (activeCurrencies.Count == 0 ||
                    !new HashSet<int>(activeCurrencies.Keys).SetEquals(
                        amounts.Select(amount => amount.CurrencyId)))
                {
                    throw new InvalidOperationException(
                        "تغيرت العملات النشطة. حدّث قائمة العملات ثم أعد فتح الوردية.");
                }

                foreach (ShiftOpeningAmount amount in amounts)
                {
                    ValidateCurrencyPrecision(
                        amount.Amount,
                        activeCurrencies[amount.CurrencyId],
                        "رصيد بداية الوردية");
                }
            }
        }

        private static void ValidateOpeningAmounts(IList<ShiftOpeningAmount> amounts)
        {
            if (amounts == null || amounts.Count == 0 ||
                amounts.Any(amount => amount == null || amount.CurrencyId <= 0))
            {
                throw new ArgumentException("أدخل رصيد بداية الوردية لكل عملة نشطة.");
            }

            if (amounts.Select(amount => amount.CurrencyId).Distinct().Count() != amounts.Count)
            {
                throw new ArgumentException("لا تكرر العملة في أرصدة بداية الوردية.");
            }

            foreach (ShiftOpeningAmount amount in amounts)
            {
                ValidateNonNegativeAmount(amount.Amount, "رصيد بداية الوردية");
            }
        }

        private static void ValidateClosingAmounts(IList<ShiftClosingAmount> amounts)
        {
            if (amounts == null || amounts.Count == 0 ||
                amounts.Any(amount => amount == null || amount.CurrencyId <= 0))
            {
                throw new ArgumentException("أدخل العدّ الفعلي لكل عملة في الوردية.");
            }

            if (amounts.Select(amount => amount.CurrencyId).Distinct().Count() != amounts.Count)
            {
                throw new ArgumentException("لا تكرر العملة في العدّ الفعلي.");
            }

            foreach (ShiftClosingAmount amount in amounts)
            {
                ValidateNonNegativeAmount(amount.ActualAmount, "العدّ الفعلي");
            }
        }

        private static void EnsureClosingCurrencies(
            IList<ShiftCashAmount> expected,
            IList<ShiftClosingAmount> actual)
        {
            if (!new HashSet<int>(expected.Select(amount => amount.CurrencyId))
                .SetEquals(actual.Select(amount => amount.CurrencyId)))
            {
                throw new ArgumentException("قائمة العملات تغيرت؛ حدّث الوردية وأعد إدخال العدّ الفعلي.");
            }
        }

        private static void ValidateAmount(decimal amount, string label)
        {
            if (amount <= 0 || amount > MaximumAmount || decimal.Round(amount, 4) != amount)
            {
                throw new ArgumentException(label + " يجب أن يكون موجبًا وبحد أقصى 4 خانات عشرية.");
            }
        }

        private static void ValidateNonNegativeAmount(decimal amount, string label)
        {
            if (amount < 0 || amount > MaximumAmount || decimal.Round(amount, 4) != amount)
            {
                throw new ArgumentException(label + " يجب أن يكون غير سالب وبحد أقصى 4 خانات عشرية.");
            }
        }

        private static void ValidateCurrencyPrecision(decimal amount, int decimalPlaces, string label)
        {
            if (decimalPlaces < 0 || decimalPlaces > 6)
            {
                throw new InvalidOperationException("دقة العملة خارج النطاق المدعوم.");
            }

            decimal smallestUnit = 1m;
            for (int i = 0; i < decimalPlaces; i++)
            {
                smallestUnit /= 10m;
            }

            if (decimal.Remainder(amount, smallestUnit) != 0)
            {
                throw new ArgumentException(
                    label + " يحتوي منازل عشرية أكثر من دقة العملة المحددة.");
            }
        }

        private static void AddAmount(SqlCommand command, string name, decimal amount)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = 18;
            parameter.Scale = 4;
            parameter.Value = amount;
        }

        private static void InsertAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            int shiftId,
            string action,
            string newValues)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs (UserId, Action, EntityName, EntityId, NewValues)
                  VALUES (@UserId, @Action, N'Shifts', @ShiftId, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = action;
                command.Parameters.Add("@ShiftId", SqlDbType.NVarChar, 100).Value =
                    shiftId.ToString(CultureInfo.InvariantCulture);
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
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

        private static void RequireOperatePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Shifts.Operate"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية تشغيل الورديات.");
            }
        }
    }
}
