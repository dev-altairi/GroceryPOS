using System;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class DashboardForm : Form
    {
        private AuthenticatedUser user;

        public DashboardForm()
        {
            InitializeComponent();
        }

        internal DashboardForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            if (authenticatedUser == null)
            {
                throw new ArgumentNullException("authenticatedUser");
            }

            user = authenticatedUser;
            identityLabel.Text = "المستخدم: " + user.FullName +
                                 (user.Roles.Count == 0
                                     ? string.Empty
                                     : "  |  الدور: " + string.Join("، ", user.Roles));
            welcomeLabel.Text = "مرحبًا بك، " + user.FullName;
            statusLabel.Text = string.Format(
                "تم تسجيل الدخول بنجاح. لديك {0} صلاحية فعّالة.",
                user.Permissions.Count);

            openProductsButton.Visible =
                user.HasPermission("Products.View") || user.HasPermission("Products.Manage");
            openUsersButton.Visible = user.HasPermission("Users.Manage");
            openRolesButton.Visible = user.HasPermission("Roles.Manage");
            openSuppliersButton.Visible =
                user.HasPermission("Suppliers.View") || user.HasPermission("Suppliers.Manage");
            openPurchasesButton.Visible = user.HasPermission("Purchases.Invoice");
            openShiftsButton.Visible = user.HasPermission("Shifts.Operate");
            openSalesButton.Visible = user.HasPermission("Sales.Create");
            openSalesReturnsButton.Visible = user.HasPermission("Sales.Return");

            openProductsButton.Click += OpenProductsButton_Click;
            openUsersButton.Click += OpenUsersButton_Click;
            openRolesButton.Click += OpenRolesButton_Click;
            changePasswordButton.Click += ChangePasswordButton_Click;
            openSuppliersButton.Click += OpenSuppliersButton_Click;
            openPurchasesButton.Click += OpenPurchasesButton_Click;
            openShiftsButton.Click += OpenShiftsButton_Click;
            openSalesButton.Click += OpenSalesButton_Click;
            openSalesReturnsButton.Click += OpenSalesReturnsButton_Click;
            logoutButton.Click += LogoutButton_Click;
        }

        private void OpenProductsButton_Click(object sender, EventArgs e)
        {
            using (var form = new ProductCatalogForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenUsersButton_Click(object sender, EventArgs e)
        {
            using (var form = new UserManagementForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenRolesButton_Click(object sender, EventArgs e)
        {
            using (var form = new RoleManagementForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            using (var form = new PasswordManagementForm(user, false))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenSuppliersButton_Click(object sender, EventArgs e)
        {
            using (var form = new SupplierManagementForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenPurchasesButton_Click(object sender, EventArgs e)
        {
            using (var form = new PurchasesForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenShiftsButton_Click(object sender, EventArgs e)
        {
            using (var form = new ShiftManagementForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenSalesButton_Click(object sender, EventArgs e)
        {
            using (var form = new SalesForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void OpenSalesReturnsButton_Click(object sender, EventArgs e)
        {
            using (var form = new SalesReturnForm(user))
            {
                form.ShowDialog(this);
            }
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
