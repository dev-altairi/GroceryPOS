using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class SaleCartItem : INotifyPropertyChanged
    {
        private decimal quantity;
        private decimal discount;

        public event PropertyChangedEventHandler PropertyChanged;

        public int ProductUnitId { get; set; }
        public string ProductName { get; set; }
        public string UnitName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity
        {
            get { return quantity; }
            set
            {
                if (quantity != value)
                {
                    quantity = value;
                    BelowCostConfirmed = false;
                    OnPropertyChanged("Quantity");
                    OnPropertyChanged("LineTotal");
                }
            }
        }

        public decimal Discount
        {
            get { return discount; }
            set
            {
                if (discount != value)
                {
                    discount = value;
                    BelowCostConfirmed = false;
                    OnPropertyChanged("Discount");
                    OnPropertyChanged("LineTotal");
                }
            }
        }

        public decimal AverageCost { get; set; }
        public decimal FactorToBase { get; set; }
        public bool BelowCostConfirmed { get; set; }

        public decimal LineTotal
        {
            get { return (UnitPrice * Quantity) - Discount; }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public partial class SalesForm : Form
    {
        private AuthenticatedUser user;
        private readonly BindingList<SaleCartItem> cart = new BindingList<SaleCartItem>();
        private string checkoutIdempotencyKey = Guid.NewGuid().ToString("N");

        public SalesForm()
        {
            InitializeComponent();
            resultsGrid.DataSource = new List<SaleUnitOption>();
            cartGrid.DataSource = cart;
        }

        internal SalesForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            searchInput.KeyDown += SearchInput_KeyDown;
            searchButton.Click += (sender, args) => SearchProducts();
            addButton.Click += (sender, args) => AddSelectedResult();
            resultsGrid.CellDoubleClick += (sender, args) => AddSelectedResult();
            discountColumn.ReadOnly = !user.HasPermission("Sales.Discount");
            cartGrid.CellValueChanged += (sender, args) => UpdateTotal();
            cartGrid.RowsRemoved += (sender, args) => UpdateTotal();
            closeButton.Click += (sender, args) => Close();
            removeButton.Click += (sender, args) => RemoveSelectedItem();
            checkoutButton.Click += CheckoutButton_Click;
            Shown += (sender, args) => searchInput.Focus();
        }

        private void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchProducts();
            }
        }

        private void SearchProducts()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchInput.Text))
                {
                    MessageBox.Show("أدخل باركودًا أو جزءًا من اسم المنتج للبحث.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                List<SaleUnitOption> matches = SalesService.SearchUnits(user, searchInput.Text);
                resultsGrid.DataSource = matches;
                if (matches.Count == 0)
                {
                    MessageBox.Show("لم يُعثر على منتج مطابق.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaleUnitOption exactBarcode = matches.FirstOrDefault(
                    item => string.Equals(item.Barcode, searchInput.Text.Trim(), StringComparison.Ordinal));
                if (exactBarcode != null)
                {
                    AddUnit(exactBarcode);
                    searchInput.Clear();
                }
                else
                {
                    resultsGrid.Focus();
                }
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر البحث عن المنتجات.");
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

        private SaleUnitOption SelectedResult()
        {
            return resultsGrid.CurrentRow == null
                ? null
                : resultsGrid.CurrentRow.DataBoundItem as SaleUnitOption;
        }

        private void AddSelectedResult()
        {
            SaleUnitOption selected = SelectedResult();
            if (selected == null)
            {
                MessageBox.Show("ابحث عن منتج ثم حدده لإضافته إلى السلة.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AddUnit(selected);
            searchInput.Focus();
            searchInput.SelectAll();
        }

        private void AddUnit(SaleUnitOption unit)
        {
            SaleCartItem existing = cart.FirstOrDefault(item => item.ProductUnitId == unit.ProductUnitId);
            if (existing == null)
            {
                cart.Add(new SaleCartItem
                {
                    ProductUnitId = unit.ProductUnitId,
                    ProductName = unit.ProductName,
                    UnitName = unit.UnitName,
                    UnitPrice = unit.RetailPrice,
                    Quantity = quantityInput.Value,
                    AverageCost = unit.AverageCost,
                    FactorToBase = unit.FactorToBase
                });
            }
            else
            {
                try
                {
                    existing.Quantity = checked(existing.Quantity + quantityInput.Value);
                    int index = cart.IndexOf(existing);
                    cart.ResetItem(index);
                }
                catch (OverflowException)
                {
                    MessageBox.Show("كمية المنتج تتجاوز الحد المسموح.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            UpdateTotal();
        }

        private void RemoveSelectedItem()
        {
            SaleCartItem selected = cartGrid.CurrentRow == null
                ? null
                : cartGrid.CurrentRow.DataBoundItem as SaleCartItem;
            if (selected != null)
            {
                cart.Remove(selected);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            try
            {
                cartGrid.EndEdit();
                decimal total = cart.Sum(item => item.LineTotal);
                totalLabel.Text = "الإجمالي: " + total.ToString("N4", CultureInfo.CurrentCulture);
            }
            catch (OverflowException)
            {
                totalLabel.Text = "الإجمالي: خارج النطاق";
            }
        }

        private void CheckoutButton_Click(object sender, EventArgs e)
        {
            try
            {
                cartGrid.EndEdit();
                if (cart.Count == 0)
                {
                    throw new ArgumentException("أضف منتجًا واحدًا على الأقل إلى السلة.");
                }

                List<SaleCurrency> currencies = SalesService.LoadPaymentCurrencies(user);
                SaleCurrency baseCurrency = currencies.First(currency => currency.IsBase);
                decimal total = cart.Sum(item => item.LineTotal);
                List<SaleCartItem> belowCostItems = cart.Where(item =>
                    (item.UnitPrice * item.Quantity - item.Discount) / item.Quantity / item.FactorToBase
                    < item.AverageCost).ToList();
                if (belowCostItems.Count > 0)
                {
                    if (!user.HasPermission("Sales.Discount"))
                    {
                        throw new UnauthorizedAccessException("بيع أحد الأصناف بأقل من التكلفة يتطلب صلاحية الخصم.");
                    }

                    string names = string.Join("، ", belowCostItems.Select(item => item.ProductName));
                    if (MessageBox.Show(
                        "سيتم بيع المنتجات التالية بأقل من متوسط تكلفتها: " + names +
                        Environment.NewLine + "هل تريد تأكيد البيع؟",
                        Text,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return;
                    }

                    foreach (SaleCartItem item in belowCostItems)
                    {
                        item.BelowCostConfirmed = true;
                    }
                }

                using (var paymentForm = new SalePaymentForm(total, baseCurrency, currencies))
                {
                    if (paymentForm.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    SaleReceipt receipt = SalesService.PostSale(user, new SaleDraft
                    {
                        Lines = cart.Select(item => new SaleLineDraft
                        {
                            ProductUnitId = item.ProductUnitId,
                            Quantity = item.Quantity,
                            Discount = item.Discount,
                            BelowCostConfirmed = item.BelowCostConfirmed
                        }).ToList(),
                        Payments = paymentForm.Payments,
                        ChangeCurrencyId = paymentForm.ChangeCurrencyId,
                        IdempotencyKey = checkoutIdempotencyKey
                    });
                    string change = receipt.ChangeAmount <= 0
                        ? string.Empty
                        : Environment.NewLine + "الباقي: " +
                          receipt.ChangeAmount.ToString("N4", CultureInfo.CurrentCulture) + " " +
                          receipt.ChangeCurrencyCode;
                    MessageBox.Show(
                        "تم إتمام البيع. رقم الفاتورة: " + receipt.InvoiceNo +
                        Environment.NewLine + "الإجمالي: " +
                        receipt.Total.ToString("N4", CultureInfo.CurrentCulture) + " " +
                        receipt.CurrencyCode + change,
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    cart.Clear();
                    checkoutIdempotencyKey = Guid.NewGuid().ToString("N");
                    resultsGrid.DataSource = new List<SaleUnitOption>();
                    searchInput.Clear();
                    UpdateTotal();
                    searchInput.Focus();
                }
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
                PurchasingUiErrors.ShowLogged(this, error, "تعذر إتمام عملية البيع.");
            }
            catch (OverflowException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تجاوزت إحدى قيم البيع حدود الحساب.");
            }
        }
    }

    internal sealed class SalePaymentRow
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public int DecimalPlaces { get; set; }
        public decimal RateToBase { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public partial class SalePaymentForm : Form
    {
        private SaleCurrency baseCurrency;
        private readonly BindingList<SalePaymentRow> rows;

        internal IList<SalePaymentDraft> Payments
        {
            get
            {
                return rows.Where(row => row.AmountPaid > 0)
                    .Select(row => new SalePaymentDraft
                    {
                        CurrencyId = row.CurrencyId,
                        AmountPaid = row.AmountPaid
                    }).ToList();
            }
        }

        internal int? ChangeCurrencyId
        {
            get
            {
                SaleCurrency currency = changeCurrencyComboBox.SelectedItem as SaleCurrency;
                return currency == null ? (int?)null : currency.Id;
            }
        }

        public SalePaymentForm()
        {
            InitializeComponent();
            rows = new BindingList<SalePaymentRow>();
            grid.DataSource = rows;
        }

        internal SalePaymentForm(
            decimal total,
            SaleCurrency baseCurrencyOption,
            IList<SaleCurrency> currencies)
            : this()
        {
            baseCurrency = baseCurrencyOption;
            requiredLabel.Text = "المطلوب: " + total.ToString("N4", CultureInfo.CurrentCulture) +
                                 " " + baseCurrency.Code;
            foreach (SaleCurrency currency in currencies)
            {
                rows.Add(new SalePaymentRow
                {
                    CurrencyId = currency.Id,
                    CurrencyCode = currency.Code,
                    CurrencyName = currency.Name,
                    DecimalPlaces = currency.DecimalPlaces,
                    RateToBase = currency.RateToBase,
                    AmountPaid = 0m
                });
            }

            grid.CellValueChanged += (sender, args) => UpdatePaidTotal();
            grid.DataError += (sender, args) =>
            {
                if (args.Exception != null)
                {
                    args.ThrowException = false;
                    PurchasingUiErrors.ShowLogged(
                        this,
                        args.Exception,
                        "مبلغ الدفع غير صالح؛ راجع منازل العملة.");
                }
            };
            changeCurrencyComboBox.DataSource = new List<SaleCurrency>(currencies);
            changeCurrencyComboBox.SelectedItem = currencies.FirstOrDefault(currency => currency.IsBase);
            saveButton.Click += SaveButton_Click;
            UpdatePaidTotal();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            grid.EndEdit();
            if (Payments.Count == 0)
            {
                MessageBox.Show("أدخل مبلغًا مستلمًا واحدًا على الأقل.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdatePaidTotal()
        {
            try
            {
                grid.EndEdit();
                decimal amount = 0m;
                foreach (SalePaymentRow row in rows)
                {
                    decimal converted = checked(row.AmountPaid * row.RateToBase);
                    amount = checked(amount + SalesService.RoundToCurrency(converted, baseCurrency));
                }

                paidLabel.Text = "المجموع بالعملة الأساسية: " +
                                 amount.ToString(
                                     "N" + baseCurrency.DecimalPlaces,
                                     CultureInfo.CurrentCulture) +
                                 " " + baseCurrency.Code;
            }
            catch (OverflowException)
            {
                paidLabel.Text = "مجموع الدفع خارج النطاق";
            }
        }
    }
}
