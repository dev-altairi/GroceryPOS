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

    internal sealed class SalesForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly TextBox searchInput;
        private readonly NumericUpDown quantityInput;
        private readonly DataGridView resultsGrid;
        private readonly DataGridView cartGrid;
        private readonly BindingList<SaleCartItem> cart = new BindingList<SaleCartItem>();
        private readonly Label totalLabel;
        private string checkoutIdempotencyKey = Guid.NewGuid().ToString("N");

        internal SalesForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "نقطة البيع";
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                Padding = new Padding(12)
            };
            searchInput = new TextBox
            {
                Location = new Point(420, 15),
                Width = 360,
                MaxLength = 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            searchInput.KeyDown += SearchInput_KeyDown;
            searchPanel.Controls.Add(new Label
            {
                Text = "باركود أو اسم المنتج",
                AutoSize = true,
                Location = new Point(790, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            });
            searchPanel.Controls.Add(searchInput);

            var searchButton = CreateButton("بحث");
            searchButton.Location = new Point(300, 12);
            searchButton.Click += (sender, args) => SearchProducts();
            searchPanel.Controls.Add(searchButton);

            quantityInput = new NumericUpDown
            {
                Location = new Point(170, 15),
                Width = 110,
                DecimalPlaces = 3,
                Increment = 1m,
                Minimum = 0.001m,
                Maximum = 999999999999999.999m,
                Value = 1m,
                TextAlign = HorizontalAlignment.Right
            };
            searchPanel.Controls.Add(new Label
            {
                Text = "الكمية",
                AutoSize = true,
                Location = new Point(285, 18)
            });
            searchPanel.Controls.Add(quantityInput);
            var addButton = CreateButton("إضافة للسلة");
            addButton.Location = new Point(35, 12);
            addButton.Click += (sender, args) => AddSelectedResult();
            searchPanel.Controls.Add(addButton);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 250,
                RightToLeft = RightToLeft.Yes
            };
            resultsGrid = CreateGrid();
            resultsGrid.DataSource = new List<SaleUnitOption>();
            AddColumn(resultsGrid, "DisplayName", "المنتج والوحدة", 150, true);
            AddColumn(resultsGrid, "Barcode", "الباركود", 70, true);
            AddColumn(resultsGrid, "RetailPrice", "سعر البيع", 65, true, "N4");
            AddColumn(resultsGrid, "AvailableQuantity", "المخزون الإجمالي", 75, true, "N3");
            resultsGrid.CellDoubleClick += (sender, args) => AddSelectedResult();
            split.Panel1.Controls.Add(resultsGrid);

            var cartPanel = new Panel { Dock = DockStyle.Fill };
            cartGrid = CreateGrid();
            cartGrid.AllowUserToAddRows = false;
            cartGrid.DataSource = cart;
            AddColumn(cartGrid, "ProductName", "المنتج", 110, true);
            AddColumn(cartGrid, "UnitName", "الوحدة", 55, true);
            AddColumn(cartGrid, "Quantity", "الكمية", 55, false, "N3");
            AddColumn(cartGrid, "UnitPrice", "سعر الوحدة", 65, true, "N4");
            AddColumn(
                cartGrid,
                "Discount",
                "خصم",
                55,
                !user.HasPermission("Sales.Discount"),
                "N4");
            AddColumn(cartGrid, "LineTotal", "الإجمالي", 70, true, "N4");
            cartGrid.CellValueChanged += (sender, args) => UpdateTotal();
            cartGrid.RowsRemoved += (sender, args) => UpdateTotal();
            cartPanel.Controls.Add(cartGrid);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                Padding = new Padding(12)
            };
            totalLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(680, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Text = "الإجمالي: 0"
            };
            footer.Controls.Add(totalLabel);
            var removeButton = CreateButton("حذف الصنف");
            removeButton.Location = new Point(480, 12);
            removeButton.Click += (sender, args) => RemoveSelectedItem();
            footer.Controls.Add(removeButton);
            var checkoutButton = CreateButton("الدفع وإتمام البيع");
            checkoutButton.Width = 170;
            checkoutButton.Location = new Point(285, 12);
            checkoutButton.Click += CheckoutButton_Click;
            footer.Controls.Add(checkoutButton);
            var closeButton = CreateButton("إغلاق");
            closeButton.Location = new Point(155, 12);
            closeButton.Click += (sender, args) => Close();
            footer.Controls.Add(closeButton);

            split.Panel2.Controls.Add(cartPanel);
            split.Panel2.Controls.Add(footer);
            Controls.Add(split);
            Controls.Add(searchPanel);
            Shown += (sender, args) => searchInput.Focus();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 120,
                Height = 34,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = false,
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
        }

        private static void AddColumn(
            DataGridView grid,
            string property,
            string title,
            float weight,
            bool readOnly,
            string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight,
                ReadOnly = readOnly
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
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

    internal sealed class SalePaymentForm : Form
    {
        private readonly SaleCurrency baseCurrency;
        private readonly BindingList<SalePaymentRow> rows;
        private readonly DataGridView grid;
        private readonly ComboBox changeCurrencyComboBox;
        private readonly Label paidLabel;

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

        internal SalePaymentForm(
            decimal total,
            SaleCurrency baseCurrencyOption,
            IList<SaleCurrency> currencies)
        {
            baseCurrency = baseCurrencyOption;
            Text = "الدفع";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(720, 490);
            MinimumSize = new Size(640, 430);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "المطلوب: " + total.ToString("N4", CultureInfo.CurrentCulture) +
                       " " + baseCurrency.Code,
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(400, 20)
            });
            Controls.Add(new Label
            {
                Text = "أدخل المبلغ المستلم في كل عملة. تُحوّل بقية العملات بسعر الصرف المسجل لهذا اليوم.",
                AutoSize = true,
                Location = new Point(25, 65)
            });

            rows = new BindingList<SalePaymentRow>(currencies.Select(currency => new SalePaymentRow
            {
                CurrencyId = currency.Id,
                CurrencyCode = currency.Code,
                CurrencyName = currency.Name,
                DecimalPlaces = currency.DecimalPlaces,
                RateToBase = currency.RateToBase,
                AmountPaid = 0m
            }).ToList());
            grid = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(680, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White,
                DataSource = rows
            };
            AddColumn("CurrencyCode", "العملة", 45, true);
            AddColumn("CurrencyName", "الوصف", 80, true);
            AddColumn("RateToBase", "سعر الصرف", 75, true, "N6");
            AddColumn("AmountPaid", "المبلغ المدفوع", 100, false, "N4");
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
            Controls.Add(grid);

            Controls.Add(new Label
            {
                Text = "عملة الباقي",
                AutoSize = true,
                Location = new Point(595, 365)
            });
            changeCurrencyComboBox = new ComboBox
            {
                Location = new Point(285, 360),
                Width = 290,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = new List<SaleCurrency>(currencies)
            };
            changeCurrencyComboBox.SelectedItem = currencies.FirstOrDefault(currency => currency.IsBase);
            Controls.Add(changeCurrencyComboBox);
            paidLabel = new Label
            {
                AutoSize = true,
                Location = new Point(25, 365),
                Text = "المجموع بالعملة الأساسية: 0"
            };
            Controls.Add(paidLabel);

            var save = new Button
            {
                Text = "تأكيد الدفع",
                Location = new Point(385, 420),
                Width = 115,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            save.Click += (sender, args) =>
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
            };
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(260, 420),
                Width = 100,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private void AddColumn(
            string property,
            string title,
            float weight,
            bool readOnly,
            string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight,
                ReadOnly = readOnly
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
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
