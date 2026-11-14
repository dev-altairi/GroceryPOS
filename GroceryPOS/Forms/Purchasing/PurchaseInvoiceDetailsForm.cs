using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class PurchaseInvoiceDetailsForm : Form
    {
        public PurchaseInvoiceDetailsForm()
        {
            InitializeComponent();
        }

        internal PurchaseInvoiceDetailsForm(
            PurchaseInvoiceRecord invoice,
            IList<PurchaseInvoiceLineRecord> lines)
            : this()
        {
            Text = "تفاصيل فاتورة " + invoice.InvoiceNo;
            headerLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "المورد: {0}    رقم المورد: {1}    التاريخ: {2:d}    الإجمالي: {3:N4} {4}",
                invoice.SupplierName,
                string.IsNullOrEmpty(invoice.SupplierInvoiceNo) ? "—" : invoice.SupplierInvoiceNo,
                invoice.InvoiceDate,
                invoice.Total,
                invoice.CurrencyCode);
            linesGrid.DataSource = new List<PurchaseInvoiceLineRecord>(lines);
            closeButton.Click += (sender, args) => Close();
        }
    }
}
