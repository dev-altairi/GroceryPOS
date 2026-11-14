using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class CashMovementForm : Form
    {
        public CashMovementForm()
        {
            InitializeComponent();
        }

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
}
