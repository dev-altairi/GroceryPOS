using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SalePaymentForm : Form
    {
        private SaleCurrency baseCurrency;
        private readonly BindingList<SalePaymentRow> rows;

        public SalePaymentForm()
        {
            InitializeComponent();
            rows = new BindingList<SalePaymentRow>();
        }

        internal IList<SalePaymentDraft> Payments
        {
            get
            {
                return rows.Where(row => row.AmountPaid > 0)
                    .Select(row => new SalePaymentDraft
                    {
                        CurrencyId = row.CurrencyId,
                        AmountPaid = row.AmountPaid
                    }).ToList();
            }
        }

        internal int? ChangeCurrencyId
        {
            get
            {
                SaleCurrency currency = changeCurrencyComboBox.SelectedItem as SaleCurrency;
                return currency == null ? (int?)null : currency.Id;
            }
        }

        internal SalePaymentForm(
            decimal total,
            SaleCurrency baseCurrencyOption,
            IList<SaleCurrency> currencies)
            : this()
        {
            baseCurrency = baseCurrencyOption;
            requiredLabel.Text = "المطلوب: " + total.ToString("N4", CultureInfo.CurrentCulture) +
                                 " " + baseCurrency.Code;
            foreach (SaleCurrency currency in currencies)
            {
                rows.Add(new SalePaymentRow
                {
                    CurrencyId = currency.Id,
                    CurrencyCode = currency.Code,
                    CurrencyName = currency.Name,
                    DecimalPlaces = currency.DecimalPlaces,
                    RateToBase = currency.RateToBase,
                    AmountPaid = 0m
                });
            }

            grid.CellValueChanged += (sender, args) => UpdatePaidTotal();
            grid.DataError += (sender, args) =>
            {
                if (args.Exception != null)
                {
                    args.ThrowException = false;
                    PurchasingUiErrors.ShowLogged(
                        this,
                        args.Exception,
                        "مبلغ الدفع غير صالح؛ راجع منازل العملة.");
                }
            };
            changeCurrencyComboBox.DataSource = new List<SaleCurrency>(currencies);
            changeCurrencyComboBox.SelectedItem = currencies.FirstOrDefault(currency => currency.IsBase);
            saveButton.Click += SaveButton_Click;
            UpdatePaidTotal();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            grid.EndEdit();
            if (Payments.Count == 0)
            {
                MessageBox.Show("أدخل مبلغًا مستلمًا واحدًا على الأقل.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdatePaidTotal()
        {
            try
            {
                grid.EndEdit();
                decimal amount = 0m;
                foreach (SalePaymentRow row in rows)
                {
                    decimal converted = checked(row.AmountPaid * row.RateToBase);
                    amount = checked(amount + SalesService.RoundToCurrency(converted, baseCurrency));
                }

                paidLabel.Text = "المجموع بالعملة الأساسية: " +
                                 amount.ToString(
                                     "N" + baseCurrency.DecimalPlaces,
                                     CultureInfo.CurrentCulture) +
                                 " " + baseCurrency.Code;
            }
            catch (OverflowException)
            {
                paidLabel.Text = "مجموع الدفع خارج النطاق";
            }
        }
    }
}
