namespace GroceryPOS
{
    public partial class SalePaymentForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label requiredLabel;
        private System.Windows.Forms.Label helpLabel;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label changeCurrencyLabel;
        private System.Windows.Forms.ComboBox changeCurrencyComboBox;
        private System.Windows.Forms.Label paidLabel;
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
            this.requiredLabel = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.changeCurrencyLabel = new System.Windows.Forms.Label();
            this.changeCurrencyComboBox = new System.Windows.Forms.ComboBox();
            this.paidLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // requiredLabel
            //
            this.requiredLabel.AutoSize = true;
            this.requiredLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.requiredLabel.Location = new System.Drawing.Point(400, 20);
            this.requiredLabel.Text = "المطلوب";
            //
            // helpLabel
            //
            this.helpLabel.AutoSize = true;
            this.helpLabel.Location = new System.Drawing.Point(25, 65);
            this.helpLabel.Text = "أدخل المبلغ المستلم في كل عملة. تُحوّل بقية العملات بسعر الصرف المسجل لهذا اليوم.";
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoGenerateColumns = true;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.Anchor = System.Windows.Forms.AnchorStyles.Top |
                               System.Windows.Forms.AnchorStyles.Bottom |
                               System.Windows.Forms.AnchorStyles.Left |
                               System.Windows.Forms.AnchorStyles.Right;
            this.grid.Location = new System.Drawing.Point(20, 100);
            this.grid.Name = "grid";
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.Size = new System.Drawing.Size(680, 240);
            //
            // changeCurrencyLabel
            //
            this.changeCurrencyLabel.AutoSize = true;
            this.changeCurrencyLabel.Location = new System.Drawing.Point(595, 365);
            this.changeCurrencyLabel.Text = "عملة الباقي";
            //
            // changeCurrencyComboBox
            //
            this.changeCurrencyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.changeCurrencyComboBox.Location = new System.Drawing.Point(285, 360);
            this.changeCurrencyComboBox.Size = new System.Drawing.Size(290, 25);
            //
            // paidLabel
            //
            this.paidLabel.AutoSize = true;
            this.paidLabel.Location = new System.Drawing.Point(25, 365);
            this.paidLabel.Text = "المجموع بالعملة الأساسية: 0";
            //
            // saveButton
            //
            this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                     System.Windows.Forms.AnchorStyles.Right;
            this.saveButton.Location = new System.Drawing.Point(385, 420);
            this.saveButton.Size = new System.Drawing.Size(115, 34);
            this.saveButton.Text = "تأكيد الدفع";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                       System.Windows.Forms.AnchorStyles.Right;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(260, 420);
            this.cancelButton.Size = new System.Drawing.Size(100, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // SalePaymentForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(720, 490);
            this.Controls.Add(this.requiredLabel);
            this.Controls.Add(this.helpLabel);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.changeCurrencyLabel);
            this.Controls.Add(this.changeCurrencyComboBox);
            this.Controls.Add(this.paidLabel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(640, 430);
            this.Name = "SalePaymentForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "الدفع";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
