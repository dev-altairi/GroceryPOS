namespace GroceryPOS
{
    public partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel layoutPanel;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label identityLabel;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Label welcomeLabel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button openProductsButton;
        private System.Windows.Forms.Button openUsersButton;
        private System.Windows.Forms.Button openRolesButton;
        private System.Windows.Forms.Button changePasswordButton;
        private System.Windows.Forms.Button logoutButton;
        private System.Windows.Forms.Button openSuppliersButton;
        private System.Windows.Forms.Button openPurchasesButton;
        private System.Windows.Forms.Button openShiftsButton;
        private System.Windows.Forms.Button openSalesButton;
        private System.Windows.Forms.Button openSalesReturnsButton;

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
            this.layoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.identityLabel = new System.Windows.Forms.Label();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.welcomeLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.openProductsButton = new System.Windows.Forms.Button();
            this.openUsersButton = new System.Windows.Forms.Button();
            this.openRolesButton = new System.Windows.Forms.Button();
            this.changePasswordButton = new System.Windows.Forms.Button();
            this.logoutButton = new System.Windows.Forms.Button();
            this.openSuppliersButton = new System.Windows.Forms.Button();
            this.openPurchasesButton = new System.Windows.Forms.Button();
            this.openShiftsButton = new System.Windows.Forms.Button();
            this.openSalesButton = new System.Windows.Forms.Button();
            this.openSalesReturnsButton = new System.Windows.Forms.Button();
            this.layoutPanel.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutPanel
            //
            this.layoutPanel.ColumnCount = 1;
            this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(
                System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.Controls.Add(this.headerPanel, 0, 0);
            this.layoutPanel.Controls.Add(this.contentPanel, 0, 1);
            this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutPanel.Location = new System.Drawing.Point(0, 0);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.layoutPanel.RowCount = 2;
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(
                System.Windows.Forms.SizeType.Absolute, 105F));
            this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(
                System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPanel.Size = new System.Drawing.Size(1100, 700);
            this.layoutPanel.TabIndex = 0;
            //
            // headerPanel
            //
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(25, 74, 92);
            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Controls.Add(this.identityLabel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerPanel.Location = new System.Drawing.Point(3, 3);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(24, 15, 24, 15);
            this.headerPanel.Size = new System.Drawing.Size(1094, 99);
            this.headerPanel.TabIndex = 0;
            //
            // titleLabel
            //
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 19F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.White;
            this.titleLabel.Location = new System.Drawing.Point(25, 12);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(251, 36);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "نظام إدارة البقالة";
            //
            // identityLabel
            //
            this.identityLabel.AutoSize = true;
            this.identityLabel.ForeColor = System.Drawing.Color.White;
            this.identityLabel.Location = new System.Drawing.Point(28, 62);
            this.identityLabel.Name = "identityLabel";
            this.identityLabel.Size = new System.Drawing.Size(230, 25);
            this.identityLabel.TabIndex = 1;
            this.identityLabel.Text = "المستخدم: اسم المستخدم | الدور: مدير";
            //
            // contentPanel
            //
            this.contentPanel.Controls.Add(this.welcomeLabel);
            this.contentPanel.Controls.Add(this.statusLabel);
            this.contentPanel.Controls.Add(this.openProductsButton);
            this.contentPanel.Controls.Add(this.openUsersButton);
            this.contentPanel.Controls.Add(this.openRolesButton);
            this.contentPanel.Controls.Add(this.changePasswordButton);
            this.contentPanel.Controls.Add(this.logoutButton);
            this.contentPanel.Controls.Add(this.openSuppliersButton);
            this.contentPanel.Controls.Add(this.openPurchasesButton);
            this.contentPanel.Controls.Add(this.openShiftsButton);
            this.contentPanel.Controls.Add(this.openSalesButton);
            this.contentPanel.Controls.Add(this.openSalesReturnsButton);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(3, 108);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(45);
            this.contentPanel.Size = new System.Drawing.Size(1094, 589);
            this.contentPanel.TabIndex = 1;
            //
            // welcomeLabel
            //
            this.welcomeLabel.AutoSize = true;
            this.welcomeLabel.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.welcomeLabel.Location = new System.Drawing.Point(45, 50);
            this.welcomeLabel.Name = "welcomeLabel";
            this.welcomeLabel.Size = new System.Drawing.Size(315, 41);
            this.welcomeLabel.TabIndex = 0;
            this.welcomeLabel.Text = "مرحبًا بك، اسم المستخدم";
            //
            // statusLabel
            //
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(50, 115);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(252, 25);
            this.statusLabel.TabIndex = 1;
            this.statusLabel.Text = "تم تسجيل الدخول بنجاح. لديك 0 صلاحية فعّالة.";
            //
            // openProductsButton
            //
            this.openProductsButton.Location = new System.Drawing.Point(50, 175);
            this.openProductsButton.Name = "openProductsButton";
            this.openProductsButton.Size = new System.Drawing.Size(180, 48);
            this.openProductsButton.TabIndex = 2;
            this.openProductsButton.Text = "إدارة المنتجات";
            this.openProductsButton.UseVisualStyleBackColor = true;
            //
            // openUsersButton
            //
            this.openUsersButton.Location = new System.Drawing.Point(245, 175);
            this.openUsersButton.Name = "openUsersButton";
            this.openUsersButton.Size = new System.Drawing.Size(180, 48);
            this.openUsersButton.TabIndex = 3;
            this.openUsersButton.Text = "إدارة المستخدمين";
            this.openUsersButton.UseVisualStyleBackColor = true;
            //
            // openRolesButton
            //
            this.openRolesButton.Location = new System.Drawing.Point(440, 175);
            this.openRolesButton.Name = "openRolesButton";
            this.openRolesButton.Size = new System.Drawing.Size(180, 48);
            this.openRolesButton.TabIndex = 4;
            this.openRolesButton.Text = "إدارة الأدوار";
            this.openRolesButton.UseVisualStyleBackColor = true;
            //
            // changePasswordButton
            //
            this.changePasswordButton.Location = new System.Drawing.Point(245, 245);
            this.changePasswordButton.Name = "changePasswordButton";
            this.changePasswordButton.Size = new System.Drawing.Size(180, 42);
            this.changePasswordButton.TabIndex = 5;
            this.changePasswordButton.Text = "تغيير كلمة المرور";
            this.changePasswordButton.UseVisualStyleBackColor = true;
            //
            // logoutButton
            //
            this.logoutButton.Location = new System.Drawing.Point(50, 245);
            this.logoutButton.Name = "logoutButton";
            this.logoutButton.Size = new System.Drawing.Size(150, 42);
            this.logoutButton.TabIndex = 6;
            this.logoutButton.Text = "تسجيل الخروج";
            this.logoutButton.UseVisualStyleBackColor = true;
            //
            // openSuppliersButton
            //
            this.openSuppliersButton.Location = new System.Drawing.Point(50, 320);
            this.openSuppliersButton.Name = "openSuppliersButton";
            this.openSuppliersButton.Size = new System.Drawing.Size(180, 48);
            this.openSuppliersButton.TabIndex = 7;
            this.openSuppliersButton.Text = "إدارة الموردين";
            this.openSuppliersButton.UseVisualStyleBackColor = true;
            //
            // openPurchasesButton
            //
            this.openPurchasesButton.Location = new System.Drawing.Point(245, 320);
            this.openPurchasesButton.Name = "openPurchasesButton";
            this.openPurchasesButton.Size = new System.Drawing.Size(180, 48);
            this.openPurchasesButton.TabIndex = 8;
            this.openPurchasesButton.Text = "فواتير المشتريات";
            this.openPurchasesButton.UseVisualStyleBackColor = true;
            //
            // openShiftsButton
            //
            this.openShiftsButton.Location = new System.Drawing.Point(440, 320);
            this.openShiftsButton.Name = "openShiftsButton";
            this.openShiftsButton.Size = new System.Drawing.Size(180, 48);
            this.openShiftsButton.TabIndex = 9;
            this.openShiftsButton.Text = "الورديات والصندوق";
            this.openShiftsButton.UseVisualStyleBackColor = true;
            //
            // openSalesButton
            //
            this.openSalesButton.Location = new System.Drawing.Point(635, 320);
            this.openSalesButton.Name = "openSalesButton";
            this.openSalesButton.Size = new System.Drawing.Size(180, 48);
            this.openSalesButton.TabIndex = 10;
            this.openSalesButton.Text = "نقطة البيع";
            this.openSalesButton.UseVisualStyleBackColor = true;
            //
            // openSalesReturnsButton
            //
            this.openSalesReturnsButton.Location = new System.Drawing.Point(50, 390);
            this.openSalesReturnsButton.Name = "openSalesReturnsButton";
            this.openSalesReturnsButton.Size = new System.Drawing.Size(180, 48);
            this.openSalesReturnsButton.TabIndex = 11;
            this.openSalesReturnsButton.Text = "مرتجعات المبيعات";
            this.openSalesReturnsButton.UseVisualStyleBackColor = true;
            //
            // DashboardForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.layoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "DashboardForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "الرئيسية - GroceryPOS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.layoutPanel.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.contentPanel.ResumeLayout(false);
            this.contentPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
