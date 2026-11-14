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
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "Id",
                    DataPropertyName = "Id",
                    Visible = false
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "InvoiceNo",
                    DataPropertyName = "InvoiceNo",
                    HeaderText = "رقم الفاتورة",
                    FillWeight = 80F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "SupplierInvoiceNo",
                    DataPropertyName = "SupplierInvoiceNo",
                    HeaderText = "رقم فاتورة المورد",
                    FillWeight = 90F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "SupplierName",
                    DataPropertyName = "SupplierName",
                    HeaderText = "المورد",
                    FillWeight = 120F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "InvoiceDate",
                    DataPropertyName = "InvoiceDate",
                    HeaderText = "التاريخ",
                    FillWeight = 75F,
                    DefaultCellStyle = { Format = "d" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "CurrencyCode",
                    DataPropertyName = "CurrencyCode",
                    HeaderText = "العملة",
                    FillWeight = 45F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "Total",
                    DataPropertyName = "Total",
                    HeaderText = "الإجمالي",
                    FillWeight = 75F,
                    DefaultCellStyle = { Format = "N4" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "TotalBase",
                    DataPropertyName = "TotalBase",
                    HeaderText = "الإجمالي بالعملة الأساسية",
                    FillWeight = 95F,
                    DefaultCellStyle = { Format = "N4" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "LineCount",
                    DataPropertyName = "LineCount",
                    HeaderText = "الأصناف",
                    FillWeight = 50F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    DataPropertyName = "Status",
                    HeaderText = "الحالة",
                    FillWeight = 55F
                }
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
