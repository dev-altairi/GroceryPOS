using System.Windows.Forms;
using System.Collections.Generic;

namespace GroceryPOS
{
    public partial class ProductEditorForm : Form
    {
        public ProductEditorForm()
        {
            InitializeComponent();
        }

        internal ProductEditorForm(
            AuthenticatedUser user,
            IList<CatalogOption> categories,
            IList<CatalogOption> units,
            ProductRecord product)
            : this()
        {
        }
    }
}
