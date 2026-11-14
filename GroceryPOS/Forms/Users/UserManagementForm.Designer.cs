namespace GroceryPOS
{
    public partial class UserManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button toggleButton;
        private System.Windows.Forms.Button resetPasswordButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label status;

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
            this.toolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.toggleButton = new System.Windows.Forms.Button();
            this.resetPasswordButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.status = new System.Windows.Forms.Label();
            this.toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // toolbar
            //
            this.toolbar.Controls.Add(this.addButton);
            this.toolbar.Controls.Add(this.editButton);
            this.toolbar.Controls.Add(this.toggleButton);
            this.toolbar.Controls.Add(this.resetPasswordButton);
            this.toolbar.Controls.Add(this.refreshButton);
            this.toolbar.Controls.Add(this.closeButton);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.toolbar.Height = 55;
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(8);
            this.toolbar.WrapContents = false;
            //
            // addButton
            //
            this.addButton.Margin = new System.Windows.Forms.Padding(4);
            this.addButton.Size = new System.Drawing.Size(112, 32);
            this.addButton.Text = "إضافة مستخدم";
            this.addButton.UseVisualStyleBackColor = true;
            //
            // editButton
            //
            this.editButton.Enabled = false;
            this.editButton.Margin = new System.Windows.Forms.Padding(4);
            this.editButton.Size = new System.Drawing.Size(112, 32);
            this.editButton.Text = "تعديل";
            this.editButton.UseVisualStyleBackColor = true;
            //
            // toggleButton
            //
            this.toggleButton.Enabled = false;
            this.toggleButton.Margin = new System.Windows.Forms.Padding(4);
            this.toggleButton.Size = new System.Drawing.Size(112, 32);
            this.toggleButton.Text = "تعطيل";
            this.toggleButton.UseVisualStyleBackColor = true;
            //
            // resetPasswordButton
            //
            this.resetPasswordButton.Enabled = false;
            this.resetPasswordButton.Margin = new System.Windows.Forms.Padding(4);
            this.resetPasswordButton.Size = new System.Drawing.Size(112, 32);
            this.resetPasswordButton.Text = "إعادة كلمة المرور";
            this.resetPasswordButton.UseVisualStyleBackColor = true;
            //
            // refreshButton
            //
            this.refreshButton.Margin = new System.Windows.Forms.Padding(4);
            this.refreshButton.Size = new System.Drawing.Size(112, 32);
            this.refreshButton.Text = "تحديث";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Size = new System.Drawing.Size(112, 32);
            this.closeButton.Text = "إغلاق";
            this.closeButton.UseVisualStyleBackColor = true;
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoGenerateColumns = true;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            //
            // status
            //
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Height = 30;
            this.status.Name = "status";
            this.status.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.status.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // UserManagementForm
            //
            this.ClientSize = new System.Drawing.Size(920, 540);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.status);
            this.Controls.Add(this.toolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(760, 420);
            this.Name = "UserManagementForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إدارة المستخدمين";
            this.toolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
