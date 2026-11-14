namespace GroceryPOS
{
    public partial class UserEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label fullNameLabel;
        private System.Windows.Forms.TextBox fullNameTextBox;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label confirmPasswordLabel;
        private System.Windows.Forms.TextBox confirmPasswordTextBox;
        private System.Windows.Forms.Label rolesLabel;
        private System.Windows.Forms.CheckedListBox rolesListBox;
        private System.Windows.Forms.Label firstLoginNoteLabel;
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
            this.fullNameLabel = new System.Windows.Forms.Label();
            this.fullNameTextBox = new System.Windows.Forms.TextBox();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.confirmPasswordLabel = new System.Windows.Forms.Label();
            this.confirmPasswordTextBox = new System.Windows.Forms.TextBox();
            this.rolesLabel = new System.Windows.Forms.Label();
            this.rolesListBox = new System.Windows.Forms.CheckedListBox();
            this.firstLoginNoteLabel = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // fullNameLabel
            //
            this.fullNameLabel.AutoSize = true;
            this.fullNameLabel.Location = new System.Drawing.Point(380, 28);
            this.fullNameLabel.Text = "الاسم الكامل";
            //
            // fullNameTextBox
            //
            this.fullNameTextBox.Location = new System.Drawing.Point(150, 25);
            this.fullNameTextBox.MaxLength = 150;
            this.fullNameTextBox.Size = new System.Drawing.Size(300, 25);
            //
            // usernameLabel
            //
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(380, 78);
            this.usernameLabel.Text = "اسم المستخدم";
            //
            // usernameTextBox
            //
            this.usernameTextBox.Location = new System.Drawing.Point(150, 75);
            this.usernameTextBox.MaxLength = 50;
            this.usernameTextBox.Size = new System.Drawing.Size(300, 25);
            //
            // passwordLabel
            //
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(380, 128);
            this.passwordLabel.Text = "كلمة المرور المؤقتة (12 حرفًا على الأقل)";
            //
            // passwordTextBox
            //
            this.passwordTextBox.Location = new System.Drawing.Point(150, 125);
            this.passwordTextBox.MaxLength = 128;
            this.passwordTextBox.Size = new System.Drawing.Size(300, 25);
            this.passwordTextBox.UseSystemPasswordChar = true;
            //
            // confirmPasswordLabel
            //
            this.confirmPasswordLabel.AutoSize = true;
            this.confirmPasswordLabel.Location = new System.Drawing.Point(380, 178);
            this.confirmPasswordLabel.Text = "تأكيد كلمة المرور";
            //
            // confirmPasswordTextBox
            //
            this.confirmPasswordTextBox.Location = new System.Drawing.Point(150, 175);
            this.confirmPasswordTextBox.MaxLength = 128;
            this.confirmPasswordTextBox.Size = new System.Drawing.Size(300, 25);
            this.confirmPasswordTextBox.UseSystemPasswordChar = true;
            //
            // rolesLabel
            //
            this.rolesLabel.AutoSize = true;
            this.rolesLabel.Location = new System.Drawing.Point(410, 232);
            this.rolesLabel.Text = "الأدوار";
            //
            // rolesListBox
            //
            this.rolesListBox.CheckOnClick = true;
            this.rolesListBox.IntegralHeight = false;
            this.rolesListBox.Location = new System.Drawing.Point(35, 255);
            this.rolesListBox.Size = new System.Drawing.Size(400, 230);
            //
            // firstLoginNoteLabel
            //
            this.firstLoginNoteLabel.AutoSize = true;
            this.firstLoginNoteLabel.ForeColor = System.Drawing.Color.DimGray;
            this.firstLoginNoteLabel.Location = new System.Drawing.Point(95, 490);
            this.firstLoginNoteLabel.Text = "يُطلب من المستخدم تغيير كلمة المرور المؤقتة عند أول دخول.";
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(275, 525);
            this.saveButton.Size = new System.Drawing.Size(90, 32);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(165, 525);
            this.cancelButton.Size = new System.Drawing.Size(90, 32);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // UserEditorForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(500, 585);
            this.Controls.Add(this.fullNameLabel);
            this.Controls.Add(this.fullNameTextBox);
            this.Controls.Add(this.usernameLabel);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.confirmPasswordLabel);
            this.Controls.Add(this.confirmPasswordTextBox);
            this.Controls.Add(this.rolesLabel);
            this.Controls.Add(this.rolesListBox);
            this.Controls.Add(this.firstLoginNoteLabel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إضافة مستخدم";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
