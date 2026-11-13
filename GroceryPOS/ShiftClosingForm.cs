using System.ComponentModel;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ShiftClosingForm : Form
    {
        public ShiftClosingForm()
        {
            InitializeComponent();
            rows = new BindingList<ShiftClosingRow>();
            grid.DataSource = rows;
        }
    }
}
