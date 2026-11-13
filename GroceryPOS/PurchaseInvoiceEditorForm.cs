using System.Windows.Forms;
using System.Collections.Generic;

namespace GroceryPOS
{
    public partial class PurchaseInvoiceEditorForm : Form
    {
        public PurchaseInvoiceEditorForm()
        {
            InitializeComponent();
            productUnits = new List<ProductUnitOption>();
        }
    }
}
