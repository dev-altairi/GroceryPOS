namespace GroceryPOS
{
    public partial class ProductCatalogForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button unitsButton;
        private System.Windows.Forms.Button categoriesButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.DataGridView productsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn skuColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn categoryNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn baseUnitNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn barcodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn retailPriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn wholesalePriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn stockQuantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn minStockColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tracksExpiryColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn isActiveColumn;
        private System.Windows.Forms.Label statusLabel;

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
            this.toolbarPanel = new System.Windows.Forms.Panel();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.unitsButton = new System.Windows.Forms.Button();
            this.categoriesButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.productsGrid = new System.Windows.Forms.DataGridView();
            this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.skuColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.categoryNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.baseUnitNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barcodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.retailPriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wholesalePriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stockQuantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.minStockColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tracksExpiryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isActiveColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusLabel = new System.Windows.Forms.Label();
            this.toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.productsGrid)).BeginInit();
            this.SuspendLayout();
            //
            // toolbarPanel
            //
            this.toolbarPanel.Controls.Add(this.searchLabel);
            this.toolbarPanel.Controls.Add(this.searchTextBox);
            this.toolbarPanel.Controls.Add(this.refreshButton);
            this.toolbarPanel.Controls.Add(this.addButton);
            this.toolbarPanel.Controls.Add(this.editButton);
            this.toolbarPanel.Controls.Add(this.unitsButton);
            this.toolbarPanel.Controls.Add(this.categoriesButton);
            this.toolbarPanel.Controls.Add(this.closeButton);
            this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarPanel.Height = 65;
            this.toolbarPanel.Name = "toolbarPanel";
            this.toolbarPanel.Padding = new System.Windows.Forms.Padding(12);
            this.toolbarPanel.TabIndex = 0;
            //
            // searchLabel
            //
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(1000, 19);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(179, 19);
            this.searchLabel.TabIndex = 0;
            this.searchLabel.Text = "بحث بالاسم أو الرمز أو الباركود:";
            //
            // searchTextBox
            //
            this.searchTextBox.Location = new System.Drawing.Point(690, 14);
            this.searchTextBox.MaxLength = 200;
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(300, 25);
            this.searchTextBox.TabIndex = 1;
            //
            // refreshButton
            //
            this.refreshButton.Location = new System.Drawing.Point(600, 12);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 32);
            this.refreshButton.TabIndex = 2;
            this.refreshButton.Text = "تحديث";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // addButton
            //
            this.addButton.Location = new System.Drawing.Point(485, 12);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(100, 32);
            this.addButton.TabIndex = 3;
            this.addButton.Text = "إضافة منتج";
            this.addButton.UseVisualStyleBackColor = true;
            //
            // editButton
            //
            this.editButton.Enabled = false;
            this.editButton.Location = new System.Drawing.Point(370, 12);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(105, 32);
            this.editButton.TabIndex = 4;
            this.editButton.Text = "تعديل المحدد";
            this.editButton.UseVisualStyleBackColor = true;
            //
            // unitsButton
            //
            this.unitsButton.Enabled = false;
            this.unitsButton.Location = new System.Drawing.Point(245, 12);
            this.unitsButton.Name = "unitsButton";
            this.unitsButton.Size = new System.Drawing.Size(115, 32);
            this.unitsButton.TabIndex = 5;
            this.unitsButton.Text = "وحدات وباركود";
            this.unitsButton.UseVisualStyleBackColor = true;
            //
            // categoriesButton
            //
            this.categoriesButton.Location = new System.Drawing.Point(145, 12);
            this.categoriesButton.Name = "categoriesButton";
            this.categoriesButton.Size = new System.Drawing.Size(90, 32);
            this.categoriesButton.TabIndex = 6;
            this.categoriesButton.Text = "التصنيفات";
            this.categoriesButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                      System.Windows.Forms.AnchorStyles.Left;
            this.closeButton.Location = new System.Drawing.Point(35, 12);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 32);
            this.closeButton.TabIndex = 7;
            this.closeButton.Text = "إغلاق";
            this.closeButton.UseVisualStyleBackColor = true;
            //
            // productsGrid
            //
            this.productsGrid.AllowUserToAddRows = false;
            this.productsGrid.AllowUserToDeleteRows = false;
            this.productsGrid.AutoGenerateColumns = false;
            this.productsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.productsGrid.BackgroundColor = System.Drawing.Color.White;
            this.productsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.idColumn.DataPropertyName = "Id";
            this.idColumn.Name = "Id";
            this.idColumn.Visible = false;
            this.skuColumn.DataPropertyName = "Sku";
            this.skuColumn.FillWeight = 75F;
            this.skuColumn.HeaderText = "SKU";
            this.skuColumn.Name = "Sku";
            this.nameColumn.DataPropertyName = "Name";
            this.nameColumn.FillWeight = 170F;
            this.nameColumn.HeaderText = "اسم المنتج";
            this.nameColumn.Name = "Name";
            this.categoryNameColumn.DataPropertyName = "CategoryName";
            this.categoryNameColumn.FillWeight = 100F;
            this.categoryNameColumn.HeaderText = "التصنيف";
            this.categoryNameColumn.Name = "CategoryName";
            this.baseUnitNameColumn.DataPropertyName = "BaseUnitName";
            this.baseUnitNameColumn.FillWeight = 65F;
            this.baseUnitNameColumn.HeaderText = "الوحدة";
            this.baseUnitNameColumn.Name = "BaseUnitName";
            this.barcodeColumn.DataPropertyName = "Barcode";
            this.barcodeColumn.FillWeight = 110F;
            this.barcodeColumn.HeaderText = "الباركود";
            this.barcodeColumn.Name = "Barcode";
            this.retailPriceColumn.DataPropertyName = "RetailPrice";
            this.retailPriceColumn.DefaultCellStyle.Format = "N2";
            this.retailPriceColumn.FillWeight = 75F;
            this.retailPriceColumn.HeaderText = "سعر البيع";
            this.retailPriceColumn.Name = "RetailPrice";
            this.wholesalePriceColumn.DataPropertyName = "WholesalePrice";
            this.wholesalePriceColumn.DefaultCellStyle.Format = "N2";
            this.wholesalePriceColumn.FillWeight = 75F;
            this.wholesalePriceColumn.HeaderText = "سعر الجملة";
            this.wholesalePriceColumn.Name = "WholesalePrice";
            this.stockQuantityColumn.DataPropertyName = "StockQuantity";
            this.stockQuantityColumn.DefaultCellStyle.Format = "N3";
            this.stockQuantityColumn.FillWeight = 70F;
            this.stockQuantityColumn.HeaderText = "المخزون";
            this.stockQuantityColumn.Name = "StockQuantity";
            this.minStockColumn.DataPropertyName = "MinStock";
            this.minStockColumn.DefaultCellStyle.Format = "N3";
            this.minStockColumn.FillWeight = 70F;
            this.minStockColumn.HeaderText = "الحد الأدنى";
            this.minStockColumn.Name = "MinStock";
            this.tracksExpiryColumn.DataPropertyName = "TracksExpiry";
            this.tracksExpiryColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.tracksExpiryColumn.FillWeight = 65F;
            this.tracksExpiryColumn.HeaderText = "يتطلب صلاحية";
            this.tracksExpiryColumn.Name = "TracksExpiry";
            this.isActiveColumn.DataPropertyName = "IsActive";
            this.isActiveColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.isActiveColumn.FillWeight = 55F;
            this.isActiveColumn.HeaderText = "نشط";
            this.isActiveColumn.Name = "IsActive";
            this.productsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.idColumn,
                this.skuColumn,
                this.nameColumn,
                this.categoryNameColumn,
                this.baseUnitNameColumn,
                this.barcodeColumn,
                this.retailPriceColumn,
                this.wholesalePriceColumn,
                this.stockQuantityColumn,
                this.minStockColumn,
                this.tracksExpiryColumn,
                this.isActiveColumn
            });
            this.productsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.productsGrid.MultiSelect = false;
            this.productsGrid.Name = "productsGrid";
            this.productsGrid.ReadOnly = true;
            this.productsGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.productsGrid.RowHeadersVisible = false;
            this.productsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.productsGrid.TabIndex = 1;
            //
            // statusLabel
            //
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusLabel.Height = 32;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // ProductCatalogForm
            //
            this.ClientSize = new System.Drawing.Size(1120, 600);
            this.Controls.Add(this.productsGrid);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.toolbarPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(1120, 600);
            this.Name = "ProductCatalogForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إدارة المنتجات - GroceryPOS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.toolbarPanel.ResumeLayout(false);
            this.toolbarPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.productsGrid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
