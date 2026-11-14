namespace GroceryPOS
{
    public partial class PurchaseInvoiceEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label supplierLabel;
        private System.Windows.Forms.ComboBox supplierComboBox;
        private System.Windows.Forms.Label currencyLabel;
        private System.Windows.Forms.ComboBox currencyComboBox;
        private System.Windows.Forms.Label invoiceDateLabel;
        private System.Windows.Forms.DateTimePicker invoiceDatePicker;
        private System.Windows.Forms.Label supplierInvoiceNoLabel;
        private System.Windows.Forms.TextBox supplierInvoiceNoTextBox;
        private System.Windows.Forms.Label notesLabel;
        private System.Windows.Forms.TextBox notesTextBox;
        private System.Windows.Forms.DataGridView linesGrid;
        private System.Windows.Forms.DataGridViewComboBoxColumn productUnitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitPriceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn batchNoColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn expiryDateColumn;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;

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
            this.supplierLabel = new System.Windows.Forms.Label();
            this.supplierComboBox = new System.Windows.Forms.ComboBox();
            this.currencyLabel = new System.Windows.Forms.Label();
            this.currencyComboBox = new System.Windows.Forms.ComboBox();
            this.invoiceDateLabel = new System.Windows.Forms.Label();
            this.invoiceDatePicker = new System.Windows.Forms.DateTimePicker();
            this.supplierInvoiceNoLabel = new System.Windows.Forms.Label();
            this.supplierInvoiceNoTextBox = new System.Windows.Forms.TextBox();
            this.notesLabel = new System.Windows.Forms.Label();
            this.notesTextBox = new System.Windows.Forms.TextBox();
            this.linesGrid = new System.Windows.Forms.DataGridView();
            this.productUnitColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.quantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitPriceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.batchNoColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.expiryDateColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.helpLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.linesGrid)).BeginInit();
            this.SuspendLayout();
            //
            // supplierLabel
            //
            this.supplierLabel.AutoSize = true;
            this.supplierLabel.Location = new System.Drawing.Point(895, 24);
            this.supplierLabel.Name = "supplierLabel";
            this.supplierLabel.Text = "المورد";
            //
            // supplierComboBox
            //
            this.supplierComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.supplierComboBox.Location = new System.Drawing.Point(610, 20);
            this.supplierComboBox.Name = "supplierComboBox";
            this.supplierComboBox.Size = new System.Drawing.Size(260, 25);
            //
            // currencyLabel
            //
            this.currencyLabel.AutoSize = true;
            this.currencyLabel.Location = new System.Drawing.Point(895, 79);
            this.currencyLabel.Name = "currencyLabel";
            this.currencyLabel.Text = "العملة";
            //
            // currencyComboBox
            //
            this.currencyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.currencyComboBox.Location = new System.Drawing.Point(610, 75);
            this.currencyComboBox.Name = "currencyComboBox";
            this.currencyComboBox.Size = new System.Drawing.Size(260, 25);
            //
            // invoiceDateLabel
            //
            this.invoiceDateLabel.AutoSize = true;
            this.invoiceDateLabel.Location = new System.Drawing.Point(890, 130);
            this.invoiceDateLabel.Name = "invoiceDateLabel";
            this.invoiceDateLabel.Text = "تاريخ الفاتورة";
            //
            // invoiceDatePicker
            //
            this.invoiceDatePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.invoiceDatePicker.Location = new System.Drawing.Point(650, 125);
            this.invoiceDatePicker.MaxDate = System.DateTime.Today;
            this.invoiceDatePicker.Name = "invoiceDatePicker";
            this.invoiceDatePicker.Size = new System.Drawing.Size(220, 25);
            this.invoiceDatePicker.Value = System.DateTime.Today;
            //
            // supplierInvoiceNoLabel
            //
            this.supplierInvoiceNoLabel.AutoSize = true;
            this.supplierInvoiceNoLabel.Location = new System.Drawing.Point(570, 130);
            this.supplierInvoiceNoLabel.Name = "supplierInvoiceNoLabel";
            this.supplierInvoiceNoLabel.Text = "رقم فاتورة المورد";
            //
            // supplierInvoiceNoTextBox
            //
            this.supplierInvoiceNoTextBox.Location = new System.Drawing.Point(350, 125);
            this.supplierInvoiceNoTextBox.MaxLength = 50;
            this.supplierInvoiceNoTextBox.Name = "supplierInvoiceNoTextBox";
            this.supplierInvoiceNoTextBox.Size = new System.Drawing.Size(195, 25);
            //
            // notesLabel
            //
            this.notesLabel.AutoSize = true;
            this.notesLabel.Location = new System.Drawing.Point(280, 130);
            this.notesLabel.Name = "notesLabel";
            this.notesLabel.Text = "ملاحظات";
            //
            // notesTextBox
            //
            this.notesTextBox.Location = new System.Drawing.Point(35, 125);
            this.notesTextBox.MaxLength = 500;
            this.notesTextBox.Name = "notesTextBox";
            this.notesTextBox.Size = new System.Drawing.Size(225, 25);
            //
            // linesGrid
            //
            this.linesGrid.AllowUserToAddRows = true;
            this.linesGrid.AllowUserToDeleteRows = true;
            this.linesGrid.AutoGenerateColumns = false;
            this.linesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.linesGrid.BackgroundColor = System.Drawing.Color.White;
            this.linesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.productUnitColumn,
                this.quantityColumn,
                this.unitPriceColumn,
                this.batchNoColumn,
                this.expiryDateColumn
            });
            this.linesGrid.Location = new System.Drawing.Point(25, 175);
            this.linesGrid.MultiSelect = false;
            this.linesGrid.Name = "linesGrid";
            this.linesGrid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.linesGrid.RowHeadersVisible = false;
            this.linesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.linesGrid.Size = new System.Drawing.Size(990, 420);
            //
            // productUnitColumn
            //
            this.productUnitColumn.DataPropertyName = "ProductUnitId";
            this.productUnitColumn.DisplayMember = "DisplayName";
            this.productUnitColumn.FillWeight = 185F;
            this.productUnitColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.productUnitColumn.HeaderText = "المنتج والوحدة";
            this.productUnitColumn.Name = "ProductUnitId";
            this.productUnitColumn.ValueMember = "Id";
            //
            // quantityColumn
            //
            this.quantityColumn.DataPropertyName = "Quantity";
            this.quantityColumn.DefaultCellStyle.Format = "N3";
            this.quantityColumn.FillWeight = 65F;
            this.quantityColumn.HeaderText = "الكمية";
            this.quantityColumn.Name = "Quantity";
            //
            // unitPriceColumn
            //
            this.unitPriceColumn.DataPropertyName = "UnitPrice";
            this.unitPriceColumn.DefaultCellStyle.Format = "N4";
            this.unitPriceColumn.FillWeight = 75F;
            this.unitPriceColumn.HeaderText = "سعر الوحدة";
            this.unitPriceColumn.Name = "UnitPrice";
            //
            // batchNoColumn
            //
            this.batchNoColumn.DataPropertyName = "BatchNo";
            this.batchNoColumn.FillWeight = 80F;
            this.batchNoColumn.HeaderText = "رقم الدفعة";
            this.batchNoColumn.Name = "BatchNo";
            //
            // expiryDateColumn
            //
            this.expiryDateColumn.DataPropertyName = "ExpiryDate";
            this.expiryDateColumn.DefaultCellStyle.Format = "yyyy-MM-dd";
            this.expiryDateColumn.FillWeight = 90F;
            this.expiryDateColumn.HeaderText = "الصلاحية (YYYY-MM-DD)";
            this.expiryDateColumn.Name = "ExpiryDate";
            //
            // helpLabel
            //
            this.helpLabel.AutoSize = true;
            this.helpLabel.ForeColor = System.Drawing.Color.DimGray;
            this.helpLabel.Location = new System.Drawing.Point(470, 610);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Text = "أدخل سعر الوحدة بعملة الفاتورة. الصلاحية إلزامية للمنتجات التي تتطلب التتبع.";
            //
            // saveButton
            //
            this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                     System.Windows.Forms.AnchorStyles.Right;
            this.saveButton.Location = new System.Drawing.Point(270, 635);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(130, 34);
            this.saveButton.Text = "استلام وحفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                       System.Windows.Forms.AnchorStyles.Right;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(160, 635);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(90, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // PurchaseInvoiceEditorForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1040, 680);
            this.Controls.Add(this.supplierLabel);
            this.Controls.Add(this.supplierComboBox);
            this.Controls.Add(this.currencyLabel);
            this.Controls.Add(this.currencyComboBox);
            this.Controls.Add(this.invoiceDateLabel);
            this.Controls.Add(this.invoiceDatePicker);
            this.Controls.Add(this.supplierInvoiceNoLabel);
            this.Controls.Add(this.supplierInvoiceNoTextBox);
            this.Controls.Add(this.notesLabel);
            this.Controls.Add(this.notesTextBox);
            this.Controls.Add(this.linesGrid);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(1060, 750);
            this.Name = "PurchaseInvoiceEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "استلام فاتورة شراء";
            ((System.ComponentModel.ISupportInitialize)(this.linesGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
