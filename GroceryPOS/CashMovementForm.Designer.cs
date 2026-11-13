namespace GroceryPOS
{
    public partial class CashMovementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label currencyLabel;
        private System.Windows.Forms.ComboBox currencyComboBox;
        private System.Windows.Forms.RadioButton cashInRadio;
        private System.Windows.Forms.RadioButton cashOutRadio;
        private System.Windows.Forms.Label amountLabel;
        private System.Windows.Forms.NumericUpDown amountInput;
        private System.Windows.Forms.Label reasonLabel;
        private System.Windows.Forms.TextBox reasonInput;
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
            this.currencyLabel = new System.Windows.Forms.Label();
            this.currencyComboBox = new System.Windows.Forms.ComboBox();
            this.cashInRadio = new System.Windows.Forms.RadioButton();
            this.cashOutRadio = new System.Windows.Forms.RadioButton();
            this.amountLabel = new System.Windows.Forms.Label();
            this.amountInput = new System.Windows.Forms.NumericUpDown();
            this.reasonLabel = new System.Windows.Forms.Label();
            this.reasonInput = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.amountInput)).BeginInit();
            this.SuspendLayout();
            //
            // currencyLabel
            //
            this.currencyLabel.AutoSize = true;
            this.currencyLabel.Location = new System.Drawing.Point(390, 34);
            this.currencyLabel.Text = "العملة";
            //
            // currencyComboBox
            //
            this.currencyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.currencyComboBox.Location = new System.Drawing.Point(40, 30);
            this.currencyComboBox.Size = new System.Drawing.Size(330, 25);
            //
            // cashInRadio
            //
            this.cashInRadio.AutoSize = true;
            this.cashInRadio.Checked = true;
            this.cashInRadio.Location = new System.Drawing.Point(355, 85);
            this.cashInRadio.Text = "إيداع";
            //
            // cashOutRadio
            //
            this.cashOutRadio.AutoSize = true;
            this.cashOutRadio.Location = new System.Drawing.Point(245, 85);
            this.cashOutRadio.Text = "سحب";
            //
            // amountLabel
            //
            this.amountLabel.AutoSize = true;
            this.amountLabel.Location = new System.Drawing.Point(390, 135);
            this.amountLabel.Text = "المبلغ";
            //
            // amountInput
            //
            this.amountInput.DecimalPlaces = 4;
            this.amountInput.Increment = 0.0001M;
            this.amountInput.Location = new System.Drawing.Point(40, 130);
            this.amountInput.Maximum = 99999999999999.9999M;
            this.amountInput.Size = new System.Drawing.Size(330, 25);
            this.amountInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // reasonLabel
            //
            this.reasonLabel.AutoSize = true;
            this.reasonLabel.Location = new System.Drawing.Point(390, 185);
            this.reasonLabel.Text = "السبب";
            //
            // reasonInput
            //
            this.reasonInput.Location = new System.Drawing.Point(40, 180);
            this.reasonInput.MaxLength = 300;
            this.reasonInput.Size = new System.Drawing.Size(330, 25);
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(280, 255);
            this.saveButton.Size = new System.Drawing.Size(90, 34);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(170, 255);
            this.cancelButton.Size = new System.Drawing.Size(90, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // CashMovementForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(500, 330);
            this.Controls.Add(this.currencyLabel);
            this.Controls.Add(this.currencyComboBox);
            this.Controls.Add(this.cashInRadio);
            this.Controls.Add(this.cashOutRadio);
            this.Controls.Add(this.amountLabel);
            this.Controls.Add(this.amountInput);
            this.Controls.Add(this.reasonLabel);
            this.Controls.Add(this.reasonInput);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CashMovementForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "حركة صندوق";
            ((System.ComponentModel.ISupportInitialize)(this.amountInput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
