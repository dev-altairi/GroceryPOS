using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class ProductCatalogForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly TextBox searchTextBox;
        private readonly DataGridView productsGrid;
        private readonly Button addButton;
        private readonly Button editButton;
        private readonly Button unitsButton;
        private readonly Button categoriesButton;
        private readonly Label statusLabel;
        private readonly Timer searchTimer;

        internal ProductCatalogForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "إدارة المنتجات - GroceryPOS";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1120, 600);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(12)
            };
            toolbar.Controls.Add(new Label
            {
                Text = "بحث بالاسم أو الرمز أو الباركود:",
                AutoSize = true,
                Location = new Point(1000, 19)
            });
            searchTextBox = new TextBox
            {
                Location = new Point(690, 14),
                Width = 300,
                MaxLength = 200
            };
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
            toolbar.Controls.Add(searchTextBox);

            var refreshButton = new Button
            {
                Text = "تحديث",
                Location = new Point(600, 12),
                Width = 75,
                Height = 32
            };
            refreshButton.Click += (sender, args) => RefreshProducts();
            toolbar.Controls.Add(refreshButton);

            addButton = new Button
            {
                Text = "إضافة منتج",
                Location = new Point(485, 12),
                Width = 100,
                Height = 32,
                Visible = user.HasPermission("Products.Manage")
            };
            addButton.Click += AddButton_Click;
            toolbar.Controls.Add(addButton);

            editButton = new Button
            {
                Text = "تعديل المحدد",
                Location = new Point(370, 12),
                Width = 105,
                Height = 32,
                Visible = user.HasPermission("Products.Manage"),
                Enabled = false
            };
            editButton.Click += EditButton_Click;
            toolbar.Controls.Add(editButton);

            unitsButton = new Button
            {
                Text = "وحدات وباركود",
                Location = new Point(245, 12),
                Width = 115,
                Height = 32,
                Visible = user.HasPermission("Products.Manage"),
                Enabled = false
            };
            unitsButton.Click += UnitsButton_Click;
            toolbar.Controls.Add(unitsButton);

            categoriesButton = new Button
            {
                Text = "التصنيفات",
                Location = new Point(145, 12),
                Width = 90,
                Height = 32,
                Visible = user.HasPermission("Categories.Manage")
            };
            categoriesButton.Click += CategoriesButton_Click;
            toolbar.Controls.Add(categoriesButton);

            var closeButton = new Button
            {
                Text = "إغلاق",
                Location = new Point(35, 12),
                Width = 100,
                Height = 32,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            closeButton.Click += (sender, args) => Close();
            toolbar.Controls.Add(closeButton);

            productsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RightToLeft = RightToLeft.Yes,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            productsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id", DataPropertyName = "Id", Visible = false
            });
            AddColumn("Sku", "SKU", 75);
            AddColumn("Name", "اسم المنتج", 170);
            AddColumn("CategoryName", "التصنيف", 100);
            AddColumn("BaseUnitName", "الوحدة", 65);
            AddColumn("Barcode", "الباركود", 110);
            AddColumn("RetailPrice", "سعر البيع", 75, "N2");
            AddColumn("WholesalePrice", "سعر الجملة", 75, "N2");
            AddColumn("StockQuantity", "المخزون", 70, "N3");
            AddColumn("MinStock", "الحد الأدنى", 70, "N3");
            AddColumn("TracksExpiry", "يتطلب صلاحية", 65, null, true);
            AddColumn("IsActive", "نشط", 55, null, true);
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

            statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(12, 0, 12, 0)
            };

            Controls.Add(productsGrid);
            Controls.Add(statusLabel);
            Controls.Add(toolbar);
            Shown += (sender, args) => RefreshProducts();
            FormClosed += (sender, args) => searchTimer.Dispose();
        }

        private void AddColumn(
            string property,
            string title,
            float fillWeight,
            string format = null,
            bool centered = false)
        {
            var column = new DataGridViewTextBoxColumn
            {
                Name = property,
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = fillWeight
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            if (centered)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            productsGrid.Columns.Add(column);
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

    internal sealed class ProductEditorForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly ProductRecord product;
        private readonly TextBox skuTextBox;
        private readonly TextBox nameTextBox;
        private readonly ComboBox categoryComboBox;
        private readonly ComboBox unitComboBox;
        private readonly TextBox barcodeTextBox;
        private readonly TextBox retailPriceTextBox;
        private readonly TextBox wholesalePriceTextBox;
        private readonly TextBox minimumStockTextBox;
        private readonly CheckBox tracksExpiryCheckBox;
        private readonly CheckBox activeCheckBox;

        internal ProductEditorForm(
            AuthenticatedUser authenticatedUser,
            IList<CatalogOption> categories,
            IList<CatalogOption> units,
            ProductRecord existingProduct)
        {
            user = authenticatedUser;
            product = existingProduct;
            Text = existingProduct == null ? "إضافة منتج" : "تعديل منتج";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(570, 525);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            skuTextBox = AddTextField("رمز SKU (اختياري)", 25, 50);
            nameTextBox = AddTextField("اسم المنتج", 75, 200);
            categoryComboBox = AddComboField("التصنيف", 125, categories);
            unitComboBox = AddComboField("الوحدة الأساسية", 175, units);
            barcodeTextBox = AddTextField("الباركود (اختياري)", 225, 50);
            retailPriceTextBox = AddTextField("سعر البيع بالعملة الأساسية", 275, 40);
            wholesalePriceTextBox = AddTextField("سعر الجملة بالعملة الأساسية", 325, 40);
            minimumStockTextBox = AddTextField("الحد الأدنى للمخزون", 375, 40);
            tracksExpiryCheckBox = new CheckBox
            {
                Text = "المنتج يتطلب تتبع تاريخ الصلاحية",
                AutoSize = true,
                Location = new Point(250, 425)
            };
            activeCheckBox = new CheckBox
            {
                Text = "المنتج نشط",
                AutoSize = true,
                Location = new Point(110, 425),
                Checked = true
            };
            Controls.Add(tracksExpiryCheckBox);
            Controls.Add(activeCheckBox);

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

            bool canManage = user.HasPermission("Products.Manage");
            bool canEditPrices = user.HasPermission("Products.EditPrice");
            foreach (Control control in Controls)
            {
                control.Enabled = canManage;
            }
            retailPriceTextBox.Enabled = canManage && canEditPrices;
            wholesalePriceTextBox.Enabled = canManage && canEditPrices;

            if (!canManage)
            {
                Text = "عرض منتج";
            }

            var saveButton = new Button
            {
                Text = canManage ? "حفظ" : "إغلاق",
                Location = new Point(330, 470),
                Width = 105,
                Height = 36,
                DialogResult = canManage ? DialogResult.None : DialogResult.Cancel
            };
            saveButton.Click += (sender, args) =>
            {
                if (canManage)
                {
                    SaveProduct();
                }
                else
                {
                    Close();
                }
            };
            Controls.Add(saveButton);
            var cancelButton = new Button
            {
                Text = canManage ? "إلغاء" : "رجوع",
                Location = new Point(205, 470),
                Width = 105,
                Height = 36,
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Click += (sender, args) => Close();
            Controls.Add(cancelButton);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private TextBox AddTextField(string label, int top, int maxLength)
        {
            Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(405, top + 5)
            });
            var textBox = new TextBox
            {
                Location = new Point(75, top),
                Width = 310,
                MaxLength = maxLength
            };
            Controls.Add(textBox);
            return textBox;
        }

        private ComboBox AddComboField(string label, int top, IList<CatalogOption> options)
        {
            Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(405, top + 5)
            });
            var comboBox = new ComboBox
            {
                Location = new Point(75, top),
                Width = 310,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = new List<CatalogOption>(options),
                DisplayMember = "Name",
                ValueMember = "Id"
            };
            Controls.Add(comboBox);
            return comboBox;
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
