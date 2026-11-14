namespace GroceryPOS
{
    public partial class PurchaseInvoiceDetailsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.DataGridView linesGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn productNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn qtyBaseColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitPriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lineTotalColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn batchNoColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn expiryDateColumn;
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
            this.headerLabel = new System.Windows.Forms.Label();
            this.linesGrid = new System.Windows.Forms.DataGridView();
            this.productNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qtyBaseColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitPriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lineTotalColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.batchNoColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.expiryDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.closeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.linesGrid)).BeginInit();
            this.SuspendLayout();
            //
            // headerLabel
            //
            this.headerLabel.AutoSize = true;
            this.headerLabel.Location = new System.Drawing.Point(25, 20);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Text = "ملخص الفاتورة";
            //
            // linesGrid
            //
            this.linesGrid.AllowUserToAddRows = false;
            this.linesGrid.AllowUserToDeleteRows = false;
            this.linesGrid.AutoGenerateColumns = false;
            this.linesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.linesGrid.BackgroundColor = System.Drawing.Color.White;
            this.productNameColumn.DataPropertyName = "ProductName";
            this.productNameColumn.FillWeight = 135F;
            this.productNameColumn.HeaderText = "المنتج";
            this.productNameColumn.Name = "ProductName";
            this.unitNameColumn.DataPropertyName = "UnitName";
            this.unitNameColumn.FillWeight = 65F;
            this.unitNameColumn.HeaderText = "الوحدة";
            this.unitNameColumn.Name = "UnitName";
            this.quantityColumn.DataPropertyName = "Quantity";
            this.quantityColumn.DefaultCellStyle.Format = "N3";
            this.quantityColumn.FillWeight = 65F;
            this.quantityColumn.HeaderText = "الكمية";
            this.quantityColumn.Name = "Quantity";
            this.qtyBaseColumn.DataPropertyName = "QtyBase";
            this.qtyBaseColumn.DefaultCellStyle.Format = "N3";
            this.qtyBaseColumn.FillWeight = 75F;
            this.qtyBaseColumn.HeaderText = "الكمية الأساسية";
            this.qtyBaseColumn.Name = "QtyBase";
            this.unitPriceColumn.DataPropertyName = "UnitPrice";
            this.unitPriceColumn.DefaultCellStyle.Format = "N4";
            this.unitPriceColumn.FillWeight = 75F;
            this.unitPriceColumn.HeaderText = "سعر الوحدة";
            this.unitPriceColumn.Name = "UnitPrice";
            this.lineTotalColumn.DataPropertyName = "LineTotal";
            this.lineTotalColumn.DefaultCellStyle.Format = "N4";
            this.lineTotalColumn.FillWeight = 75F;
            this.lineTotalColumn.HeaderText = "الإجمالي";
            this.lineTotalColumn.Name = "LineTotal";
            this.batchNoColumn.DataPropertyName = "BatchNo";
            this.batchNoColumn.FillWeight = 70F;
            this.batchNoColumn.HeaderText = "الدفعة";
            this.batchNoColumn.Name = "BatchNo";
            this.expiryDateColumn.DataPropertyName = "ExpiryDate";
            this.expiryDateColumn.DefaultCellStyle.Format = "d";
            this.expiryDateColumn.FillWeight = 75F;
            this.expiryDateColumn.HeaderText = "الصلاحية";
            this.expiryDateColumn.Name = "ExpiryDate";
            this.linesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.productNameColumn,
                this.unitNameColumn,
                this.quantityColumn,
                this.qtyBaseColumn,
                this.unitPriceColumn,
                this.lineTotalColumn,
                this.batchNoColumn,
                this.expiryDateColumn
            });
            this.linesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top |
                                    System.Windows.Forms.AnchorStyles.Bottom |
                                    System.Windows.Forms.AnchorStyles.Left |
                                    System.Windows.Forms.AnchorStyles.Right;
            this.linesGrid.Location = new System.Drawing.Point(20, 60);
            this.linesGrid.Name = "linesGrid";
            this.linesGrid.ReadOnly = true;
            this.linesGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.linesGrid.RowHeadersVisible = false;
            this.linesGrid.Size = new System.Drawing.Size(840, 360);
            //
            // closeButton
            //
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.closeButton.Location = new System.Drawing.Point(390, 430);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(95, 32);
            this.closeButton.Text = "إغلاق";
            this.closeButton.UseVisualStyleBackColor = true;
            //
            // PurchaseInvoiceDetailsForm
            //
            this.ClientSize = new System.Drawing.Size(880, 480);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.linesGrid);
            this.Controls.Add(this.closeButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(720, 500);
            this.Name = "PurchaseInvoiceDetailsForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "تفاصيل فاتورة";
            ((System.ComponentModel.ISupportInitialize)(this.linesGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
