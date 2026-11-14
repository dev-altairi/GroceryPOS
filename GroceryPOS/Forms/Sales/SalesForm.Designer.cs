namespace GroceryPOS
{
    public partial class SalesForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchInput;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Label quantityLabel;
        private System.Windows.Forms.NumericUpDown quantityInput;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.DataGridView resultsGrid;
        private System.Windows.Forms.Panel cartPanel;
        private System.Windows.Forms.DataGridView cartGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn discountColumn;
        private System.Windows.Forms.Panel footer;
        private System.Windows.Forms.Label totalLabel;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button checkoutButton;
        private System.Windows.Forms.Button closeButton;

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
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchInput = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.quantityLabel = new System.Windows.Forms.Label();
            this.quantityInput = new System.Windows.Forms.NumericUpDown();
            this.addButton = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.resultsGrid = new System.Windows.Forms.DataGridView();
            this.cartPanel = new System.Windows.Forms.Panel();
            this.cartGrid = new System.Windows.Forms.DataGridView();
            this.discountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.footer = new System.Windows.Forms.Panel();
            this.totalLabel = new System.Windows.Forms.Label();
            this.removeButton = new System.Windows.Forms.Button();
            this.checkoutButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.quantityInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).BeginInit();
            this.cartPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cartGrid)).BeginInit();
            this.footer.SuspendLayout();
            this.SuspendLayout();
            //
            // searchPanel
            //
            this.searchPanel.Controls.Add(this.searchLabel);
            this.searchPanel.Controls.Add(this.searchInput);
            this.searchPanel.Controls.Add(this.searchButton);
            this.searchPanel.Controls.Add(this.quantityLabel);
            this.searchPanel.Controls.Add(this.quantityInput);
            this.searchPanel.Controls.Add(this.addButton);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Height = 62;
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Padding = new System.Windows.Forms.Padding(12);
            //
            // searchLabel
            //
            this.searchLabel.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                      System.Windows.Forms.AnchorStyles.Right;
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(790, 18);
            this.searchLabel.Text = "باركود أو اسم المنتج";
            //
            // searchInput
            //
            this.searchInput.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                      System.Windows.Forms.AnchorStyles.Right;
            this.searchInput.Location = new System.Drawing.Point(420, 15);
            this.searchInput.MaxLength = 200;
            this.searchInput.Size = new System.Drawing.Size(360, 25);
            //
            // searchButton
            //
            this.searchButton.Location = new System.Drawing.Point(300, 12);
            this.searchButton.Size = new System.Drawing.Size(120, 34);
            this.searchButton.Text = "بحث";
            this.searchButton.UseVisualStyleBackColor = true;
            //
            // quantityLabel
            //
            this.quantityLabel.AutoSize = true;
            this.quantityLabel.Location = new System.Drawing.Point(285, 18);
            this.quantityLabel.Text = "الكمية";
            //
            // quantityInput
            //
            this.quantityInput.DecimalPlaces = 3;
            this.quantityInput.Increment = 1M;
            this.quantityInput.Location = new System.Drawing.Point(170, 15);
            this.quantityInput.Maximum = 999999999999999.999M;
            this.quantityInput.Minimum = 0.001M;
            this.quantityInput.Size = new System.Drawing.Size(110, 25);
            this.quantityInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.quantityInput.Value = 1M;
            //
            // addButton
            //
            this.addButton.Location = new System.Drawing.Point(35, 12);
            this.addButton.Size = new System.Drawing.Size(120, 34);
            this.addButton.Text = "إضافة للسلة";
            this.addButton.UseVisualStyleBackColor = true;
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 62);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer.Panel1.Controls.Add(this.resultsGrid);
            this.splitContainer.Panel2.Controls.Add(this.cartPanel);
            this.splitContainer.Panel2.Controls.Add(this.footer);
            this.splitContainer.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.splitContainer.Size = new System.Drawing.Size(1000, 588);
            this.splitContainer.SplitterDistance = 250;
            //
            // resultsGrid
            //
            this.resultsGrid.AllowUserToAddRows = false;
            this.resultsGrid.AllowUserToDeleteRows = false;
            this.resultsGrid.AutoGenerateColumns = false;
            this.resultsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.resultsGrid.BackgroundColor = System.Drawing.Color.White;
            this.resultsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "DisplayName",
                    DataPropertyName = "DisplayName",
                    HeaderText = "المنتج والوحدة",
                    FillWeight = 150F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "Barcode",
                    DataPropertyName = "Barcode",
                    HeaderText = "الباركود",
                    FillWeight = 70F
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "RetailPrice",
                    DataPropertyName = "RetailPrice",
                    HeaderText = "سعر البيع",
                    FillWeight = 65F,
                    DefaultCellStyle = { Format = "N4" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "AvailableQuantity",
                    DataPropertyName = "AvailableQuantity",
                    HeaderText = "المخزون الإجمالي",
                    FillWeight = 75F,
                    DefaultCellStyle = { Format = "N3" }
                }
            });
            this.resultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsGrid.MultiSelect = false;
            this.resultsGrid.Name = "resultsGrid";
            this.resultsGrid.ReadOnly = true;
            this.resultsGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.resultsGrid.RowHeadersVisible = false;
            this.resultsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //
            // cartPanel
            //
            this.cartPanel.Controls.Add(this.cartGrid);
            this.cartPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cartPanel.Name = "cartPanel";
            //
            // cartGrid
            //
            this.cartGrid.AllowUserToAddRows = false;
            this.cartGrid.AllowUserToDeleteRows = false;
            this.cartGrid.AutoGenerateColumns = false;
            this.cartGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.cartGrid.BackgroundColor = System.Drawing.Color.White;
            this.cartGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
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
                    FillWeight = 55F,
                    ReadOnly = true
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "Quantity",
                    DataPropertyName = "Quantity",
                    HeaderText = "الكمية",
                    FillWeight = 55F,
                    DefaultCellStyle = { Format = "N3" }
                },
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "UnitPrice",
                    DataPropertyName = "UnitPrice",
                    HeaderText = "سعر الوحدة",
                    FillWeight = 65F,
                    ReadOnly = true,
                    DefaultCellStyle = { Format = "N4" }
                },
                this.discountColumn,
                new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = "LineTotal",
                    DataPropertyName = "LineTotal",
                    HeaderText = "الإجمالي",
                    FillWeight = 70F,
                    ReadOnly = true,
                    DefaultCellStyle = { Format = "N4" }
                }
            });
            this.cartGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cartGrid.MultiSelect = false;
            this.cartGrid.Name = "cartGrid";
            this.cartGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cartGrid.RowHeadersVisible = false;
            this.cartGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //
            // discountColumn
            //
            this.discountColumn.DataPropertyName = "Discount";
            this.discountColumn.DefaultCellStyle.Format = "N4";
            this.discountColumn.FillWeight = 55F;
            this.discountColumn.HeaderText = "خصم";
            this.discountColumn.Name = "Discount";
            //
            // footer
            //
            this.footer.Controls.Add(this.totalLabel);
            this.footer.Controls.Add(this.removeButton);
            this.footer.Controls.Add(this.checkoutButton);
            this.footer.Controls.Add(this.closeButton);
            this.footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footer.Height = 65;
            this.footer.Name = "footer";
            this.footer.Padding = new System.Windows.Forms.Padding(12);
            //
            // totalLabel
            //
            this.totalLabel.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                     System.Windows.Forms.AnchorStyles.Right;
            this.totalLabel.AutoSize = true;
            this.totalLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.totalLabel.Location = new System.Drawing.Point(680, 18);
            this.totalLabel.Text = "الإجمالي: 0";
            //
            // removeButton
            //
            this.removeButton.Location = new System.Drawing.Point(480, 12);
            this.removeButton.Size = new System.Drawing.Size(120, 34);
            this.removeButton.Text = "حذف الصنف";
            this.removeButton.UseVisualStyleBackColor = true;
            //
            // checkoutButton
            //
            this.checkoutButton.Location = new System.Drawing.Point(285, 12);
            this.checkoutButton.Size = new System.Drawing.Size(170, 34);
            this.checkoutButton.Text = "الدفع وإتمام البيع";
            this.checkoutButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            this.closeButton.Location = new System.Drawing.Point(155, 12);
            this.closeButton.Size = new System.Drawing.Size(120, 34);
            this.closeButton.Text = "إغلاق";
            this.closeButton.UseVisualStyleBackColor = true;
            //
            // SalesForm
            //
            this.ClientSize = new System.Drawing.Size(1000, 650);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.searchPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Name = "SalesForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "نقطة البيع";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.quantityInput)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).EndInit();
            this.cartPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cartGrid)).EndInit();
            this.footer.ResumeLayout(false);
            this.footer.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
