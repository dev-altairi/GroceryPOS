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
    public partial class SalesReturnForm : Form
    {
        private AuthenticatedUser user;
        private readonly BindingList<SaleReturnLine> lines = new BindingList<SaleReturnLine>();
        private readonly Dictionary<long, string> idempotencyKeys = new Dictionary<long, string>();
        private long currentInvoiceId;
        private string idempotencyKey = Guid.NewGuid().ToString("N");

        public SalesReturnForm()
        {
            InitializeComponent();
            linesGrid.DataSource = lines;
        }

        internal SalesReturnForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            invoicesGrid.SelectionChanged += (sender, args) => InvoiceSelectionChanged();
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
            loadInvoiceButton.Click += (sender, args) => LoadSelectedInvoiceLines();
            postButton.Click += (sender, args) => PostReturn();
            refreshButton.Click += (sender, args) => LoadInvoices();
            Shown += (sender, args) => LoadInvoices();
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
            loadInvoiceButton.Enabled = SelectedInvoice() != null;
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
