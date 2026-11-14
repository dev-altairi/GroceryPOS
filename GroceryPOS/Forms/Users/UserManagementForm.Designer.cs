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
        private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fullNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn usernameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn roleNamesColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isActiveColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn mustChangePasswordColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lockedUntilColumn;
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
            this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fullNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.usernameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.roleNamesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isActiveColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.mustChangePasswordColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.lockedUntilColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.grid.AutoGenerateColumns = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.idColumn.DataPropertyName = "Id";
            this.idColumn.Name = "Id";
            this.idColumn.Visible = false;
            this.fullNameColumn.DataPropertyName = "FullName";
            this.fullNameColumn.FillWeight = 120F;
            this.fullNameColumn.HeaderText = "الاسم";
            this.fullNameColumn.Name = "FullName";
            this.usernameColumn.DataPropertyName = "Username";
            this.usernameColumn.FillWeight = 90F;
            this.usernameColumn.HeaderText = "اسم المستخدم";
            this.usernameColumn.Name = "Username";
            this.roleNamesColumn.DataPropertyName = "RoleNames";
            this.roleNamesColumn.FillWeight = 150F;
            this.roleNamesColumn.HeaderText = "الأدوار";
            this.roleNamesColumn.Name = "RoleNames";
            this.isActiveColumn.DataPropertyName = "IsActive";
            this.isActiveColumn.FillWeight = 55F;
            this.isActiveColumn.HeaderText = "نشط";
            this.isActiveColumn.Name = "IsActive";
            this.mustChangePasswordColumn.DataPropertyName = "MustChangePassword";
            this.mustChangePasswordColumn.FillWeight = 95F;
            this.mustChangePasswordColumn.HeaderText = "يلزم تغيير كلمة المرور";
            this.mustChangePasswordColumn.Name = "MustChangePassword";
            this.lockedUntilColumn.DataPropertyName = "LockedUntil";
            this.lockedUntilColumn.DefaultCellStyle.Format = "g";
            this.lockedUntilColumn.FillWeight = 90F;
            this.lockedUntilColumn.HeaderText = "القفل حتى";
            this.lockedUntilColumn.Name = "LockedUntil";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.idColumn,
                this.fullNameColumn,
                this.usernameColumn,
                this.roleNamesColumn,
                this.isActiveColumn,
                this.mustChangePasswordColumn,
                this.lockedUntilColumn
            });
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
