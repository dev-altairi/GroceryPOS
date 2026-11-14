using System;
using System.ComponentModel;
using System.Linq;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GroceryPOS
{
    public partial class PurchaseInvoiceEditorForm : Form
    {
        private AuthenticatedUser user;
        private readonly BindingList<PurchaseInvoiceLineDraft> lines =
            new BindingList<PurchaseInvoiceLineDraft>();
        private List<ProductUnitOption> productUnits;

        public PurchaseInvoiceEditorForm()
        {
            InitializeComponent();
            productUnits = new List<ProductUnitOption>();
        }

        internal string InvoiceNumber { get; private set; }

        internal PurchaseInvoiceEditorForm(
            AuthenticatedUser authenticatedUser,
            IList<SupplierOption> suppliers,
            IList<CurrencyOption> currencies,
            IList<ProductUnitOption> availableProductUnits)
            : this()
        {
            user = authenticatedUser;
            productUnits.Add(new ProductUnitOption(
                0, 0, "— اختر منتجًا —", string.Empty, 1m, false));
            productUnits.AddRange(availableProductUnits);
            productUnitColumn.DataSource = productUnits;
            linesGrid.DataSource = lines;

            supplierComboBox.DataSource = new List<SupplierOption>(suppliers);
            if (suppliers.Count > 0)
            {
                supplierComboBox.SelectedIndex = 0;
            }

            currencyComboBox.DataSource = new List<CurrencyOption>(currencies);
            if (currencies.Count > 0)
            {
                currencyComboBox.SelectedIndex = 0;
            }

            CurrencyOption baseCurrency = currencies.FirstOrDefault(currency => currency.IsBase);
            currencyComboBox.SelectedItem = baseCurrency;
            linesGrid.DataError += LinesGrid_DataError;
            saveButton.Click += SaveButton_Click;
        }

        private void LinesGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception == null)
            {
                return;
            }

            e.ThrowException = false;
            PurchasingUiErrors.ShowLogged(
                this,
                e.Exception,
                "القيمة المدخلة غير صالحة. راجع المنتج والكمية والسعر وتاريخ الصلاحية.");
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                linesGrid.EndEdit();
                CurrencyManager manager = BindingContext[lines] as CurrencyManager;
                if (manager != null)
                {
                    manager.EndCurrentEdit();
                }

                SupplierOption supplier = supplierComboBox.SelectedItem as SupplierOption;
                CurrencyOption currency = currencyComboBox.SelectedItem as CurrencyOption;
                if (supplier == null || currency == null)
                {
                    throw new ArgumentException("اختر المورد وعملة الفاتورة.");
                }

                List<PurchaseInvoiceLineDraft> enteredLines = new List<PurchaseInvoiceLineDraft>();
                foreach (PurchaseInvoiceLineDraft line in lines)
                {
                    bool untouched = line.ProductUnitId == 0 &&
                                     line.Quantity == 0 &&
                                     line.UnitPrice == 0 &&
                                     string.IsNullOrWhiteSpace(line.BatchNo) &&
                                     !line.ExpiryDate.HasValue;
                    if (untouched)
                    {
                        continue;
                    }

                    enteredLines.Add(line);
                }

                InvoiceNumber = PurchasesService.PostInvoice(user, new PurchaseInvoiceDraft
                {
                    SupplierId = supplier.Id,
                    CurrencyId = currency.Id,
                    InvoiceDate = invoiceDatePicker.Value.Date,
                    SupplierInvoiceNo = supplierInvoiceNoTextBox.Text,
                    Notes = notesTextBox.Text,
                    Lines = enteredLines
                });
                DialogResult = DialogResult.OK;
                Close();
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
                PurchasingUiErrors.ShowLogged(this, error, "تعذر حفظ فاتورة الشراء.");
            }
        }
    }
}
