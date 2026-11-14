using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class UserEditorForm : Form
    {
        public UserEditorForm()
        {
            InitializeComponent();
        }

        internal string FullName
        {
            get { return fullNameTextBox.Text; }
        }

        internal string Username
        {
            get { return usernameTextBox.Text; }
        }

        internal string Password
        {
            get { return passwordTextBox.Text; }
        }

        internal string ConfirmPassword
        {
            get { return confirmPasswordTextBox.Text; }
        }

        internal IList<int> SelectedRoleIds
        {
            get
            {
                return rolesListBox.CheckedItems
                    .Cast<RoleOption>()
                    .Select(role => role.Id)
                    .ToList();
            }
        }

        internal UserEditorForm(ManagedUserRecord current, IList<RoleOption> roles)
            : this()
        {
            Text = current == null ? "إضافة مستخدم" : "تعديل مستخدم";
            fullNameTextBox.Text = current == null ? string.Empty : current.FullName;
            usernameTextBox.Text = current == null ? string.Empty : current.Username;
            passwordTextBox.Enabled = current == null;
            confirmPasswordTextBox.Enabled = current == null;
            passwordLabel.Text = current == null
                ? "كلمة المرور المؤقتة (12 حرفًا على الأقل)"
                : "كلمة المرور لا تتغير هنا";
            foreach (RoleOption role in roles)
            {
                bool selected = current != null && current.RoleIds.Contains(role.Id);
                rolesListBox.Items.Add(role, selected);
            }

            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(usernameTextBox.Text))
            {
                MessageBox.Show("أدخل الاسم واسم المستخدم.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (passwordTextBox.Enabled && passwordTextBox.Text.Length < 12)
            {
                MessageBox.Show("كلمة المرور يجب أن تكون من 12 حرفًا على الأقل.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordTextBox.Focus();
                return;
            }

            if (passwordTextBox.Enabled &&
                !string.Equals(passwordTextBox.Text, confirmPasswordTextBox.Text, StringComparison.Ordinal))
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                confirmPasswordTextBox.Clear();
                confirmPasswordTextBox.Focus();
                return;
            }

            if (rolesListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("اختر دورًا واحدًا على الأقل.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
