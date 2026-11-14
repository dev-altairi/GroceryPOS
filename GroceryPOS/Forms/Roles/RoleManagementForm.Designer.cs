namespace GroceryPOS
{
    public partial class RoleManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button toggleButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn permissionNamesColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isSystemColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isActiveColumn;
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
            this.refreshButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.permissionNamesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isSystemColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.isActiveColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
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
            this.addButton.Size = new System.Drawing.Size(120, 32);
            this.addButton.Text = "إضافة دور";
            this.addButton.UseVisualStyleBackColor = true;
            //
            // editButton
            //
            this.editButton.Enabled = false;
            this.editButton.Margin = new System.Windows.Forms.Padding(4);
            this.editButton.Size = new System.Drawing.Size(120, 32);
            this.editButton.Text = "تعديل الصلاحيات";
            this.editButton.UseVisualStyleBackColor = true;
            //
            // toggleButton
            //
            this.toggleButton.Enabled = false;
            this.toggleButton.Margin = new System.Windows.Forms.Padding(4);
            this.toggleButton.Size = new System.Drawing.Size(120, 32);
            this.toggleButton.Text = "تعطيل";
            this.toggleButton.UseVisualStyleBackColor = true;
            //
            // refreshButton
            //
            this.refreshButton.Margin = new System.Windows.Forms.Padding(4);
            this.refreshButton.Size = new System.Drawing.Size(120, 32);
            this.refreshButton.Text = "تحديث";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Size = new System.Drawing.Size(120, 32);
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
            this.nameColumn.DataPropertyName = "Name";
            this.nameColumn.FillWeight = 95F;
            this.nameColumn.HeaderText = "اسم الدور";
            this.nameColumn.Name = "Name";
            this.descriptionColumn.DataPropertyName = "Description";
            this.descriptionColumn.FillWeight = 120F;
            this.descriptionColumn.HeaderText = "الوصف";
            this.descriptionColumn.Name = "Description";
            this.permissionNamesColumn.DataPropertyName = "PermissionNames";
            this.permissionNamesColumn.FillWeight = 210F;
            this.permissionNamesColumn.HeaderText = "الصلاحيات";
            this.permissionNamesColumn.Name = "PermissionNames";
            this.isSystemColumn.DataPropertyName = "IsSystem";
            this.isSystemColumn.FillWeight = 50F;
            this.isSystemColumn.HeaderText = "نظام";
            this.isSystemColumn.Name = "IsSystem";
            this.isActiveColumn.DataPropertyName = "IsActive";
            this.isActiveColumn.FillWeight = 50F;
            this.isActiveColumn.HeaderText = "نشط";
            this.isActiveColumn.Name = "IsActive";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.idColumn,
                this.nameColumn,
                this.descriptionColumn,
                this.permissionNamesColumn,
                this.isSystemColumn,
                this.isActiveColumn
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
            // RoleManagementForm
            //
            this.ClientSize = new System.Drawing.Size(980, 560);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.status);
            this.Controls.Add(this.toolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(780, 430);
            this.Name = "RoleManagementForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إدارة الأدوار والصلاحيات";
            this.toolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
