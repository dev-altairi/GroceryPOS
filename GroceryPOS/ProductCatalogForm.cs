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

    public partial class ProductEditorForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly ProductRecord product;
        private bool canManage;

        public ProductEditorForm()
        {
            InitializeComponent();
        }

        internal ProductEditorForm(
            AuthenticatedUser authenticatedUser,
            IList<CatalogOption> categories,
            IList<CatalogOption> units,
            ProductRecord existingProduct)
            : this()
        {
            if (authenticatedUser == null)
            {
                throw new ArgumentNullException("authenticatedUser");
            }

            user = authenticatedUser;
            product = existingProduct;
            categoryComboBox.DataSource = new List<CatalogOption>(categories);
            unitComboBox.DataSource = new List<CatalogOption>(units);
            Text = existingProduct == null ? "إضافة منتج" : "تعديل منتج";

            if (product != null)
            {
                skuTextBox.Text = product.Sku;
                nameTextBox.Text = product.Name;
                SelectOption(categoryComboBox, product.CategoryId);
                SelectOption(unitComboBox, product.BaseUnitId);
                unitComboBox.Enabled = false;
                barcodeTextBox.Text = product.Barcode;
                retailPriceTextBox.Text = product.RetailPrice.ToString(CultureInfo.CurrentCulture);
                wholesalePriceTextBox.Text = product.WholesalePrice.HasValue
                    ? product.WholesalePrice.Value.ToString(CultureInfo.CurrentCulture)
                    : string.Empty;
                minimumStockTextBox.Text = product.MinStock.ToString(CultureInfo.CurrentCulture);
                tracksExpiryCheckBox.Checked = product.TracksExpiry;
                activeCheckBox.Checked = product.IsActive;
            }
            else
            {
                minimumStockTextBox.Text = "0";
                retailPriceTextBox.Text = "0";
            }

            canManage = user.HasPermission("Products.Manage");
            bool canEditPrices = user.HasPermission("Products.EditPrice");
            foreach (Control control in Controls)
            {
                if (control != saveButton && control != cancelButton)
                {
                    control.Enabled = canManage;
                }
            }
            retailPriceTextBox.Enabled = canManage && canEditPrices;
            wholesalePriceTextBox.Enabled = canManage && canEditPrices;

            if (!canManage)
            {
                Text = "عرض منتج";
            }

            saveButton.Text = canManage ? "حفظ" : "إغلاق";
            saveButton.DialogResult = canManage ? DialogResult.None : DialogResult.Cancel;
            cancelButton.Text = canManage ? "إلغاء" : "رجوع";
            saveButton.Click += SaveButton_Click;
            cancelButton.Click += (sender, args) => Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (canManage)
            {
                SaveProduct();
            }
            else
            {
                Close();
            }
        }

        private static void SelectOption(ComboBox comboBox, int id)
        {
            foreach (CatalogOption option in comboBox.Items)
            {
                if (option.Id == id)
                {
                    comboBox.SelectedItem = option;
                    return;
                }
            }
        }

        private void SaveProduct()
        {
            CatalogOption category = categoryComboBox.SelectedItem as CatalogOption;
            CatalogOption unit = unitComboBox.SelectedItem as CatalogOption;
            decimal retailPrice;
            decimal? wholesalePrice;
            decimal minimumStock;

            if (category == null || unit == null)
            {
                MessageBox.Show("اختر التصنيف والوحدة الأساسية.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryParseDecimal(retailPriceTextBox.Text, false, out retailPrice) ||
                !TryParseDecimal(wholesalePriceTextBox.Text, true, out wholesalePrice) ||
                !TryParseDecimal(minimumStockTextBox.Text, false, out minimumStock))
            {
                MessageBox.Show(
                    "أدخل أسعارًا وكميات رقمية صحيحة. اترك سعر الجملة فارغًا إذا لم يُحدد.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ProductsService.Save(user, new ProductDraft
                {
                    Id = product == null ? (int?)null : product.Id,
                    Sku = skuTextBox.Text,
                    Name = nameTextBox.Text,
                    CategoryId = category.Id,
                    BaseUnitId = unit.Id,
                    Barcode = barcodeTextBox.Text,
                    RetailPrice = retailPrice,
                    WholesalePrice = wholesalePrice,
                    MinStock = minimumStock,
                    TracksExpiry = tracksExpiryCheckBox.Checked,
                    IsActive = activeCheckBox.Checked
                });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                ShowSaveError(error);
            }
        }

        private static bool TryParseDecimal(string text, bool allowEmpty, out decimal? value)
        {
            value = null;
            if (allowEmpty && string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            decimal parsed;
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed) &&
                !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
            {
                return false;
            }

            value = parsed;
            return true;
        }

        private static bool TryParseDecimal(string text, bool allowEmpty, out decimal value)
        {
            value = 0;
            decimal? parsed;
            if (!TryParseDecimal(text, allowEmpty, out parsed) || !parsed.HasValue)
            {
                return false;
            }

            value = parsed.Value;
            return true;
        }

        private void ShowSaveError(SqlException error)
        {
            string message;
            if (error.Number == 2601 || error.Number == 2627)
            {
                message = "رمز SKU أو الباركود مستخدم بالفعل لمنتج آخر.";
            }
            else if (error.Number == 547)
            {
                message = "تعذر حفظ المنتج بسبب ارتباطه ببيانات أخرى.";
            }
            else
            {
                message = "تعذر حفظ المنتج في قاعدة البيانات.";
            }

            try
            {
                AppLogger.Write(error);
                message += Environment.NewLine + "حُفظت التفاصيل التقنية في سجل التطبيق.";
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
