using System.ComponentModel;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SalePaymentForm : Form
    {
        public SalePaymentForm()
        {
            InitializeComponent();
            rows = new BindingList<SalePaymentRow>();
            grid.DataSource = rows;
        }
    }
}
