namespace GroceryPOS
{
    public partial class OwnerSetupForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Label fullNameLabel;
        private System.Windows.Forms.TextBox fullNameTextBox;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label confirmPasswordLabel;
        private System.Windows.Forms.TextBox confirmPasswordTextBox;
        private System.Windows.Forms.Button createButton;

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
            this.titleLabel = new System.Windows.Forms.Label();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.fullNameLabel = new System.Windows.Forms.Label();
            this.fullNameTextBox = new System.Windows.Forms.TextBox();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.confirmPasswordLabel = new System.Windows.Forms.Label();
            this.confirmPasswordTextBox = new System.Windows.Forms.TextBox();
            this.createButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // titleLabel
            //
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 17F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(135, 20);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(200, 31);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "إنشاء حساب المالك";
            //
            // instructionLabel
            //
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(67, 65);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(336, 19);
            this.instructionLabel.TabIndex = 1;
            this.instructionLabel.Text = "أنشئ الحساب الأول. كلمة المرور لا تقل عن 12 حرفًا.";
            //
            // fullNameLabel
            //
            this.fullNameLabel.AutoSize = true;
            this.fullNameLabel.Location = new System.Drawing.Point(355, 114);
            this.fullNameLabel.Name = "fullNameLabel";
            this.fullNameLabel.Size = new System.Drawing.Size(73, 19);
            this.fullNameLabel.TabIndex = 2;
            this.fullNameLabel.Text = "الاسم الكامل";
            //
            // fullNameTextBox
            //
            this.fullNameTextBox.Location = new System.Drawing.Point(35, 110);
            this.fullNameTextBox.MaxLength = 150;
            this.fullNameTextBox.Name = "fullNameTextBox";
            this.fullNameTextBox.Size = new System.Drawing.Size(290, 25);
            this.fullNameTextBox.TabIndex = 3;
            //
            // usernameLabel
            //
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(355, 164);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(78, 19);
            this.usernameLabel.TabIndex = 4;
            this.usernameLabel.Text = "اسم المستخدم";
            //
            // usernameTextBox
            //
            this.usernameTextBox.Location = new System.Drawing.Point(35, 160);
            this.usernameTextBox.MaxLength = 50;
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(290, 25);
            this.usernameTextBox.TabIndex = 5;
            //
            // passwordLabel
            //
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(355, 214);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(70, 19);
            this.passwordLabel.TabIndex = 6;
            this.passwordLabel.Text = "كلمة المرور";
            //
            // passwordTextBox
            //
            this.passwordTextBox.Location = new System.Drawing.Point(35, 210);
            this.passwordTextBox.MaxLength = 128;
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(290, 25);
            this.passwordTextBox.TabIndex = 7;
            this.passwordTextBox.UseSystemPasswordChar = true;
            //
            // confirmPasswordLabel
            //
            this.confirmPasswordLabel.AutoSize = true;
            this.confirmPasswordLabel.Location = new System.Drawing.Point(355, 264);
            this.confirmPasswordLabel.Name = "confirmPasswordLabel";
            this.confirmPasswordLabel.Size = new System.Drawing.Size(104, 19);
            this.confirmPasswordLabel.TabIndex = 8;
            this.confirmPasswordLabel.Text = "تأكيد كلمة المرور";
            //
            // confirmPasswordTextBox
            //
            this.confirmPasswordTextBox.Location = new System.Drawing.Point(35, 260);
            this.confirmPasswordTextBox.MaxLength = 128;
            this.confirmPasswordTextBox.Name = "confirmPasswordTextBox";
            this.confirmPasswordTextBox.Size = new System.Drawing.Size(290, 25);
            this.confirmPasswordTextBox.TabIndex = 9;
            this.confirmPasswordTextBox.UseSystemPasswordChar = true;
            //
            // createButton
            //
            this.createButton.Location = new System.Drawing.Point(165, 320);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(150, 38);
            this.createButton.TabIndex = 10;
            this.createButton.Text = "إنشاء الحساب";
            this.createButton.UseVisualStyleBackColor = true;
            //
            // OwnerSetupForm
            //
            this.AcceptButton = this.createButton;
            this.ClientSize = new System.Drawing.Size(470, 390);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.confirmPasswordTextBox);
            this.Controls.Add(this.confirmPasswordLabel);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.usernameLabel);
            this.Controls.Add(this.fullNameTextBox);
            this.Controls.Add(this.fullNameLabel);
            this.Controls.Add(this.instructionLabel);
            this.Controls.Add(this.titleLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OwnerSetupForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "الإعداد الأولي - GroceryPOS";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
