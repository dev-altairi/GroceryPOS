namespace GroceryPOS
{
    public partial class PasswordManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Label currentPasswordLabel;
        private System.Windows.Forms.TextBox currentPasswordTextBox;
        private System.Windows.Forms.Label newPasswordLabel;
        private System.Windows.Forms.TextBox newPasswordTextBox;
        private System.Windows.Forms.Label confirmPasswordLabel;
        private System.Windows.Forms.TextBox confirmPasswordTextBox;
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
            this.instructionLabel = new System.Windows.Forms.Label();
            this.currentPasswordLabel = new System.Windows.Forms.Label();
            this.currentPasswordTextBox = new System.Windows.Forms.TextBox();
            this.newPasswordLabel = new System.Windows.Forms.Label();
            this.newPasswordTextBox = new System.Windows.Forms.TextBox();
            this.confirmPasswordLabel = new System.Windows.Forms.Label();
            this.confirmPasswordTextBox = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // instructionLabel
            //
            this.instructionLabel.Location = new System.Drawing.Point(35, 18);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(450, 42);
            this.instructionLabel.TabIndex = 0;
            this.instructionLabel.Text = "أدخل كلمة المرور الحالية ثم كلمة المرور الجديدة.";
            //
            // currentPasswordLabel
            //
            this.currentPasswordLabel.AutoSize = true;
            this.currentPasswordLabel.Location = new System.Drawing.Point(370, 74);
            this.currentPasswordLabel.Name = "currentPasswordLabel";
            this.currentPasswordLabel.Size = new System.Drawing.Size(106, 19);
            this.currentPasswordLabel.TabIndex = 1;
            this.currentPasswordLabel.Text = "كلمة المرور الحالية";
            //
            // currentPasswordTextBox
            //
            this.currentPasswordTextBox.Location = new System.Drawing.Point(35, 70);
            this.currentPasswordTextBox.MaxLength = 128;
            this.currentPasswordTextBox.Name = "currentPasswordTextBox";
            this.currentPasswordTextBox.Size = new System.Drawing.Size(310, 25);
            this.currentPasswordTextBox.TabIndex = 2;
            this.currentPasswordTextBox.UseSystemPasswordChar = true;
            //
            // newPasswordLabel
            //
            this.newPasswordLabel.AutoSize = true;
            this.newPasswordLabel.Location = new System.Drawing.Point(235, 129);
            this.newPasswordLabel.Name = "newPasswordLabel";
            this.newPasswordLabel.Size = new System.Drawing.Size(241, 19);
            this.newPasswordLabel.TabIndex = 3;
            this.newPasswordLabel.Text = "كلمة المرور الجديدة (12 حرفًا على الأقل)";
            //
            // newPasswordTextBox
            //
            this.newPasswordTextBox.Location = new System.Drawing.Point(35, 125);
            this.newPasswordTextBox.MaxLength = 128;
            this.newPasswordTextBox.Name = "newPasswordTextBox";
            this.newPasswordTextBox.Size = new System.Drawing.Size(310, 25);
            this.newPasswordTextBox.TabIndex = 4;
            this.newPasswordTextBox.UseSystemPasswordChar = true;
            //
            // confirmPasswordLabel
            //
            this.confirmPasswordLabel.AutoSize = true;
            this.confirmPasswordLabel.Location = new System.Drawing.Point(370, 184);
            this.confirmPasswordLabel.Name = "confirmPasswordLabel";
            this.confirmPasswordLabel.Size = new System.Drawing.Size(104, 19);
            this.confirmPasswordLabel.TabIndex = 5;
            this.confirmPasswordLabel.Text = "تأكيد كلمة المرور";
            //
            // confirmPasswordTextBox
            //
            this.confirmPasswordTextBox.Location = new System.Drawing.Point(35, 180);
            this.confirmPasswordTextBox.MaxLength = 128;
            this.confirmPasswordTextBox.Name = "confirmPasswordTextBox";
            this.confirmPasswordTextBox.Size = new System.Drawing.Size(310, 25);
            this.confirmPasswordTextBox.TabIndex = 6;
            this.confirmPasswordTextBox.UseSystemPasswordChar = true;
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(295, 250);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(130, 36);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(180, 250);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(95, 36);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // PasswordManagementForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(520, 390);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.confirmPasswordTextBox);
            this.Controls.Add(this.confirmPasswordLabel);
            this.Controls.Add(this.newPasswordTextBox);
            this.Controls.Add(this.newPasswordLabel);
            this.Controls.Add(this.currentPasswordTextBox);
            this.Controls.Add(this.currentPasswordLabel);
            this.Controls.Add(this.instructionLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasswordManagementForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "تغيير كلمة المرور";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
