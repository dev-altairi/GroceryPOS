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
    internal sealed class SalesReturnForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly DataGridView invoicesGrid;
        private readonly DataGridView linesGrid;
        private readonly BindingList<SaleReturnLine> lines = new BindingList<SaleReturnLine>();
        private readonly Dictionary<long, string> idempotencyKeys = new Dictionary<long, string>();
        private readonly TextBox reasonInput;
        private readonly Label totalLabel;
        private long currentInvoiceId;
        private string idempotencyKey = Guid.NewGuid().ToString("N");

        internal SalesReturnForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "مرتجعات المبيعات";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1100, 720);
            MinimumSize = new Size(900, 600);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var invoicePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                Padding = new Padding(12)
            };
            invoicePanel.Controls.Add(new Label
            {
                Text = "فواتير البيع القابلة للإرجاع",
                AutoSize = true,
                Location = new Point(870, 12)
            });
            invoicesGrid = CreateGrid();
            invoicesGrid.Location = new Point(20, 42);
            invoicesGrid.Size = new Size(1050, 155);
            invoicesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            invoicesGrid.SelectionChanged += (sender, args) => InvoiceSelectionChanged();
            AddColumn(invoicesGrid, "InvoiceNo", "رقم الفاتورة", 75, true);
            AddColumn(invoicesGrid, "SaleDate", "تاريخ البيع", 70, true, "g");
            AddColumn(invoicesGrid, "CurrencyCode", "العملة", 35, true);
            AddColumn(invoicesGrid, "Total", "الإجمالي", 55, true, "N4");
            AddColumn(invoicesGrid, "ReturnedTotal", "المردود", 55, true, "N4");
            var loadButton = CreateButton("تحميل الأصناف");
            loadButton.Name = "loadInvoiceButton";
            loadButton.Location = new Point(20, 12);
            loadButton.Click += (sender, args) => LoadSelectedInvoiceLines();
            invoicePanel.Controls.Add(loadButton);

            var detailsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            detailsPanel.Controls.Add(new Label
            {
                Text = "أدخل الكمية الصالحة والكمية التالفة كلٌّ على حدة. تُعاد الصالحة للمخزون فقط.",
                AutoSize = true,
                Location = new Point(420, 10)
            });
            linesGrid = CreateGrid();
            linesGrid.Location = new Point(20, 42);
            linesGrid.Size = new Size(1050, 350);
            linesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            linesGrid.AllowUserToAddRows = false;
            linesGrid.AllowUserToDeleteRows = false;
            linesGrid.DataSource = lines;
            linesGrid.CellValueChanged += (sender, args) => UpdatePreview();
            linesGrid.DataError += (sender, args) =>
            {
                args.ThrowException = false;
                if (args.Exception != null)
                {
                    PurchasingUiErrors.ShowLogged(
                        this,
                        args.Exception,
                        "كمية الإرجاع غير صالحة.");
                }
            };
            AddColumn(linesGrid, "ProductName", "المنتج", 110, true);
            AddColumn(linesGrid, "UnitName", "الوحدة", 45, true);
            AddColumn(linesGrid, "QuantitySold", "المباع", 45, true, "N3");
            AddColumn(linesGrid, "QuantityReturned", "المردود", 50, true, "N3");
            AddColumn(linesGrid, "RemainingQuantity", "المتبقي", 50, true, "N3");
            AddColumn(linesGrid, "UnitPrice", "سعر البيع", 55, true, "N4");
            AddColumn(linesGrid, "GoodQuantity", "كمية صالحة", 70, false, "N3");
            AddColumn(linesGrid, "DamagedQuantity", "كمية تالفة", 70, false, "N3");
            detailsPanel.Controls.Add(linesGrid);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 105,
                Padding = new Padding(12)
            };
            footer.Controls.Add(new Label
            {
                Text = "سبب الإرجاع (اختياري)",
                AutoSize = true,
                Location = new Point(870, 16)
            });
            reasonInput = new TextBox
            {
                Location = new Point(350, 12),
                Width = 500,
                MaxLength = 300,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            footer.Controls.Add(reasonInput);
            totalLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(690, 58),
                Text = "قيمة الرد التقديرية: 0"
            };
            footer.Controls.Add(totalLabel);
            var postButton = CreateButton("تأكيد المرتجع");
            postButton.Width = 150;
            postButton.Location = new Point(510, 52);
            postButton.Click += (sender, args) => PostReturn();
            footer.Controls.Add(postButton);
            var refreshButton = CreateButton("تحديث الفواتير");
            refreshButton.Width = 130;
            refreshButton.Location = new Point(365, 52);
            refreshButton.Click += (sender, args) => LoadInvoices();
            footer.Controls.Add(refreshButton);
            Controls.Add(detailsPanel);
            Controls.Add(invoicePanel);
            Controls.Add(footer);

            Shown += (sender, args) => LoadInvoices();
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
                Dock = DockStyle.None,
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

        private void LoadInvoices()
        {
            try
            {
                invoicesGrid.DataSource = SalesReturnsService.LoadReturnableInvoices(user);
                lines.Clear();
                currentInvoiceId = 0;
                UpdatePreview();
                UpdateLoadButton();
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل فواتير البيع القابلة للإرجاع.");
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

        private void UpdateLoadButton()
        {
            Button button = Controls.Find("loadInvoiceButton", true).FirstOrDefault() as Button;
            if (button != null)
            {
                button.Enabled = SelectedInvoice() != null;
            }
        }

        private void InvoiceSelectionChanged()
        {
            SaleReturnInvoice selected = SelectedInvoice();
            if (selected == null || selected.Id != currentInvoiceId)
            {
                currentInvoiceId = 0;
                lines.Clear();
                UpdatePreview();
            }

            UpdateLoadButton();
        }

        private SaleReturnInvoice SelectedInvoice()
        {
            return invoicesGrid.CurrentRow == null
                ? null
                : invoicesGrid.CurrentRow.DataBoundItem as SaleReturnInvoice;
        }

        private void LoadSelectedInvoiceLines()
        {
            try
            {
                SaleReturnInvoice invoice = SelectedInvoice();
                if (invoice == null)
                {
                    MessageBox.Show("حدد فاتورة بيع أولاً.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                List<SaleReturnLine> loadedLines =
                    SalesReturnsService.LoadReturnableLines(user, invoice.Id);
                linesGrid.EndEdit();
                lines.Clear();
                foreach (SaleReturnLine line in loadedLines)
                {
                    lines.Add(line);
                }

                currentInvoiceId = invoice.Id;
                if (!idempotencyKeys.TryGetValue(invoice.Id, out idempotencyKey))
                {
                    idempotencyKey = Guid.NewGuid().ToString("N");
                    idempotencyKeys.Add(invoice.Id, idempotencyKey);
                }

                UpdatePreview();
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل أصناف الفاتورة.");
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

        private void UpdatePreview()
        {
            try
            {
                linesGrid.EndEdit();
                totalLabel.Text = "قيمة الرد التقديرية: " +
                                  CalculatePreviewTotal().ToString("N4", CultureInfo.CurrentCulture);
            }
            catch (OverflowException)
            {
                totalLabel.Text = "قيمة الرد التقديرية: خارج النطاق";
            }
        }

        private decimal CalculatePreviewTotal()
        {
            decimal total = 0m;
            foreach (SaleReturnLine line in lines)
            {
                decimal quantity = checked(line.GoodQuantity + line.DamagedQuantity);
                if (quantity <= 0 || line.RemainingQuantity <= 0)
                {
                    continue;
                }

                decimal remainingValue = line.LineTotal - line.ReturnedTotal;
                decimal amount = quantity >= line.RemainingQuantity
                    ? remainingValue
                    : decimal.Round(
                        checked(remainingValue * (quantity / line.RemainingQuantity)),
                        4,
                        MidpointRounding.AwayFromZero);
                total = checked(total + amount);
            }

            return total;
        }

        private void PostReturn()
        {
            try
            {
                linesGrid.EndEdit();
                if (lines.Any(line => line.GoodQuantity < 0 || line.DamagedQuantity < 0))
                {
                    throw new ArgumentException("كميات الإرجاع لا يمكن أن تكون سالبة.");
                }

                if (lines.Any(line =>
                    line.GoodQuantity + line.DamagedQuantity > line.RemainingQuantity))
                {
                    throw new ArgumentException("كمية الإرجاع تتجاوز الكمية المتبقية في الفاتورة.");
                }

                List<SaleReturnLineDraft> selectedLines = lines
                    .SelectMany(line => new[]
                    {
                        new SaleReturnLineDraft
                        {
                            InvoiceLineId = line.InvoiceLineId,
                            Quantity = line.GoodQuantity,
                            ItemCondition = "Good"
                        },
                        new SaleReturnLineDraft
                        {
                            InvoiceLineId = line.InvoiceLineId,
                            Quantity = line.DamagedQuantity,
                            ItemCondition = "Damaged"
                        }
                    })
                    .Where(line => line.Quantity > 0)
                    .ToList();
                if (currentInvoiceId <= 0 || selectedLines.Count == 0)
                {
                    throw new ArgumentException("حمّل فاتورة البيع وأدخل كمية صالحة للإرجاع.");
                }

                SaleReturnInvoice invoice = SelectedInvoice();
                if (invoice == null || invoice.Id != currentInvoiceId)
                {
                    throw new InvalidOperationException("حدد الفاتورة المحملة قبل تأكيد المرتجع.");
                }

                decimal estimatedRefund = CalculatePreviewTotal();
                if (MessageBox.Show(
                    "سيُسجل مرتجع للفاتورة " + invoice.InvoiceNo +
                    " مع رد نقدي تقديري قدره " +
                    estimatedRefund.ToString("N4", CultureInfo.CurrentCulture) + " " +
                    invoice.CurrencyCode +
                    Environment.NewLine + "لن تُعاد الكميات التالفة إلى المخزون. هل تريد المتابعة؟",
                    Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }

                SaleReturnReceipt receipt = SalesReturnsService.PostReturn(user, new SaleReturnDraft
                {
                    InvoiceId = currentInvoiceId,
                    Reason = reasonInput.Text,
                    IdempotencyKey = idempotencyKey,
                    Lines = selectedLines
                });
                MessageBox.Show(
                    "تم تسجيل المرتجع رقم " + receipt.ReturnNo +
                    Environment.NewLine + "المبلغ المردود نقدًا: " +
                    receipt.Total.ToString("N4", CultureInfo.CurrentCulture) + " " +
                    receipt.CurrencyCode,
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                idempotencyKeys.Remove(currentInvoiceId);
                reasonInput.Clear();
                LoadInvoices();
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
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تسجيل مرتجع المبيعات.");
            }
            catch (OverflowException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تجاوزت إحدى قيم المرتجع حدود الحساب.");
            }
        }
    }
}
