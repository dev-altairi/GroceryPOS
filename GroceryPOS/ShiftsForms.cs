using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ShiftManagementForm : Form
    {
        private AuthenticatedUser user;

        public ShiftManagementForm()
        {
            InitializeComponent();
        }

        internal ShiftManagementForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            openButton.Visible = user.HasPermission("Shifts.Operate");
            openButton.Click += OpenButton_Click;
            movementButton.Visible = user.HasPermission("Shifts.CashMovement");
            movementButton.Click += MovementButton_Click;
            closeButton.Visible = user.HasPermission("Shifts.Operate");
            closeButton.Click += CloseButton_Click;
            refreshButton.Click += (sender, args) => RefreshShifts();
            closeWindowButton.Click += (sender, args) => Close();
            grid.SelectionChanged += (sender, args) => UpdateSelection();
            Shown += (sender, args) => RefreshShifts();
        }

        private ShiftRecord SelectedShift()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as ShiftRecord;
        }

        private void UpdateSelection()
        {
            ShiftRecord shift = SelectedShift();
            bool selected = shift != null;
            bool ownShift = selected && shift.CashierId == user.Id;
            movementButton.Enabled = ownShift && user.HasPermission("Shifts.CashMovement");
            closeButton.Enabled = selected &&
                                  (ownShift || user.HasPermission("Shifts.ApproveDifference"));
            openButton.Enabled = !((grid.DataSource as List<ShiftRecord> ?? new List<ShiftRecord>())
                .Any(item => item.CashierId == user.Id));
        }

        private void RefreshShifts()
        {
            try
            {
                List<ShiftRecord> shifts = ShiftsService.LoadOpenShifts(user);
                grid.DataSource = shifts;
                status.Text = "الورديات المفتوحة: " + shifts.Count.ToString(CultureInfo.CurrentCulture);
                UpdateSelection();
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل الورديات.");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<ShiftCurrency> currencies = ShiftsService.LoadActiveCurrencies(user);
                using (var editor = new ShiftOpeningForm(currencies))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        ShiftsService.OpenShift(user, editor.OpeningAmounts);
                        RefreshShifts();
                    }
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر فتح الوردية.");
            }
        }

        private void MovementButton_Click(object sender, EventArgs e)
        {
            ShiftRecord shift = SelectedShift();
            if (shift == null)
            {
                return;
            }

            try
            {
                List<ShiftCurrency> currencies = ShiftsService.LoadActiveCurrencies(user);
                using (var editor = new CashMovementForm(currencies))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        ShiftsService.AddCashMovement(
                            user,
                            shift.Id,
                            editor.CurrencyId,
                            editor.IsCashIn,
                            editor.Amount,
                            editor.Reason);
                        RefreshShifts();
                    }
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تسجيل حركة الصندوق.");
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ShiftRecord shift = SelectedShift();
            if (shift == null)
            {
                return;
            }

            try
            {
                IList<ShiftCashAmount> expected = ShiftsService.LoadExpectedCash(user, shift.Id);
                using (var editor = new ShiftClosingForm(shift, expected))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        ShiftsService.CloseShift(
                            user,
                            shift.Id,
                            editor.ClosingAmounts,
                            editor.CloseNotes);
                        MessageBox.Show(
                            "تم إغلاق الوردية وحفظ العدّ والمطابقة.",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        RefreshShifts();
                    }
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر إغلاق الوردية.");
            }
        }
    }

    internal sealed class ShiftOpeningRow
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public decimal Amount { get; set; }
    }

    public partial class ShiftOpeningForm : Form
    {
        private BindingList<ShiftOpeningRow> rows;

        internal IList<ShiftOpeningAmount> OpeningAmounts
        {
            get
            {
                return rows.Select(row => new ShiftOpeningAmount
                {
                    CurrencyId = row.CurrencyId,
                    Amount = row.Amount
                }).ToList();
            }
        }

        public ShiftOpeningForm()
        {
            InitializeComponent();
            rows = new BindingList<ShiftOpeningRow>();
            grid.DataSource = rows;
        }

        internal ShiftOpeningForm(IList<ShiftCurrency> currencies)
            : this()
        {
            foreach (ShiftCurrency currency in currencies)
            {
                rows.Add(new ShiftOpeningRow
                {
                    CurrencyId = currency.Id,
                    CurrencyCode = currency.Code,
                    CurrencyName = currency.Name,
                    Amount = 0m
                });
            }

            grid.DataError += Grid_DataError;
            saveButton.Click += Save_Click;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                grid.EndEdit();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                ShowInputError(error);
            }
        }

        private void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                e.ThrowException = false;
                PurchasingUiErrors.ShowLogged(
                    this,
                    e.Exception,
                    "القيمة المدخلة غير صالحة. راجع المبلغ والسبب.");
            }
        }

        private void ShowInputError(Exception error)
        {
            MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public partial class CashMovementForm : Form
    {
        internal int CurrencyId
        {
            get
            {
                ShiftCurrency currency = currencyComboBox.SelectedItem as ShiftCurrency;
                return currency == null ? 0 : currency.Id;
            }
        }

        internal bool IsCashIn { get { return cashInRadio.Checked; } }
        internal decimal Amount { get { return amountInput.Value; } }
        internal string Reason { get { return reasonInput.Text; } }

        public CashMovementForm()
        {
            InitializeComponent();
        }

        internal CashMovementForm(IList<ShiftCurrency> currencies)
            : this()
        {
            currencyComboBox.DataSource = new List<ShiftCurrency>(currencies);
            saveButton.Click += Save_Click;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (CurrencyId <= 0 || Amount <= 0 || string.IsNullOrWhiteSpace(Reason))
            {
                MessageBox.Show("اختر العملة وأدخل مبلغًا موجبًا وسبب الحركة.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal sealed class ShiftClosingRow
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public string DifferenceReason { get; set; }
    }

    public partial class ShiftClosingForm : Form
    {
        private BindingList<ShiftClosingRow> rows;

        internal IList<ShiftClosingAmount> ClosingAmounts
        {
            get
            {
                return rows.Select(row => new ShiftClosingAmount
                {
                    CurrencyId = row.CurrencyId,
                    ActualAmount = row.ActualAmount,
                    DifferenceReason = row.DifferenceReason
                }).ToList();
            }
        }

        internal string CloseNotes { get { return notesInput.Text; } }

        public ShiftClosingForm()
        {
            InitializeComponent();
            rows = new BindingList<ShiftClosingRow>();
            grid.DataSource = rows;
        }

        internal ShiftClosingForm(ShiftRecord shift, IList<ShiftCashAmount> expected)
            : this()
        {
            Text = "إغلاق الوردية — " + shift.CashierName;
            foreach (ShiftCashAmount amount in expected)
            {
                rows.Add(new ShiftClosingRow
                {
                    CurrencyId = amount.CurrencyId,
                    CurrencyCode = amount.CurrencyCode,
                    CurrencyName = amount.CurrencyName,
                    ExpectedAmount = amount.ExpectedAmount,
                    ActualAmount = amount.ExpectedAmount,
                    DifferenceReason = string.Empty
                });
            }

            grid.DataError += Grid_DataError;
            saveButton.Click += Save_Click;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                grid.EndEdit();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception != null)
            {
                e.ThrowException = false;
                PurchasingUiErrors.ShowLogged(
                    this,
                    e.Exception,
                    "القيمة المدخلة غير صالحة. راجع المبلغ والسبب.");
            }
        }
    }

}
