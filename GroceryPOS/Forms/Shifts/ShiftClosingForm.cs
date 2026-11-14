using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ShiftClosingForm : Form
    {
        private BindingList<ShiftClosingRow> rows;

        public ShiftClosingForm()
        {
            InitializeComponent();
            rows = new BindingList<ShiftClosingRow>();
        }

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
