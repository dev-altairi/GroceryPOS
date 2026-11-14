namespace GroceryPOS
{
    public partial class ProductEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label skuLabel;
        private System.Windows.Forms.TextBox skuTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label categoryLabel;
        private System.Windows.Forms.ComboBox categoryComboBox;
        private System.Windows.Forms.Label unitLabel;
        private System.Windows.Forms.ComboBox unitComboBox;
        private System.Windows.Forms.Label barcodeLabel;
        private System.Windows.Forms.TextBox barcodeTextBox;
        private System.Windows.Forms.Label retailPriceLabel;
        private System.Windows.Forms.TextBox retailPriceTextBox;
        private System.Windows.Forms.Label wholesalePriceLabel;
        private System.Windows.Forms.TextBox wholesalePriceTextBox;
        private System.Windows.Forms.Label minimumStockLabel;
        private System.Windows.Forms.TextBox minimumStockTextBox;
        private System.Windows.Forms.CheckBox tracksExpiryCheckBox;
        private System.Windows.Forms.CheckBox activeCheckBox;
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
            this.skuLabel = new System.Windows.Forms.Label();
            this.skuTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.categoryLabel = new System.Windows.Forms.Label();
            this.categoryComboBox = new System.Windows.Forms.ComboBox();
            this.unitLabel = new System.Windows.Forms.Label();
            this.unitComboBox = new System.Windows.Forms.ComboBox();
            this.barcodeLabel = new System.Windows.Forms.Label();
            this.barcodeTextBox = new System.Windows.Forms.TextBox();
            this.retailPriceLabel = new System.Windows.Forms.Label();
            this.retailPriceTextBox = new System.Windows.Forms.TextBox();
            this.wholesalePriceLabel = new System.Windows.Forms.Label();
            this.wholesalePriceTextBox = new System.Windows.Forms.TextBox();
            this.minimumStockLabel = new System.Windows.Forms.Label();
            this.minimumStockTextBox = new System.Windows.Forms.TextBox();
            this.tracksExpiryCheckBox = new System.Windows.Forms.CheckBox();
            this.activeCheckBox = new System.Windows.Forms.CheckBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // skuLabel
            //
            this.skuLabel.AutoSize = true;
            this.skuLabel.Location = new System.Drawing.Point(405, 30);
            this.skuLabel.Name = "skuLabel";
            this.skuLabel.Size = new System.Drawing.Size(143, 19);
            this.skuLabel.TabIndex = 0;
            this.skuLabel.Text = "رمز SKU (اختياري)";
            //
            // skuTextBox
            //
            this.skuTextBox.Location = new System.Drawing.Point(75, 25);
            this.skuTextBox.MaxLength = 50;
            this.skuTextBox.Name = "skuTextBox";
            this.skuTextBox.Size = new System.Drawing.Size(310, 25);
            this.skuTextBox.TabIndex = 1;
            //
            // nameLabel
            //
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(405, 80);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(77, 19);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Text = "اسم المنتج";
            //
            // nameTextBox
            //
            this.nameTextBox.Location = new System.Drawing.Point(75, 75);
            this.nameTextBox.MaxLength = 200;
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(310, 25);
            this.nameTextBox.TabIndex = 3;
            //
            // categoryLabel
            //
            this.categoryLabel.AutoSize = true;
            this.categoryLabel.Location = new System.Drawing.Point(405, 130);
            this.categoryLabel.Name = "categoryLabel";
            this.categoryLabel.Size = new System.Drawing.Size(66, 19);
            this.categoryLabel.TabIndex = 4;
            this.categoryLabel.Text = "التصنيف";
            //
            // categoryComboBox
            //
            this.categoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryComboBox.FormattingEnabled = true;
            this.categoryComboBox.Location = new System.Drawing.Point(75, 125);
            this.categoryComboBox.Name = "categoryComboBox";
            this.categoryComboBox.Size = new System.Drawing.Size(310, 25);
            this.categoryComboBox.TabIndex = 5;
            this.categoryComboBox.DisplayMember = "Name";
            this.categoryComboBox.ValueMember = "Id";
            //
            // unitLabel
            //
            this.unitLabel.AutoSize = true;
            this.unitLabel.Location = new System.Drawing.Point(405, 180);
            this.unitLabel.Name = "unitLabel";
            this.unitLabel.Size = new System.Drawing.Size(92, 19);
            this.unitLabel.TabIndex = 6;
            this.unitLabel.Text = "الوحدة الأساسية";
            //
            // unitComboBox
            //
            this.unitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.unitComboBox.FormattingEnabled = true;
            this.unitComboBox.Location = new System.Drawing.Point(75, 175);
            this.unitComboBox.Name = "unitComboBox";
            this.unitComboBox.Size = new System.Drawing.Size(310, 25);
            this.unitComboBox.TabIndex = 7;
            this.unitComboBox.DisplayMember = "Name";
            this.unitComboBox.ValueMember = "Id";
            //
            // barcodeLabel
            //
            this.barcodeLabel.AutoSize = true;
            this.barcodeLabel.Location = new System.Drawing.Point(405, 230);
            this.barcodeLabel.Name = "barcodeLabel";
            this.barcodeLabel.Size = new System.Drawing.Size(119, 19);
            this.barcodeLabel.TabIndex = 8;
            this.barcodeLabel.Text = "الباركود (اختياري)";
            //
            // barcodeTextBox
            //
            this.barcodeTextBox.Location = new System.Drawing.Point(75, 225);
            this.barcodeTextBox.MaxLength = 50;
            this.barcodeTextBox.Name = "barcodeTextBox";
            this.barcodeTextBox.Size = new System.Drawing.Size(310, 25);
            this.barcodeTextBox.TabIndex = 9;
            //
            // retailPriceLabel
            //
            this.retailPriceLabel.AutoSize = true;
            this.retailPriceLabel.Location = new System.Drawing.Point(405, 280);
            this.retailPriceLabel.Name = "retailPriceLabel";
            this.retailPriceLabel.Size = new System.Drawing.Size(158, 19);
            this.retailPriceLabel.TabIndex = 10;
            this.retailPriceLabel.Text = "سعر البيع بالعملة الأساسية";
            //
            // retailPriceTextBox
            //
            this.retailPriceTextBox.Location = new System.Drawing.Point(75, 275);
            this.retailPriceTextBox.MaxLength = 40;
            this.retailPriceTextBox.Name = "retailPriceTextBox";
            this.retailPriceTextBox.Size = new System.Drawing.Size(310, 25);
            this.retailPriceTextBox.TabIndex = 11;
            this.retailPriceTextBox.Text = "0";
            //
            // wholesalePriceLabel
            //
            this.wholesalePriceLabel.AutoSize = true;
            this.wholesalePriceLabel.Location = new System.Drawing.Point(405, 330);
            this.wholesalePriceLabel.Name = "wholesalePriceLabel";
            this.wholesalePriceLabel.Size = new System.Drawing.Size(165, 19);
            this.wholesalePriceLabel.TabIndex = 12;
            this.wholesalePriceLabel.Text = "سعر الجملة بالعملة الأساسية";
            //
            // wholesalePriceTextBox
            //
            this.wholesalePriceTextBox.Location = new System.Drawing.Point(75, 325);
            this.wholesalePriceTextBox.MaxLength = 40;
            this.wholesalePriceTextBox.Name = "wholesalePriceTextBox";
            this.wholesalePriceTextBox.Size = new System.Drawing.Size(310, 25);
            this.wholesalePriceTextBox.TabIndex = 13;
            //
            // minimumStockLabel
            //
            this.minimumStockLabel.AutoSize = true;
            this.minimumStockLabel.Location = new System.Drawing.Point(405, 380);
            this.minimumStockLabel.Name = "minimumStockLabel";
            this.minimumStockLabel.Size = new System.Drawing.Size(113, 19);
            this.minimumStockLabel.TabIndex = 14;
            this.minimumStockLabel.Text = "الحد الأدنى للمخزون";
            //
            // minimumStockTextBox
            //
            this.minimumStockTextBox.Location = new System.Drawing.Point(75, 375);
            this.minimumStockTextBox.MaxLength = 40;
            this.minimumStockTextBox.Name = "minimumStockTextBox";
            this.minimumStockTextBox.Size = new System.Drawing.Size(310, 25);
            this.minimumStockTextBox.TabIndex = 15;
            this.minimumStockTextBox.Text = "0";
            //
            // tracksExpiryCheckBox
            //
            this.tracksExpiryCheckBox.AutoSize = true;
            this.tracksExpiryCheckBox.Location = new System.Drawing.Point(250, 425);
            this.tracksExpiryCheckBox.Name = "tracksExpiryCheckBox";
            this.tracksExpiryCheckBox.Size = new System.Drawing.Size(189, 23);
            this.tracksExpiryCheckBox.TabIndex = 16;
            this.tracksExpiryCheckBox.Text = "المنتج يتطلب تتبع تاريخ الصلاحية";
            this.tracksExpiryCheckBox.UseVisualStyleBackColor = true;
            //
            // activeCheckBox
            //
            this.activeCheckBox.AutoSize = true;
            this.activeCheckBox.Checked = true;
            this.activeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.activeCheckBox.Location = new System.Drawing.Point(110, 425);
            this.activeCheckBox.Name = "activeCheckBox";
            this.activeCheckBox.Size = new System.Drawing.Size(89, 23);
            this.activeCheckBox.TabIndex = 17;
            this.activeCheckBox.Text = "المنتج نشط";
            this.activeCheckBox.UseVisualStyleBackColor = true;
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(330, 470);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(105, 36);
            this.saveButton.TabIndex = 18;
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(205, 470);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(105, 36);
            this.cancelButton.TabIndex = 19;
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // ProductEditorForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(570, 525);
            this.Controls.Add(this.skuLabel);
            this.Controls.Add(this.skuTextBox);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.categoryLabel);
            this.Controls.Add(this.categoryComboBox);
            this.Controls.Add(this.unitLabel);
            this.Controls.Add(this.unitComboBox);
            this.Controls.Add(this.barcodeLabel);
            this.Controls.Add(this.barcodeTextBox);
            this.Controls.Add(this.retailPriceLabel);
            this.Controls.Add(this.retailPriceTextBox);
            this.Controls.Add(this.wholesalePriceLabel);
            this.Controls.Add(this.wholesalePriceTextBox);
            this.Controls.Add(this.minimumStockLabel);
            this.Controls.Add(this.minimumStockTextBox);
            this.Controls.Add(this.tracksExpiryCheckBox);
            this.Controls.Add(this.activeCheckBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProductEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إضافة منتج";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
