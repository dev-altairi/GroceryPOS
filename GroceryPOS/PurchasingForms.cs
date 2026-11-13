using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class SupplierManagementForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly TextBox searchTextBox;
        private readonly DataGridView grid;
        private readonly Button addButton;
        private readonly Button editButton;
        private readonly Button toggleButton;
        private readonly Label status;
        private readonly Timer searchTimer;

        internal SupplierManagementForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "إدارة الموردين";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 520);
            MinimumSize = new Size(720, 400);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 58,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8)
            };
            addButton = CreateButton("إضافة مورد");
            addButton.Visible = user.HasPermission("Suppliers.Manage");
            addButton.Click += (sender, args) => OpenEditor(null);
            toolbar.Controls.Add(addButton);

            editButton = CreateButton("تعديل");
            editButton.Visible = user.HasPermission("Suppliers.Manage");
            editButton.Enabled = false;
            editButton.Click += (sender, args) =>
            {
                SupplierRecord selected = SelectedSupplier();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toolbar.Controls.Add(editButton);

            toggleButton = CreateButton("تعطيل");
            toggleButton.Visible = user.HasPermission("Suppliers.Manage");
            toggleButton.Enabled = false;
            toggleButton.Click += ToggleButton_Click;
            toolbar.Controls.Add(toggleButton);

            toolbar.Controls.Add(new Label
            {
                Text = "بحث:",
                AutoSize = true,
                Margin = new Padding(10, 10, 3, 0)
            });
            searchTextBox = new TextBox
            {
                Width = 220,
                MaxLength = 200,
                Margin = new Padding(4, 4, 4, 4)
            };
            toolbar.Controls.Add(searchTextBox);
            searchTimer = new Timer { Interval = 300 };
            searchTimer.Tick += (sender, args) =>
            {
                searchTimer.Stop();
                RefreshSuppliers();
            };
            searchTextBox.TextChanged += (sender, args) =>
            {
                searchTimer.Stop();
                searchTimer.Start();
            };

            var refreshButton = CreateButton("تحديث");
            refreshButton.Click += (sender, args) => RefreshSuppliers();
            toolbar.Controls.Add(refreshButton);
            var closeButton = CreateButton("إغلاق");
            closeButton.Click += (sender, args) => Close();
            toolbar.Controls.Add(closeButton);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id", Visible = false
            });
            AddColumn("Name", "اسم المورد", 140);
            AddColumn("Phone", "الهاتف", 85);
            AddColumn("Address", "العنوان", 120);
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive", HeaderText = "نشط", FillWeight = 55
            });
            grid.SelectionChanged += (sender, args) => UpdateSelection();
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0 && user.HasPermission("Suppliers.Manage"))
                {
                    SupplierRecord supplier = SelectedSupplier();
                    if (supplier != null)
                    {
                        OpenEditor(supplier);
                    }
                }
            };

            status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 10, 0)
            };
            Controls.Add(grid);
            Controls.Add(status);
            Controls.Add(toolbar);
            Shown += (sender, args) => RefreshSuppliers();
            FormClosed += (sender, args) => searchTimer.Dispose();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 105,
                Height = 32,
                Margin = new Padding(3)
            };
        }

        private void AddColumn(string property, string title, float weight)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            });
        }

        private SupplierRecord SelectedSupplier()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as SupplierRecord;
        }

        private void UpdateSelection()
        {
            SupplierRecord supplier = SelectedSupplier();
            bool canManage = user.HasPermission("Suppliers.Manage");
            editButton.Enabled = canManage && supplier != null;
            toggleButton.Enabled = canManage && supplier != null;
            toggleButton.Text = supplier != null && supplier.IsActive ? "تعطيل" : "تفعيل";
        }

        private void RefreshSuppliers()
        {
            try
            {
                List<SupplierRecord> suppliers = SuppliersService.Search(user, searchTextBox.Text);
                grid.DataSource = suppliers;
                status.Text = "عدد الموردين: " + suppliers.Count.ToString(CultureInfo.CurrentCulture);
                UpdateSelection();
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل الموردين من قاعدة البيانات.");
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

        private void OpenEditor(SupplierRecord supplier)
        {
            using (var editor = new SupplierEditorForm(supplier))
            {
                if (editor.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    SuppliersService.Save(user, new SupplierDraft
                    {
                        Id = supplier == null ? (int?)null : supplier.Id,
                        Name = editor.SupplierName,
                        Phone = editor.Phone,
                        Address = editor.Address,
                        Notes = editor.Notes
                    });
                    RefreshSuppliers();
                }
                catch (ArgumentException error)
                {
                    MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (InvalidOperationException error)
                {
                    MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (UnauthorizedAccessException error)
                {
                    MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (SqlException error)
                {
                    ShowError(error, "تعذر حفظ المورد.");
                }
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            SupplierRecord supplier = SelectedSupplier();
            if (supplier == null)
            {
                return;
            }

            bool activate = !supplier.IsActive;
            string prompt = activate
                ? "هل تريد تفعيل المورد " + supplier.Name + "؟"
                : "هل تريد تعطيل المورد " + supplier.Name + "؟";
            if (MessageBox.Show(prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SuppliersService.SetActive(user, supplier.Id, activate);
                RefreshSuppliers();
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تغيير حالة المورد.");
            }
        }

        private void ShowError(Exception error, string message)
        {
            PurchasingUiErrors.ShowLogged(this, error, message);
        }
    }

    internal sealed class SupplierEditorForm : Form
    {
        private readonly TextBox nameTextBox;
        private readonly TextBox phoneTextBox;
        private readonly TextBox addressTextBox;
        private readonly TextBox notesTextBox;

        internal string SupplierName { get { return nameTextBox.Text; } }
        internal string Phone { get { return phoneTextBox.Text; } }
        internal string Address { get { return addressTextBox.Text; } }
        internal string Notes { get { return notesTextBox.Text; } }

        internal SupplierEditorForm(SupplierRecord current)
        {
            Text = current == null ? "إضافة مورد" : "تعديل مورد";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(500, 360);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            nameTextBox = AddField("اسم المورد", 25, 200);
            phoneTextBox = AddField("الهاتف", 75, 30);
            addressTextBox = AddField("العنوان", 125, 300);
            notesTextBox = AddField("ملاحظات", 175, 500);
            notesTextBox.Multiline = true;
            notesTextBox.Height = 75;
            if (current != null)
            {
                nameTextBox.Text = current.Name;
                phoneTextBox.Text = current.Phone;
                addressTextBox.Text = current.Address;
                notesTextBox.Text = current.Notes;
            }

            var save = new Button
            {
                Text = "حفظ",
                Location = new Point(285, 290),
                Width = 90,
                Height = 34
            };
            save.Click += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("اسم المورد مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nameTextBox.Focus();
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(175, 290),
                Width = 90,
                Height = 34,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private TextBox AddField(string label, int top, int maxLength)
        {
            Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(385, top + 4)
            });
            var textBox = new TextBox
            {
                Location = new Point(35, top),
                Width = 325,
                MaxLength = maxLength
            };
            Controls.Add(textBox);
            return textBox;
        }
    }

    internal sealed class PurchasesForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly DataGridView grid;
        private readonly Label status;

        internal PurchasesForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "فواتير المشتريات";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1000, 560);
            MinimumSize = new Size(800, 430);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 55,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8)
            };
            var addButton = CreateButton("استلام فاتورة");
            addButton.Click += AddInvoiceButton_Click;
            toolbar.Controls.Add(addButton);
            var refreshButton = CreateButton("تحديث");
            refreshButton.Click += (sender, args) => RefreshInvoices();
            toolbar.Controls.Add(refreshButton);
            var closeButton = CreateButton("إغلاق");
            closeButton.Click += (sender, args) => Close();
            toolbar.Controls.Add(closeButton);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id", Visible = false
            });
            AddColumn("InvoiceNo", "رقم الفاتورة", 80);
            AddColumn("SupplierInvoiceNo", "رقم فاتورة المورد", 90);
            AddColumn("SupplierName", "المورد", 120);
            AddColumn("InvoiceDate", "التاريخ", 75, "d");
            AddColumn("CurrencyCode", "العملة", 45);
            AddColumn("Total", "الإجمالي", 75, "N4");
            AddColumn("TotalBase", "الإجمالي بالعملة الأساسية", 95, "N4");
            AddColumn("LineCount", "الأصناف", 50);
            AddColumn("Status", "الحالة", 55);
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    PurchaseInvoiceRecord invoice = SelectedInvoice();
                    if (invoice != null)
                    {
                        ShowInvoiceLines(invoice);
                    }
                }
            };

            status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 10, 0)
            };
            Controls.Add(grid);
            Controls.Add(status);
            Controls.Add(toolbar);
            Shown += (sender, args) => RefreshInvoices();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 125,
                Height = 32,
                Margin = new Padding(4)
            };
        }

        private void AddColumn(string property, string title, float weight, string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
        }

        private PurchaseInvoiceRecord SelectedInvoice()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as PurchaseInvoiceRecord;
        }

        private void RefreshInvoices()
        {
            try
            {
                List<PurchaseInvoiceRecord> invoices = PurchasesService.LoadInvoices(user);
                grid.DataSource = invoices;
                status.Text = "عدد الفواتير: " + invoices.Count.ToString(CultureInfo.CurrentCulture);
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل فواتير المشتريات.");
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

        private void AddInvoiceButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<SupplierOption> suppliers = PurchasesService.LoadActiveSuppliers(user);
                List<CurrencyOption> currencies = PurchasesService.LoadActiveCurrencies(user);
                List<ProductUnitOption> productUnits = PurchasesService.LoadPurchaseUnits(user);
                using (var editor = new PurchaseInvoiceEditorForm(user, suppliers, currencies, productUnits))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        MessageBox.Show(
                            "تم استلام الفاتورة " + editor.InvoiceNumber +
                            " وتحديث المخزون ومتوسط التكلفة ورصيد المورد.",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        RefreshInvoices();
                    }
                }
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل بيانات فاتورة الشراء.");
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

        private void ShowInvoiceLines(PurchaseInvoiceRecord invoice)
        {
            try
            {
                List<PurchaseInvoiceLineRecord> lines =
                    PurchasesService.LoadInvoiceLines(user, invoice.Id);
                using (var details = new PurchaseInvoiceDetailsForm(invoice, lines))
                {
                    details.ShowDialog(this);
                }
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل تفاصيل فاتورة الشراء.");
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
    }

    internal sealed class PurchaseInvoiceEditorForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly ComboBox supplierComboBox;
        private readonly ComboBox currencyComboBox;
        private readonly DateTimePicker invoiceDatePicker;
        private readonly TextBox supplierInvoiceNoTextBox;
        private readonly TextBox notesTextBox;
        private readonly DataGridView linesGrid;
        private readonly BindingList<PurchaseInvoiceLineDraft> lines =
            new BindingList<PurchaseInvoiceLineDraft>();
        private readonly List<ProductUnitOption> productUnits;

        internal string InvoiceNumber { get; private set; }

        internal PurchaseInvoiceEditorForm(
            AuthenticatedUser authenticatedUser,
            IList<SupplierOption> suppliers,
            IList<CurrencyOption> currencies,
            IList<ProductUnitOption> availableProductUnits)
        {
            user = authenticatedUser;
            productUnits = new List<ProductUnitOption>
            {
                new ProductUnitOption(0, 0, "— اختر منتجًا —", string.Empty, 1m, false)
            };
            productUnits.AddRange(availableProductUnits);

            Text = "استلام فاتورة شراء";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1040, 680);
            MinimumSize = new Size(1060, 750);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            supplierComboBox = AddCombo("المورد", 20, new List<SupplierOption>(suppliers));
            currencyComboBox = AddCombo("العملة", 75, new List<CurrencyOption>(currencies));
            CurrencyOption baseCurrency = currencies.FirstOrDefault(currency => currency.IsBase);
            currencyComboBox.SelectedItem = baseCurrency;

            Controls.Add(new Label
            {
                Text = "تاريخ الفاتورة",
                AutoSize = true,
                Location = new Point(890, 130)
            });
            invoiceDatePicker = new DateTimePicker
            {
                Location = new Point(650, 125),
                Width = 220,
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today,
                Value = DateTime.Today
            };
            Controls.Add(invoiceDatePicker);

            Controls.Add(new Label
            {
                Text = "رقم فاتورة المورد",
                AutoSize = true,
                Location = new Point(570, 130)
            });
            supplierInvoiceNoTextBox = new TextBox
            {
                Location = new Point(350, 125),
                Width = 195,
                MaxLength = 50
            };
            Controls.Add(supplierInvoiceNoTextBox);
            Controls.Add(new Label
            {
                Text = "ملاحظات",
                AutoSize = true,
                Location = new Point(280, 130)
            });
            notesTextBox = new TextBox
            {
                Location = new Point(35, 125),
                Width = 225,
                MaxLength = 500
            };
            Controls.Add(notesTextBox);

            linesGrid = new DataGridView
            {
                Location = new Point(25, 175),
                Size = new Size(990, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White,
                DataSource = lines
            };
            linesGrid.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = "ProductUnitId",
                HeaderText = "المنتج والوحدة",
                FillWeight = 185,
                DataSource = productUnits,
                DisplayMember = "DisplayName",
                ValueMember = "Id",
                FlatStyle = FlatStyle.Flat
            });
            AddGridColumn("Quantity", "الكمية", 65, "N3");
            AddGridColumn("UnitPrice", "سعر الوحدة", 75, "N4");
            AddGridColumn("BatchNo", "رقم الدفعة", 80);
            AddGridColumn("ExpiryDate", "الصلاحية (YYYY-MM-DD)", 90, "yyyy-MM-dd");
            linesGrid.DataError += LinesGrid_DataError;
            Controls.Add(linesGrid);

            Controls.Add(new Label
            {
                Text = "أدخل سعر الوحدة بعملة الفاتورة. الصلاحية إلزامية للمنتجات التي تتطلب التتبع.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(470, 610)
            });
            var save = new Button
            {
                Text = "استلام وحفظ",
                Location = new Point(270, 635),
                Width = 130,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            save.Click += SaveButton_Click;
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(160, 635),
                Width = 90,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private ComboBox AddCombo<T>(string label, int top, IList<T> choices)
        {
            Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(895, top + 4)
            });
            var combo = new ComboBox
            {
                Location = new Point(610, top),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = choices
            };
            if (choices.Count > 0)
            {
                combo.SelectedIndex = 0;
            }

            Controls.Add(combo);
            return combo;
        }

        private void AddGridColumn(string property, string title, float weight, string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            linesGrid.Columns.Add(column);
        }

        private void LinesGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.Exception == null)
            {
                return;
            }

            e.ThrowException = false;
            PurchasingUiErrors.ShowLogged(
                this,
                e.Exception,
                "القيمة المدخلة غير صالحة. راجع المنتج والكمية والسعر وتاريخ الصلاحية.");
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                linesGrid.EndEdit();
                CurrencyManager manager = BindingContext[lines] as CurrencyManager;
                if (manager != null)
                {
                    manager.EndCurrentEdit();
                }

                SupplierOption supplier = supplierComboBox.SelectedItem as SupplierOption;
                CurrencyOption currency = currencyComboBox.SelectedItem as CurrencyOption;
                if (supplier == null || currency == null)
                {
                    throw new ArgumentException("اختر المورد وعملة الفاتورة.");
                }

                List<PurchaseInvoiceLineDraft> enteredLines = new List<PurchaseInvoiceLineDraft>();
                foreach (PurchaseInvoiceLineDraft line in lines)
                {
                    bool untouched = line.ProductUnitId == 0 &&
                                     line.Quantity == 0 &&
                                     line.UnitPrice == 0 &&
                                     string.IsNullOrWhiteSpace(line.BatchNo) &&
                                     !line.ExpiryDate.HasValue;
                    if (untouched)
                    {
                        continue;
                    }

                    enteredLines.Add(line);
                }

                InvoiceNumber = PurchasesService.PostInvoice(user, new PurchaseInvoiceDraft
                {
                    SupplierId = supplier.Id,
                    CurrencyId = currency.Id,
                    InvoiceDate = invoiceDatePicker.Value.Date,
                    SupplierInvoiceNo = supplierInvoiceNoTextBox.Text,
                    Notes = notesTextBox.Text,
                    Lines = enteredLines
                });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر حفظ فاتورة الشراء.");
            }
        }
    }

    internal sealed class PurchaseInvoiceDetailsForm : Form
    {
        internal PurchaseInvoiceDetailsForm(
            PurchaseInvoiceRecord invoice,
            IList<PurchaseInvoiceLineRecord> lines)
        {
            Text = "تفاصيل فاتورة " + invoice.InvoiceNo;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(880, 480);
            MinimumSize = new Size(720, 500);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = string.Format(
                    CultureInfo.CurrentCulture,
                    "المورد: {0}    رقم المورد: {1}    التاريخ: {2:d}    الإجمالي: {3:N4} {4}",
                    invoice.SupplierName,
                    string.IsNullOrEmpty(invoice.SupplierInvoiceNo) ? "—" : invoice.SupplierInvoiceNo,
                    invoice.InvoiceDate,
                    invoice.Total,
                    invoice.CurrencyCode),
                AutoSize = true,
                Location = new Point(25, 20)
            });
            var grid = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(840, 360),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White,
                DataSource = new List<PurchaseInvoiceLineRecord>(lines)
            };
            AddColumn(grid, "ProductName", "المنتج", 135);
            AddColumn(grid, "UnitName", "الوحدة", 65);
            AddColumn(grid, "Quantity", "الكمية", 65, "N3");
            AddColumn(grid, "QtyBase", "الكمية الأساسية", 75, "N3");
            AddColumn(grid, "UnitPrice", "سعر الوحدة", 75, "N4");
            AddColumn(grid, "LineTotal", "الإجمالي", 75, "N4");
            AddColumn(grid, "BatchNo", "الدفعة", 70);
            AddColumn(grid, "ExpiryDate", "الصلاحية", 75, "d");
            Controls.Add(grid);
            var close = new Button
            {
                Text = "إغلاق",
                Location = new Point(390, 430),
                Width = 95,
                Height = 32,
                Anchor = AnchorStyles.Bottom
            };
            close.Click += (sender, args) => Close();
            Controls.Add(close);
        }

        private static void AddColumn(
            DataGridView grid,
            string property,
            string title,
            float weight,
            string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
        }
    }

    internal static class PurchasingUiErrors
    {
        internal static void ShowLogged(IWin32Window owner, Exception error, string message)
        {
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

            MessageBox.Show(owner, message, "GroceryPOS", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
