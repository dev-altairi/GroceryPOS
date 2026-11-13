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
    public partial class SupplierManagementForm : Form
    {
        private AuthenticatedUser user;
        private Timer searchTimer;

        public SupplierManagementForm()
        {
            InitializeComponent();
        }

        internal SupplierManagementForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Visible = user.HasPermission("Suppliers.Manage");
            addButton.Click += (sender, args) => OpenEditor(null);
            editButton.Visible = user.HasPermission("Suppliers.Manage");
            editButton.Click += (sender, args) =>
            {
                SupplierRecord selected = SelectedSupplier();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toggleButton.Visible = user.HasPermission("Suppliers.Manage");
            toggleButton.Click += ToggleButton_Click;
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
            refreshButton.Click += (sender, args) => RefreshSuppliers();
            closeButton.Click += (sender, args) => Close();
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
            Shown += (sender, args) => RefreshSuppliers();
            FormClosed += (sender, args) => searchTimer.Dispose();
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

    public partial class SupplierEditorForm : Form
    {
        internal string SupplierName { get { return nameTextBox.Text; } }
        internal string Phone { get { return phoneTextBox.Text; } }
        internal string Address { get { return addressTextBox.Text; } }
        internal string Notes { get { return notesTextBox.Text; } }

        public SupplierEditorForm()
        {
            InitializeComponent();
        }

        internal SupplierEditorForm(SupplierRecord current)
            : this()
        {
            Text = current == null ? "إضافة مورد" : "تعديل مورد";
            if (current != null)
            {
                nameTextBox.Text = current.Name;
                phoneTextBox.Text = current.Phone;
                addressTextBox.Text = current.Address;
                notesTextBox.Text = current.Notes;
            }

            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("اسم المورد مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    public partial class PurchasesForm : Form
    {
        private AuthenticatedUser user;

        public PurchasesForm()
        {
            InitializeComponent();
        }

        internal PurchasesForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Click += AddInvoiceButton_Click;
            refreshButton.Click += (sender, args) => RefreshInvoices();
            closeButton.Click += (sender, args) => Close();
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
            Shown += (sender, args) => RefreshInvoices();
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

    public partial class PurchaseInvoiceEditorForm : Form
    {
        private AuthenticatedUser user;
        private readonly BindingList<PurchaseInvoiceLineDraft> lines =
            new BindingList<PurchaseInvoiceLineDraft>();
        private List<ProductUnitOption> productUnits;

        internal string InvoiceNumber { get; private set; }

        public PurchaseInvoiceEditorForm()
        {
            InitializeComponent();
            productUnits = new List<ProductUnitOption>();
        }

        internal PurchaseInvoiceEditorForm(
            AuthenticatedUser authenticatedUser,
            IList<SupplierOption> suppliers,
            IList<CurrencyOption> currencies,
            IList<ProductUnitOption> availableProductUnits)
            : this()
        {
            user = authenticatedUser;
            productUnits.Add(new ProductUnitOption(
                0, 0, "— اختر منتجًا —", string.Empty, 1m, false));
            productUnits.AddRange(availableProductUnits);
            productUnitColumn.DataSource = productUnits;
            linesGrid.DataSource = lines;

            supplierComboBox.DataSource = new List<SupplierOption>(suppliers);
            if (suppliers.Count > 0)
            {
                supplierComboBox.SelectedIndex = 0;
            }

            currencyComboBox.DataSource = new List<CurrencyOption>(currencies);
            if (currencies.Count > 0)
            {
                currencyComboBox.SelectedIndex = 0;
            }

            CurrencyOption baseCurrency = currencies.FirstOrDefault(currency => currency.IsBase);
            currencyComboBox.SelectedItem = baseCurrency;
            linesGrid.DataError += LinesGrid_DataError;
            saveButton.Click += SaveButton_Click;
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

    public partial class PurchaseInvoiceDetailsForm : Form
    {
        public PurchaseInvoiceDetailsForm()
        {
            InitializeComponent();
        }

        internal PurchaseInvoiceDetailsForm(
            PurchaseInvoiceRecord invoice,
            IList<PurchaseInvoiceLineRecord> lines)
            : this()
        {
            Text = "تفاصيل فاتورة " + invoice.InvoiceNo;
            headerLabel.Text = string.Format(
                CultureInfo.CurrentCulture,
                "المورد: {0}    رقم المورد: {1}    التاريخ: {2:d}    الإجمالي: {3:N4} {4}",
                invoice.SupplierName,
                string.IsNullOrEmpty(invoice.SupplierInvoiceNo) ? "—" : invoice.SupplierInvoiceNo,
                invoice.InvoiceDate,
                invoice.Total,
                invoice.CurrencyCode);
            linesGrid.DataSource = new List<PurchaseInvoiceLineRecord>(lines);
            closeButton.Click += (sender, args) => Close();
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
