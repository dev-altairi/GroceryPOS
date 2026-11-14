using System.Windows.Forms;

using System.ComponentModel;

namespace GroceryPOS
{
    public partial class SalesForm : Form
    {
        public SalesForm()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            resultsGrid.DataSource = new System.Collections.Generic.List<SaleUnitOption>();
            cartGrid.DataSource = cart;
        }
    }
}
