namespace GroceryPOS
{
    public partial class SalesReturnForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel invoicePanel;
        private System.Windows.Forms.Label invoicesLabel;
        private System.Windows.Forms.Button loadInvoiceButton;
        private System.Windows.Forms.DataGridView invoicesGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn invoiceNoColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn saleDateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyCodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn returnedTotalColumn;
        private System.Windows.Forms.Panel detailsPanel;
        private System.Windows.Forms.Label detailsLabel;
        private System.Windows.Forms.DataGridView linesGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn productNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantitySoldColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityReturnedColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remainingQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitPriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn goodQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn damagedQuantityColumn;
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
            this.invoiceNoColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.saleDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currencyCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.returnedTotalColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.detailsPanel = new System.Windows.Forms.Panel();
            this.detailsLabel = new System.Windows.Forms.Label();
            this.linesGrid = new System.Windows.Forms.DataGridView();
            this.productNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantitySoldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityReturnedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remainingQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitPriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.goodQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.damagedQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.invoiceNoColumn.DataPropertyName = "InvoiceNo";
            this.invoiceNoColumn.FillWeight = 75F;
            this.invoiceNoColumn.HeaderText = "رقم الفاتورة";
            this.invoiceNoColumn.Name = "InvoiceNo";
            this.saleDateColumn.DataPropertyName = "SaleDate";
            this.saleDateColumn.DefaultCellStyle.Format = "g";
            this.saleDateColumn.FillWeight = 70F;
            this.saleDateColumn.HeaderText = "تاريخ البيع";
            this.saleDateColumn.Name = "SaleDate";
            this.currencyCodeColumn.DataPropertyName = "CurrencyCode";
            this.currencyCodeColumn.FillWeight = 35F;
            this.currencyCodeColumn.HeaderText = "العملة";
            this.currencyCodeColumn.Name = "CurrencyCode";
            this.totalColumn.DataPropertyName = "Total";
            this.totalColumn.DefaultCellStyle.Format = "N4";
            this.totalColumn.FillWeight = 55F;
            this.totalColumn.HeaderText = "الإجمالي";
            this.totalColumn.Name = "Total";
            this.returnedTotalColumn.DataPropertyName = "ReturnedTotal";
            this.returnedTotalColumn.DefaultCellStyle.Format = "N4";
            this.returnedTotalColumn.FillWeight = 55F;
            this.returnedTotalColumn.HeaderText = "المردود";
            this.returnedTotalColumn.Name = "ReturnedTotal";
            this.invoicesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.invoiceNoColumn,
                this.saleDateColumn,
                this.currencyCodeColumn,
                this.totalColumn,
                this.returnedTotalColumn
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
            this.productNameColumn.DataPropertyName = "ProductName";
            this.productNameColumn.FillWeight = 110F;
            this.productNameColumn.HeaderText = "المنتج";
            this.productNameColumn.Name = "ProductName";
            this.productNameColumn.ReadOnly = true;
            this.unitNameColumn.DataPropertyName = "UnitName";
            this.unitNameColumn.FillWeight = 45F;
            this.unitNameColumn.HeaderText = "الوحدة";
            this.unitNameColumn.Name = "UnitName";
            this.unitNameColumn.ReadOnly = true;
            this.quantitySoldColumn.DataPropertyName = "QuantitySold";
            this.quantitySoldColumn.DefaultCellStyle.Format = "N3";
            this.quantitySoldColumn.FillWeight = 45F;
            this.quantitySoldColumn.HeaderText = "المباع";
            this.quantitySoldColumn.Name = "QuantitySold";
            this.quantitySoldColumn.ReadOnly = true;
            this.quantityReturnedColumn.DataPropertyName = "QuantityReturned";
            this.quantityReturnedColumn.DefaultCellStyle.Format = "N3";
            this.quantityReturnedColumn.FillWeight = 50F;
            this.quantityReturnedColumn.HeaderText = "المردود";
            this.quantityReturnedColumn.Name = "QuantityReturned";
            this.quantityReturnedColumn.ReadOnly = true;
            this.remainingQuantityColumn.DataPropertyName = "RemainingQuantity";
            this.remainingQuantityColumn.DefaultCellStyle.Format = "N3";
            this.remainingQuantityColumn.FillWeight = 50F;
            this.remainingQuantityColumn.HeaderText = "المتبقي";
            this.remainingQuantityColumn.Name = "RemainingQuantity";
            this.remainingQuantityColumn.ReadOnly = true;
            this.unitPriceColumn.DataPropertyName = "UnitPrice";
            this.unitPriceColumn.DefaultCellStyle.Format = "N4";
            this.unitPriceColumn.FillWeight = 55F;
            this.unitPriceColumn.HeaderText = "سعر البيع";
            this.unitPriceColumn.Name = "UnitPrice";
            this.unitPriceColumn.ReadOnly = true;
            this.goodQuantityColumn.DataPropertyName = "GoodQuantity";
            this.goodQuantityColumn.DefaultCellStyle.Format = "N3";
            this.goodQuantityColumn.FillWeight = 70F;
            this.goodQuantityColumn.HeaderText = "كمية صالحة";
            this.goodQuantityColumn.Name = "GoodQuantity";
            this.damagedQuantityColumn.DataPropertyName = "DamagedQuantity";
            this.damagedQuantityColumn.DefaultCellStyle.Format = "N3";
            this.damagedQuantityColumn.FillWeight = 70F;
            this.damagedQuantityColumn.HeaderText = "كمية تالفة";
            this.damagedQuantityColumn.Name = "DamagedQuantity";
            this.linesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.productNameColumn,
                this.unitNameColumn,
                this.quantitySoldColumn,
                this.quantityReturnedColumn,
                this.remainingQuantityColumn,
                this.unitPriceColumn,
                this.goodQuantityColumn,
                this.damagedQuantityColumn
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
