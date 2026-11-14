namespace GroceryPOS
{
    public partial class CategoryEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label parentLabel;
        private System.Windows.Forms.ComboBox parentComboBox;
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
            this.parentLabel = new System.Windows.Forms.Label();
            this.parentComboBox = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // nameLabel
            //
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(345, 35);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Text = "اسم التصنيف";
            //
            // nameTextBox
            //
            this.nameTextBox.Location = new System.Drawing.Point(30, 30);
            this.nameTextBox.MaxLength = 150;
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(295, 25);
            //
            // parentLabel
            //
            this.parentLabel.AutoSize = true;
            this.parentLabel.Location = new System.Drawing.Point(345, 90);
            this.parentLabel.Name = "parentLabel";
            this.parentLabel.Text = "التصنيف الرئيسي";
            //
            // parentComboBox
            //
            this.parentComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.parentComboBox.Location = new System.Drawing.Point(30, 85);
            this.parentComboBox.Name = "parentComboBox";
            this.parentComboBox.Size = new System.Drawing.Size(295, 25);
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(245, 155);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(90, 35);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(140, 155);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(90, 35);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // CategoryEditorForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(470, 230);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.parentLabel);
            this.Controls.Add(this.parentComboBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CategoryEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إضافة تصنيف";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
