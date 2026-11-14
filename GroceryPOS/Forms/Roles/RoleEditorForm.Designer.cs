namespace GroceryPOS
{
    public partial class RoleEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Label permissionsLabel;
        private System.Windows.Forms.CheckedListBox permissionsListBox;
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
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.permissionsLabel = new System.Windows.Forms.Label();
            this.permissionsListBox = new System.Windows.Forms.CheckedListBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // nameLabel
            //
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(490, 28);
            this.nameLabel.Text = "اسم الدور";
            //
            // nameTextBox
            //
            this.nameTextBox.Location = new System.Drawing.Point(35, 25);
            this.nameTextBox.MaxLength = 100;
            this.nameTextBox.Size = new System.Drawing.Size(420, 25);
            //
            // descriptionLabel
            //
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(520, 78);
            this.descriptionLabel.Text = "الوصف";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(35, 75);
            this.descriptionTextBox.MaxLength = 300;
            this.descriptionTextBox.Size = new System.Drawing.Size(420, 25);
            //
            // permissionsLabel
            //
            this.permissionsLabel.AutoSize = true;
            this.permissionsLabel.Location = new System.Drawing.Point(430, 125);
            this.permissionsLabel.Text = "الصلاحيات الممنوحة";
            //
            // permissionsListBox
            //
            this.permissionsListBox.CheckOnClick = true;
            this.permissionsListBox.HorizontalScrollbar = true;
            this.permissionsListBox.IntegralHeight = false;
            this.permissionsListBox.Location = new System.Drawing.Point(35, 150);
            this.permissionsListBox.Size = new System.Drawing.Size(520, 420);
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(355, 590);
            this.saveButton.Size = new System.Drawing.Size(90, 34);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(245, 590);
            this.cancelButton.Size = new System.Drawing.Size(90, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // RoleEditorForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(600, 650);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(this.permissionsLabel);
            this.Controls.Add(this.permissionsListBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 650);
            this.MinimizeBox = false;
            this.Name = "RoleEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إضافة دور";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
