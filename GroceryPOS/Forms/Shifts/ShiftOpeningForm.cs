using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ShiftOpeningForm : Form
    {
        private BindingList<ShiftOpeningRow> rows;

        public ShiftOpeningForm()
        {
            InitializeComponent();
            rows = new BindingList<ShiftOpeningRow>();
        }

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
}
