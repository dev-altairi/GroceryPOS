using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ProductCatalogForm : Form
    {
        private readonly AuthenticatedUser user;
        private Timer searchTimer;

        public ProductCatalogForm()
        {
            InitializeComponent();
        }

        internal ProductCatalogForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            if (authenticatedUser == null)
            {
                throw new ArgumentNullException("authenticatedUser");
            }

            user = authenticatedUser;
            searchTimer = new Timer { Interval = 250 };
            searchTimer.Tick += (sender, args) =>
            {
                searchTimer.Stop();
                RefreshProducts();
            };
            searchTextBox.TextChanged += (sender, args) =>
            {
                searchTimer.Stop();
                searchTimer.Start();
            };
            refreshButton.Click += (sender, args) => RefreshProducts();
            addButton.Click += AddButton_Click;
            editButton.Click += EditButton_Click;
            unitsButton.Click += UnitsButton_Click;
            categoriesButton.Click += CategoriesButton_Click;
            closeButton.Click += (sender, args) => Close();
            productsGrid.SelectionChanged += (sender, args) =>
            {
                editButton.Enabled = user.HasPermission("Products.Manage") && SelectedProduct() != null;
                unitsButton.Enabled = user.HasPermission("Products.Manage") && SelectedProduct() != null;
            };
            productsGrid.CellDoubleClick += (sender, args) =>
            {
                if (user.HasPermission("Products.Manage") && args.RowIndex >= 0)
                {
                    EditSelectedProduct();
                }
            };

            addButton.Visible = user.HasPermission("Products.Manage");
            editButton.Visible = user.HasPermission("Products.Manage");
            unitsButton.Visible = user.HasPermission("Products.Manage");
            categoriesButton.Visible = user.HasPermission("Categories.Manage");
            Shown += (sender, args) => RefreshProducts();
            FormClosed += (sender, args) => searchTimer.Dispose();
        }

        private void RefreshProducts()
        {
            try
            {
                List<ProductRecord> products = ProductsService.Search(user, searchTextBox.Text);
                productsGrid.DataSource = products;
                statusLabel.Text = "عدد المنتجات: " + products.Count.ToString(CultureInfo.CurrentCulture);
                editButton.Enabled = user.HasPermission("Products.Manage") && SelectedProduct() != null;
                unitsButton.Enabled = user.HasPermission("Products.Manage") && SelectedProduct() != null;
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل المنتجات من قاعدة البيانات.");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            OpenEditor(null);
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            EditSelectedProduct();
        }

        private void UnitsButton_Click(object sender, EventArgs e)
        {
            ProductRecord product = SelectedProduct();
            if (product == null)
            {
                return;
            }

            try
            {
                List<ProductUnitRecord> rows = ProductUnitsService.Load(user, product.Id);
                List<CatalogOption> units = ProductUnitsService.LoadChoices(product.Id);
                using (var form = new ProductUnitsForm(user, product, rows, units))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        RefreshProducts();
                    }
                }
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل وحدات المنتج.");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CategoriesButton_Click(object sender, EventArgs e)
        {
            using (var form = new CategoryManagementForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void EditSelectedProduct()
        {
            ProductRecord product = SelectedProduct();
            if (product != null)
            {
                OpenEditor(product);
            }
        }

        private ProductRecord SelectedProduct()
        {
            return productsGrid.CurrentRow == null
                ? null
                : productsGrid.CurrentRow.DataBoundItem as ProductRecord;
        }

        private void OpenEditor(ProductRecord product)
        {
            try
            {
                List<CatalogOption> categories = ProductsService.LoadCategories(
                    product == null ? (int?)null : product.CategoryId);
                List<CatalogOption> units = ProductsService.LoadUnits(
                    product == null ? (int?)null : product.BaseUnitId);
                using (var editor = new ProductEditorForm(user, categories, units, product))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        RefreshProducts();
                    }
                }
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل بيانات التصنيفات والوحدات.");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowError(Exception error, string message)
        {
            try
            {
                AppLogger.Write(error);
                message += Environment.NewLine + "حُفظت التفاصيل في سجل التطبيق.";
            }
            catch (IOException)
            {
                message += Environment.NewLine + "تعذر حفظ سجل الخطأ.";
            }
            catch (UnauthorizedAccessException)
            {
                message += Environment.NewLine + "تعذر حفظ سجل الخطأ بسبب الأذونات.";
            }

            MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
