namespace GroceryPOS
{
    public partial class SupplierEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label phoneLabel;
        private System.Windows.Forms.TextBox phoneTextBox;
        private System.Windows.Forms.Label addressLabel;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.Label notesLabel;
        private System.Windows.Forms.TextBox notesTextBox;
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
            this.phoneLabel = new System.Windows.Forms.Label();
            this.phoneTextBox = new System.Windows.Forms.TextBox();
            this.addressLabel = new System.Windows.Forms.Label();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.notesLabel = new System.Windows.Forms.Label();
            this.notesTextBox = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // nameLabel
            //
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(385, 29);
            this.nameLabel.Text = "اسم المورد";
            //
            // nameTextBox
            //
            this.nameTextBox.Location = new System.Drawing.Point(35, 25);
            this.nameTextBox.MaxLength = 200;
            this.nameTextBox.Size = new System.Drawing.Size(325, 25);
            //
            // phoneLabel
            //
            this.phoneLabel.AutoSize = true;
            this.phoneLabel.Location = new System.Drawing.Point(385, 79);
            this.phoneLabel.Text = "الهاتف";
            //
            // phoneTextBox
            //
            this.phoneTextBox.Location = new System.Drawing.Point(35, 75);
            this.phoneTextBox.MaxLength = 30;
            this.phoneTextBox.Size = new System.Drawing.Size(325, 25);
            //
            // addressLabel
            //
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(385, 129);
            this.addressLabel.Text = "العنوان";
            //
            // addressTextBox
            //
            this.addressTextBox.Location = new System.Drawing.Point(35, 125);
            this.addressTextBox.MaxLength = 300;
            this.addressTextBox.Size = new System.Drawing.Size(325, 25);
            //
            // notesLabel
            //
            this.notesLabel.AutoSize = true;
            this.notesLabel.Location = new System.Drawing.Point(385, 179);
            this.notesLabel.Text = "ملاحظات";
            //
            // notesTextBox
            //
            this.notesTextBox.Location = new System.Drawing.Point(35, 175);
            this.notesTextBox.MaxLength = 500;
            this.notesTextBox.Multiline = true;
            this.notesTextBox.Size = new System.Drawing.Size(325, 75);
            //
            // saveButton
            //
            this.saveButton.Location = new System.Drawing.Point(285, 290);
            this.saveButton.Size = new System.Drawing.Size(90, 34);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(175, 290);
            this.cancelButton.Size = new System.Drawing.Size(90, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // SupplierEditorForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(500, 360);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.phoneLabel);
            this.Controls.Add(this.phoneTextBox);
            this.Controls.Add(this.addressLabel);
            this.Controls.Add(this.addressTextBox);
            this.Controls.Add(this.notesLabel);
            this.Controls.Add(this.notesTextBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SupplierEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إضافة مورد";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
