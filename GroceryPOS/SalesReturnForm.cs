using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SalesReturnForm : Form
    {
        public SalesReturnForm()
        {
            InitializeComponent();
            linesGrid.DataSource = lines;
        }
    }
}
