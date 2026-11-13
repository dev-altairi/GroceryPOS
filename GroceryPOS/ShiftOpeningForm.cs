using System.ComponentModel;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ShiftOpeningForm : Form
    {
        public ShiftOpeningForm()
        {
            InitializeComponent();
            rows = new BindingList<ShiftOpeningRow>();
            grid.DataSource = rows;
        }
    }
}
