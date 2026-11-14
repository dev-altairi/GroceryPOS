namespace GroceryPOS
{
    public partial class ProductUnitsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label noteLabel;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewComboBoxColumn unitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn factorColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn barcodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn retailPriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn wholesalePriceColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn defaultSaleUnitColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn activeColumn;
        private System.Windows.Forms.FlowLayoutPanel footer;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Label pricePermissionLabel;

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
            this.noteLabel = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.unitColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.factorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barcodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.retailPriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wholesalePriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.defaultSaleUnitColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.activeColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.footer = new System.Windows.Forms.FlowLayoutPanel();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.pricePermissionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.footer.SuspendLayout();
            this.SuspendLayout();
            //
            // noteLabel
            //
            this.noteLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.noteLabel.Height = 38;
            this.noteLabel.Name = "noteLabel";
            this.noteLabel.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.noteLabel.Text = "معامل التحويل: كم وحدة أساسية يحتويها صف الوحدة. يجب أن تبقى الوحدة الأساسية بمعامل 1.";
            this.noteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // grid
            //
            this.grid.AllowUserToAddRows = true;
            this.grid.AllowUserToDeleteRows = true;
            this.grid.AutoGenerateColumns = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.unitColumn,
                this.factorColumn,
                this.barcodeColumn,
                this.retailPriceColumn,
                this.wholesalePriceColumn,
                this.defaultSaleUnitColumn,
                this.activeColumn
            });
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //
            // unitColumn
            //
            this.unitColumn.DataPropertyName = "UnitId";
            this.unitColumn.DisplayMember = "Name";
            this.unitColumn.FillWeight = 120F;
            this.unitColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.unitColumn.HeaderText = "الوحدة";
            this.unitColumn.Name = "UnitId";
            this.unitColumn.ValueMember = "Id";
            //
            // factorColumn
            //
            this.factorColumn.DataPropertyName = "FactorToBase";
            this.factorColumn.DefaultCellStyle.Format = "N6";
            this.factorColumn.FillWeight = 90F;
            this.factorColumn.HeaderText = "معامل التحويل";
            this.factorColumn.Name = "FactorToBase";
            //
            // barcodeColumn
            //
            this.barcodeColumn.DataPropertyName = "Barcode";
            this.barcodeColumn.FillWeight = 140F;
            this.barcodeColumn.HeaderText = "الباركود";
            this.barcodeColumn.Name = "Barcode";
            //
            // retailPriceColumn
            //
            this.retailPriceColumn.DataPropertyName = "RetailPrice";
            this.retailPriceColumn.DefaultCellStyle.Format = "N4";
            this.retailPriceColumn.FillWeight = 95F;
            this.retailPriceColumn.HeaderText = "سعر البيع";
            this.retailPriceColumn.Name = "RetailPrice";
            //
            // wholesalePriceColumn
            //
            this.wholesalePriceColumn.DataPropertyName = "WholesalePrice";
            this.wholesalePriceColumn.DefaultCellStyle.Format = "N4";
            this.wholesalePriceColumn.FillWeight = 95F;
            this.wholesalePriceColumn.HeaderText = "سعر الجملة";
            this.wholesalePriceColumn.Name = "WholesalePrice";
            //
            // defaultSaleUnitColumn
            //
            this.defaultSaleUnitColumn.DataPropertyName = "IsDefaultSaleUnit";
            this.defaultSaleUnitColumn.FillWeight = 70F;
            this.defaultSaleUnitColumn.HeaderText = "افتراضية";
            this.defaultSaleUnitColumn.Name = "IsDefaultSaleUnit";
            //
            // activeColumn
            //
            this.activeColumn.DataPropertyName = "IsActive";
            this.activeColumn.FillWeight = 60F;
            this.activeColumn.HeaderText = "نشطة";
            this.activeColumn.Name = "IsActive";
            //
            // footer
            //
            this.footer.Controls.Add(this.saveButton);
            this.footer.Controls.Add(this.cancelButton);
            this.footer.Controls.Add(this.addButton);
            this.footer.Controls.Add(this.pricePermissionLabel);
            this.footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.footer.Height = 52;
            this.footer.Name = "footer";
            this.footer.Padding = new System.Windows.Forms.Padding(8);
            this.footer.WrapContents = false;
            //
            // saveButton
            //
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.Size = new System.Drawing.Size(100, 32);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Size = new System.Drawing.Size(100, 32);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // addButton
            //
            this.addButton.Margin = new System.Windows.Forms.Padding(4);
            this.addButton.Size = new System.Drawing.Size(100, 32);
            this.addButton.Text = "إضافة وحدة";
            this.addButton.UseVisualStyleBackColor = true;
            //
            // pricePermissionLabel
            //
            this.pricePermissionLabel.AutoSize = true;
            this.pricePermissionLabel.Padding = new System.Windows.Forms.Padding(12, 8, 12, 0);
            this.pricePermissionLabel.Name = "pricePermissionLabel";
            this.pricePermissionLabel.Text = "تعديل الأسعار يتطلب إذن Products.EditPrice.";
            this.pricePermissionLabel.Visible = false;
            //
            // ProductUnitsForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1040, 510);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.footer);
            this.Controls.Add(this.noteLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(850, 430);
            this.Name = "ProductUnitsForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "الوحدات والباركود";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.footer.ResumeLayout(false);
            this.footer.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
