using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SalesForm : Form
    {
        public SalesForm()
        {
            InitializeComponent();
            resultsGrid.DataSource = new System.Collections.Generic.List<SaleUnitOption>();
            cartGrid.DataSource = cart;
        }
    }
}
