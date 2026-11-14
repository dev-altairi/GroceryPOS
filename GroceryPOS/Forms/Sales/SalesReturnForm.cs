using System.ComponentModel;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SalesReturnForm : Form
    {
        public SalesReturnForm()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            linesGrid.DataSource = lines;
        }
    }
}
