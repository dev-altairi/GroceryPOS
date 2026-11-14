using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class PurchasesForm : Form
    {
        private AuthenticatedUser user;

        public PurchasesForm()
        {
            InitializeComponent();
        }

        internal PurchasesForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Click += AddInvoiceButton_Click;
            refreshButton.Click += (sender, args) => RefreshInvoices();
            closeButton.Click += (sender, args) => Close();
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    PurchaseInvoiceRecord invoice = SelectedInvoice();
                    if (invoice != null)
                    {
                        ShowInvoiceLines(invoice);
                    }
                }
            };
            Shown += (sender, args) => RefreshInvoices();
        }

        private PurchaseInvoiceRecord SelectedInvoice()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as PurchaseInvoiceRecord;
        }

        private void RefreshInvoices()
        {
            try
            {
                List<PurchaseInvoiceRecord> invoices = PurchasesService.LoadInvoices(user);
                grid.DataSource = invoices;
                status.Text = "عدد الفواتير: " + invoices.Count.ToString(CultureInfo.CurrentCulture);
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل فواتير المشتريات.");
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

        private void AddInvoiceButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<SupplierOption> suppliers = PurchasesService.LoadActiveSuppliers(user);
                List<CurrencyOption> currencies = PurchasesService.LoadActiveCurrencies(user);
                List<ProductUnitOption> productUnits = PurchasesService.LoadPurchaseUnits(user);
                using (var editor = new PurchaseInvoiceEditorForm(user, suppliers, currencies, productUnits))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        MessageBox.Show(
                            "تم استلام الفاتورة " + editor.InvoiceNumber +
                            " وتحديث المخزون ومتوسط التكلفة ورصيد المورد.",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        RefreshInvoices();
                    }
                }
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل بيانات فاتورة الشراء.");
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

        private void ShowInvoiceLines(PurchaseInvoiceRecord invoice)
        {
            try
            {
                List<PurchaseInvoiceLineRecord> lines =
                    PurchasesService.LoadInvoiceLines(user, invoice.Id);
                using (var details = new PurchaseInvoiceDetailsForm(invoice, lines))
                {
                    details.ShowDialog(this);
                }
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل تفاصيل فاتورة الشراء.");
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
    }
}
