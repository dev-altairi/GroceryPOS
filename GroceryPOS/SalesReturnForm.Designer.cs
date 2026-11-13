namespace GroceryPOS
{
    public partial class SalesReturnForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel invoicePanel;
        private System.Windows.Forms.Label invoicesLabel;
        private System.Windows.Forms.Button loadInvoiceButton;
        private System.Windows.Forms.DataGridView invoicesGrid;
        private System.Windows.Forms.Panel detailsPanel;
        private System.Windows.Forms.Label detailsLabel;
        private System.Windows.Forms.DataGridView linesGrid;
        private System.Windows.Forms.Panel footer;
        private System.Windows.Forms.Label reasonLabel;
        private System.Windows.Forms.TextBox reasonInput;
        private System.Windows.Forms.Label totalLabel;
        private System.Windows.Forms.Button postButton;
        private System.Windows.Forms.Button refreshButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.invoicePanel = new System.Windows.Forms.Panel();
            this.invoicesLabel = new System.Windows.Forms.Label();
            this.loadInvoiceButton = new System.Windows.Forms.Button();
            this.invoicesGrid = new System.Windows.Forms.DataGridView();
            this.detailsPanel = new System.Windows.Forms.Panel();
            this.detailsLabel = new System.Windows.Forms.Label();
            this.linesGrid = new System.Windows.Forms.DataGridView();
            this.footer = new System.Windows.Forms.Panel();
            this.reasonLabel = new System.Windows.Forms.Label();
            this.reasonInput = new System.Windows.Forms.TextBox();
            this.totalLabel = new System.Windows.Forms.Label();
            this.postButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.invoicePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.invoicesGrid)).BeginInit();
            this.detailsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.linesGrid)).BeginInit();
            this.footer.SuspendLayout();
            this.SuspendLayout();
            //
            // invoicePanel
            //
            this.invoicePanel.Controls.Add(this.invoicesLabel);
            this.invoicePanel.Controls.Add(this.loadInvoiceButton);
            this.invoicePanel.Controls.Add(this.invoicesGrid);
            this.invoicePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.invoicePanel.Height = 220;
            this.invoicePanel.Name = "invoicePanel";
            this.invoicePanel.Padding = new System.Windows.Forms.Padding(12);
            //
            // invoicesLabel
            //
            this.invoicesLabel.AutoSize = true;
            this.invoicesLabel.Location = new System.Drawing.Point(870, 12);
            this.invoicesLabel.Text = "فواتير البيع القابلة للإرجاع";
            //
            // loadInvoiceButton
            //
            this.loadInvoiceButton.Enabled = false;
            this.loadInvoiceButton.Location = new System.Drawing.Point(20, 12);
            this.loadInvoiceButton.Size = new System.Drawing.Size(120, 34);
            this.loadInvoiceButton.Text = "تحميل الأصناف";
            this.loadInvoiceButton.UseVisualStyleBackColor = true;
            //
            // invoicesGrid
            //
            this.invoicesGrid.AllowUserToAddRows = false;
            this.invoicesGrid.AllowUserToDeleteRows = false;
            this.invoicesGrid.AutoGenerateColumns = false;
            this.invoicesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.invoicesGrid.BackgroundColor = System.Drawing.Color.White;
            this.invoicesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "InvoiceNo",
                    DataPropertyName = "InvoiceNo",
                    HeaderText = "رقم الفاتورة",
                    FillWeight = 75F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "SaleDate",
                    DataPropertyName = "SaleDate",
                    HeaderText = "تاريخ البيع",
                    FillWeight = 70F,
                    DefaultCellStyle = { Format = "g" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "CurrencyCode",
                    DataPropertyName = "CurrencyCode",
                    HeaderText = "العملة",
                    FillWeight = 35F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "Total",
                    DataPropertyName = "Total",
                    HeaderText = "الإجمالي",
                    FillWeight = 55F,
                    DefaultCellStyle = { Format = "N4" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "ReturnedTotal",
                    DataPropertyName = "ReturnedTotal",
                    HeaderText = "المردود",
                    FillWeight = 55F,
                    DefaultCellStyle = { Format = "N4" }
                }
            });
            this.invoicesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                       System.Windows.Forms.AnchorStyles.Left |
                                       System.Windows.Forms.AnchorStyles.Right;
            this.invoicesGrid.Location = new System.Drawing.Point(20, 42);
            this.invoicesGrid.MultiSelect = false;
            this.invoicesGrid.Name = "invoicesGrid";
            this.invoicesGrid.ReadOnly = true;
            this.invoicesGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.invoicesGrid.RowHeadersVisible = false;
            this.invoicesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.invoicesGrid.Size = new System.Drawing.Size(1050, 155);
            //
            // detailsPanel
            //
            this.detailsPanel.Controls.Add(this.detailsLabel);
            this.detailsPanel.Controls.Add(this.linesGrid);
            this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsPanel.Name = "detailsPanel";
            this.detailsPanel.Padding = new System.Windows.Forms.Padding(12);
            //
            // detailsLabel
            //
            this.detailsLabel.AutoSize = true;
            this.detailsLabel.Location = new System.Drawing.Point(420, 10);
            this.detailsLabel.Text = "أدخل الكمية الصالحة والكمية التالفة كلٌّ على حدة. تُعاد الصالحة للمخزون فقط.";
            //
            // linesGrid
            //
            this.linesGrid.AllowUserToAddRows = false;
            this.linesGrid.AllowUserToDeleteRows = false;
            this.linesGrid.AutoGenerateColumns = false;
            this.linesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.linesGrid.BackgroundColor = System.Drawing.Color.White;
            this.linesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "ProductName",
                    DataPropertyName = "ProductName",
                    HeaderText = "المنتج",
                    FillWeight = 110F,
                    ReadOnly = true
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "UnitName",
                    DataPropertyName = "UnitName",
                    HeaderText = "الوحدة",
                    FillWeight = 45F,
                    ReadOnly = true
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "QuantitySold",
                    DataPropertyName = "QuantitySold",
                    HeaderText = "المباع",
                    FillWeight = 45F,
                    ReadOnly = true,
                    DefaultCellStyle = { Format = "N3" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "QuantityReturned",
                    DataPropertyName = "QuantityReturned",
                    HeaderText = "المردود",
                    FillWeight = 50F,
                    ReadOnly = true,
                    DefaultCellStyle = { Format = "N3" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "RemainingQuantity",
                    DataPropertyName = "RemainingQuantity",
                    HeaderText = "المتبقي",
                    FillWeight = 50F,
                    ReadOnly = true,
                    DefaultCellStyle = { Format = "N3" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "UnitPrice",
                    DataPropertyName = "UnitPrice",
                    HeaderText = "سعر البيع",
                    FillWeight = 55F,
                    ReadOnly = true,
                    DefaultCellStyle = { Format = "N4" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "GoodQuantity",
                    DataPropertyName = "GoodQuantity",
                    HeaderText = "كمية صالحة",
                    FillWeight = 70F,
                    DefaultCellStyle = { Format = "N3" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "DamagedQuantity",
                    DataPropertyName = "DamagedQuantity",
                    HeaderText = "كمية تالفة",
                    FillWeight = 70F,
                    DefaultCellStyle = { Format = "N3" }
                }
            });
            this.linesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                   System.Windows.Forms.AnchorStyles.Bottom |
                                   System.Windows.Forms.AnchorStyles.Left |
                                   System.Windows.Forms.AnchorStyles.Right;
            this.linesGrid.Location = new System.Drawing.Point(20, 42);
            this.linesGrid.MultiSelect = false;
            this.linesGrid.Name = "linesGrid";
            this.linesGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.linesGrid.RowHeadersVisible = false;
            this.linesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.linesGrid.Size = new System.Drawing.Size(1050, 350);
            //
            // footer
            //
            this.footer.Controls.Add(this.reasonLabel);
            this.footer.Controls.Add(this.reasonInput);
            this.footer.Controls.Add(this.totalLabel);
            this.footer.Controls.Add(this.postButton);
            this.footer.Controls.Add(this.refreshButton);
            this.footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footer.Height = 105;
            this.footer.Name = "footer";
            this.footer.Padding = new System.Windows.Forms.Padding(12);
            //
            // reasonLabel
            //
            this.reasonLabel.AutoSize = true;
            this.reasonLabel.Location = new System.Drawing.Point(870, 16);
            this.reasonLabel.Text = "سبب الإرجاع (اختياري)";
            //
            // reasonInput
            //
            this.reasonInput.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                      System.Windows.Forms.AnchorStyles.Left |
                                      System.Windows.Forms.AnchorStyles.Right;
            this.reasonInput.Location = new System.Drawing.Point(350, 12);
            this.reasonInput.MaxLength = 300;
            this.reasonInput.Size = new System.Drawing.Size(500, 25);
            //
            // totalLabel
            //
            this.totalLabel.AutoSize = true;
            this.totalLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.totalLabel.Location = new System.Drawing.Point(690, 58);
            this.totalLabel.Text = "قيمة الرد التقديرية: 0";
            //
            // postButton
            //
            this.postButton.Location = new System.Drawing.Point(510, 52);
            this.postButton.Size = new System.Drawing.Size(150, 34);
            this.postButton.Text = "تأكيد المرتجع";
            this.postButton.UseVisualStyleBackColor = true;
            //
            // refreshButton
            //
            this.refreshButton.Location = new System.Drawing.Point(365, 52);
            this.refreshButton.Size = new System.Drawing.Size(130, 34);
            this.refreshButton.Text = "تحديث الفواتير";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // SalesReturnForm
            //
            this.ClientSize = new System.Drawing.Size(1100, 720);
            this.Controls.Add(this.detailsPanel);
            this.Controls.Add(this.invoicePanel);
            this.Controls.Add(this.footer);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "SalesReturnForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "مرتجعات المبيعات";
            this.invoicePanel.ResumeLayout(false);
            this.invoicePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.invoicesGrid)).EndInit();
            this.detailsPanel.ResumeLayout(false);
            this.detailsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.linesGrid)).EndInit();
            this.footer.ResumeLayout(false);
            this.footer.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
