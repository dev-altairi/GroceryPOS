namespace GroceryPOS
{
    public partial class ShiftManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button movementButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button closeWindowButton;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn idColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cashierIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cashierNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn deviceNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn openedAtColumn;
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
            this.openButton = new System.Windows.Forms.Button();
            this.movementButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.closeWindowButton = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            this.idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cashierIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cashierNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deviceNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.openedAtColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.Label();
            this.toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // toolbar
            //
            this.toolbar.Controls.Add(this.openButton);
            this.toolbar.Controls.Add(this.movementButton);
            this.toolbar.Controls.Add(this.closeButton);
            this.toolbar.Controls.Add(this.refreshButton);
            this.toolbar.Controls.Add(this.closeWindowButton);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.toolbar.Height = 56;
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(8);
            this.toolbar.WrapContents = false;
            //
            // openButton
            //
            this.openButton.Margin = new System.Windows.Forms.Padding(4);
            this.openButton.Size = new System.Drawing.Size(120, 32);
            this.openButton.Text = "فتح وردية";
            this.openButton.UseVisualStyleBackColor = true;
            //
            // movementButton
            //
            this.movementButton.Enabled = false;
            this.movementButton.Margin = new System.Windows.Forms.Padding(4);
            this.movementButton.Size = new System.Drawing.Size(120, 32);
            this.movementButton.Text = "حركة صندوق";
            this.movementButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            this.closeButton.Enabled = false;
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Size = new System.Drawing.Size(120, 32);
            this.closeButton.Text = "إغلاق الوردية";
            this.closeButton.UseVisualStyleBackColor = true;
            //
            // refreshButton
            //
            this.refreshButton.Margin = new System.Windows.Forms.Padding(4);
            this.refreshButton.Size = new System.Drawing.Size(120, 32);
            this.refreshButton.Text = "تحديث";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // closeWindowButton
            //
            this.closeWindowButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeWindowButton.Size = new System.Drawing.Size(120, 32);
            this.closeWindowButton.Text = "إغلاق";
            this.closeWindowButton.UseVisualStyleBackColor = true;
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
            this.cashierIdColumn.DataPropertyName = "CashierId";
            this.cashierIdColumn.Name = "CashierId";
            this.cashierIdColumn.Visible = false;
            this.cashierNameColumn.DataPropertyName = "CashierName";
            this.cashierNameColumn.FillWeight = 100F;
            this.cashierNameColumn.HeaderText = "الكاشير";
            this.cashierNameColumn.Name = "CashierName";
            this.deviceNameColumn.DataPropertyName = "DeviceName";
            this.deviceNameColumn.FillWeight = 80F;
            this.deviceNameColumn.HeaderText = "الجهاز";
            this.deviceNameColumn.Name = "DeviceName";
            this.openedAtColumn.DataPropertyName = "OpenedAt";
            this.openedAtColumn.DefaultCellStyle.Format = "g";
            this.openedAtColumn.FillWeight = 90F;
            this.openedAtColumn.HeaderText = "وقت الفتح";
            this.openedAtColumn.Name = "OpenedAt";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.idColumn,
                this.cashierIdColumn,
                this.cashierNameColumn,
                this.deviceNameColumn,
                this.openedAtColumn
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
            // ShiftManagementForm
            //
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.status);
            this.Controls.Add(this.toolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(720, 380);
            this.Name = "ShiftManagementForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "الورديات والصندوق";
            this.toolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
