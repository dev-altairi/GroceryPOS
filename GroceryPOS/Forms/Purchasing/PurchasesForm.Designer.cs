namespace GroceryPOS
{
    public partial class PurchasesForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn invoiceNoColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplierInvoiceNoColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplierNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn invoiceDateColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyCodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalBaseColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lineCountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusColumn;
        private System.Windows.Forms.Label status;

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
            this.toolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.invoiceNoColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.supplierInvoiceNoColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.supplierNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.invoiceDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currencyCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalBaseColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lineCountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.Label();
            this.toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // toolbar
            //
            this.toolbar.Controls.Add(this.addButton);
            this.toolbar.Controls.Add(this.refreshButton);
            this.toolbar.Controls.Add(this.closeButton);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.toolbar.Height = 55;
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(8);
            this.toolbar.WrapContents = false;
            //
            // addButton
            //
            this.addButton.Margin = new System.Windows.Forms.Padding(4);
            this.addButton.Size = new System.Drawing.Size(125, 32);
            this.addButton.Text = "استلام فاتورة";
            this.addButton.UseVisualStyleBackColor = true;
            //
            // refreshButton
            //
            this.refreshButton.Margin = new System.Windows.Forms.Padding(4);
            this.refreshButton.Size = new System.Drawing.Size(125, 32);
            this.refreshButton.Text = "تحديث";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Size = new System.Drawing.Size(125, 32);
            this.closeButton.Text = "إغلاق";
            this.closeButton.UseVisualStyleBackColor = true;
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoGenerateColumns = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.idColumn.DataPropertyName = "Id";
            this.idColumn.Name = "Id";
            this.idColumn.Visible = false;
            this.invoiceNoColumn.DataPropertyName = "InvoiceNo";
            this.invoiceNoColumn.FillWeight = 80F;
            this.invoiceNoColumn.HeaderText = "رقم الفاتورة";
            this.invoiceNoColumn.Name = "InvoiceNo";
            this.supplierInvoiceNoColumn.DataPropertyName = "SupplierInvoiceNo";
            this.supplierInvoiceNoColumn.FillWeight = 90F;
            this.supplierInvoiceNoColumn.HeaderText = "رقم فاتورة المورد";
            this.supplierInvoiceNoColumn.Name = "SupplierInvoiceNo";
            this.supplierNameColumn.DataPropertyName = "SupplierName";
            this.supplierNameColumn.FillWeight = 120F;
            this.supplierNameColumn.HeaderText = "المورد";
            this.supplierNameColumn.Name = "SupplierName";
            this.invoiceDateColumn.DataPropertyName = "InvoiceDate";
            this.invoiceDateColumn.DefaultCellStyle.Format = "d";
            this.invoiceDateColumn.FillWeight = 75F;
            this.invoiceDateColumn.HeaderText = "التاريخ";
            this.invoiceDateColumn.Name = "InvoiceDate";
            this.currencyCodeColumn.DataPropertyName = "CurrencyCode";
            this.currencyCodeColumn.FillWeight = 45F;
            this.currencyCodeColumn.HeaderText = "العملة";
            this.currencyCodeColumn.Name = "CurrencyCode";
            this.totalColumn.DataPropertyName = "Total";
            this.totalColumn.DefaultCellStyle.Format = "N4";
            this.totalColumn.FillWeight = 75F;
            this.totalColumn.HeaderText = "الإجمالي";
            this.totalColumn.Name = "Total";
            this.totalBaseColumn.DataPropertyName = "TotalBase";
            this.totalBaseColumn.DefaultCellStyle.Format = "N4";
            this.totalBaseColumn.FillWeight = 95F;
            this.totalBaseColumn.HeaderText = "الإجمالي بالعملة الأساسية";
            this.totalBaseColumn.Name = "TotalBase";
            this.lineCountColumn.DataPropertyName = "LineCount";
            this.lineCountColumn.FillWeight = 50F;
            this.lineCountColumn.HeaderText = "الأصناف";
            this.lineCountColumn.Name = "LineCount";
            this.statusColumn.DataPropertyName = "Status";
            this.statusColumn.FillWeight = 55F;
            this.statusColumn.HeaderText = "الحالة";
            this.statusColumn.Name = "Status";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.idColumn,
                this.invoiceNoColumn,
                this.supplierInvoiceNoColumn,
                this.supplierNameColumn,
                this.invoiceDateColumn,
                this.currencyCodeColumn,
                this.totalColumn,
                this.totalBaseColumn,
                this.lineCountColumn,
                this.statusColumn
            });
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //
            // status
            //
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Height = 30;
            this.status.Name = "status";
            this.status.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.status.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // PurchasesForm
            //
            this.ClientSize = new System.Drawing.Size(1000, 560);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.status);
            this.Controls.Add(this.toolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(800, 430);
            this.Name = "PurchasesForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "فواتير المشتريات";
            this.toolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
