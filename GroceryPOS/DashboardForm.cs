using System.Drawing;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class DashboardForm : Form
    {
        internal DashboardForm(AuthenticatedUser user)
        {
            Text = "الرئيسية - GroceryPOS";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(800, 500);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 11F);

            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 74, 92),
                Padding = new Padding(24, 15, 24, 15)
            };
            var title = new Label
            {
                Text = "نظام إدارة البقالة",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 12)
            };
            var identity = new Label
            {
                Text = "المستخدم: " + user.FullName +
                       (user.Roles.Count == 0 ? string.Empty : "  |  الدور: " + string.Join("، ", user.Roles)),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(28, 62)
            };
            header.Controls.Add(title);
            header.Controls.Add(identity);
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(45)
            };
            var welcome = new Label
            {
                Text = "مرحبًا بك، " + user.FullName,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(45, 50)
            };
            var status = new Label
            {
                Text = string.Format(
                    "تم تسجيل الدخول بنجاح. لديك {0} صلاحية فعّالة.",
                    user.Permissions.Count,
                    System.Environment.NewLine),
                AutoSize = true,
                Location = new Point(50, 115)
            };
            var openProductsButton = new Button
            {
                Text = "إدارة المنتجات",
                Width = 180,
                Height = 48,
                Location = new Point(50, 175),
                Visible = user.HasPermission("Products.View") || user.HasPermission("Products.Manage")
            };
            openProductsButton.Click += (sender, args) =>
            {
                using (var productsForm = new ProductCatalogForm(user))
                {
                    productsForm.ShowDialog(this);
                }
            };
            var openUsersButton = new Button
            {
                Text = "إدارة المستخدمين",
                Width = 180,
                Height = 48,
                Location = new Point(245, 175),
                Visible = user.HasPermission("Users.Manage")
            };
            openUsersButton.Click += (sender, args) =>
            {
                using (var usersForm = new UserManagementForm(user))
                {
                    usersForm.ShowDialog(this);
                }
            };
            var openRolesButton = new Button
            {
                Text = "إدارة الأدوار",
                Width = 180,
                Height = 48,
                Location = new Point(440, 175),
                Visible = user.HasPermission("Roles.Manage")
            };
            openRolesButton.Click += (sender, args) =>
            {
                using (var rolesForm = new RoleManagementForm(user))
                {
                    rolesForm.ShowDialog(this);
                }
            };
            var changePasswordButton = new Button
            {
                Text = "تغيير كلمة المرور",
                Width = 180,
                Height = 42,
                Location = new Point(245, 245)
            };
            changePasswordButton.Click += (sender, args) =>
            {
                using (var passwordForm = new PasswordManagementForm(user, false))
                {
                    passwordForm.ShowDialog(this);
                }
            };
            var openSuppliersButton = new Button
            {
                Text = "إدارة الموردين",
                Width = 180,
                Height = 48,
                Location = new Point(50, 320),
                Visible = user.HasPermission("Suppliers.View") || user.HasPermission("Suppliers.Manage")
            };
            openSuppliersButton.Click += (sender, args) =>
            {
                using (var suppliersForm = new SupplierManagementForm(user))
                {
                    suppliersForm.ShowDialog(this);
                }
            };
            var openPurchasesButton = new Button
            {
                Text = "فواتير المشتريات",
                Width = 180,
                Height = 48,
                Location = new Point(245, 320),
                Visible = user.HasPermission("Purchases.Invoice")
            };
            openPurchasesButton.Click += (sender, args) =>
            {
                using (var purchasesForm = new PurchasesForm(user))
                {
                    purchasesForm.ShowDialog(this);
                }
            };
            var logoutButton = new Button
            {
                Text = "تسجيل الخروج",
                Width = 150,
                Height = 42,
                Location = new Point(50, 245)
            };
            logoutButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.Retry;
                Close();
            };
            content.Controls.Add(welcome);
            content.Controls.Add(status);
            content.Controls.Add(openProductsButton);
            content.Controls.Add(openUsersButton);
            content.Controls.Add(openRolesButton);
            content.Controls.Add(changePasswordButton);
            content.Controls.Add(logoutButton);
            content.Controls.Add(openSuppliersButton);
            content.Controls.Add(openPurchasesButton);
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RightToLeft = RightToLeft.Yes
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(content, 0, 1);
            Controls.Add(layout);
        }
    }
}
