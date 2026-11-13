using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class UserManagementForm : Form
    {
        private AuthenticatedUser user;

        public UserManagementForm()
        {
            InitializeComponent();
        }

        internal UserManagementForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Click += (sender, args) => OpenEditor(null);
            editButton.Click += (sender, args) =>
            {
                ManagedUserRecord selected = SelectedUser();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toggleButton.Click += ToggleButton_Click;
            resetPasswordButton.Click += ResetPasswordButton_Click;
            refreshButton.Click += (sender, args) => RefreshUsers();
            closeButton.Click += (sender, args) => Close();
            grid.SelectionChanged += (sender, args) => UpdateSelection();
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    ManagedUserRecord selected = SelectedUser();
                    if (selected != null)
                    {
                        OpenEditor(selected);
                    }
                }
            };
            Shown += (sender, args) => RefreshUsers();
        }

        private ManagedUserRecord SelectedUser()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as ManagedUserRecord;
        }

        private void UpdateSelection()
        {
            ManagedUserRecord selected = SelectedUser();
            editButton.Enabled = selected != null;
            toggleButton.Enabled = selected != null && selected.Id != user.Id;
            toggleButton.Text = selected != null && selected.IsActive ? "تعطيل" : "تفعيل";
            resetPasswordButton.Enabled = selected != null && selected.Id != user.Id;
        }

        private void RefreshUsers()
        {
            try
            {
                List<ManagedUserRecord> users = UsersService.Load(user);
                grid.DataSource = users;
                status.Text = "عدد المستخدمين: " + users.Count;
                UpdateSelection();
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل المستخدمين من قاعدة البيانات.");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenEditor(ManagedUserRecord account)
        {
            try
            {
                List<RoleOption> roles = UsersService.LoadActiveRoles(user);
                using (var editor = new UserEditorForm(account, roles))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    UsersService.Save(
                        user,
                        account == null ? (int?)null : account.Id,
                        editor.Username,
                        editor.FullName,
                        editor.Password,
                        editor.SelectedRoleIds);
                    RefreshUsers();
                }
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر حفظ المستخدم في قاعدة البيانات.");
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            ManagedUserRecord selected = SelectedUser();
            if (selected == null)
            {
                return;
            }

            bool activate = !selected.IsActive;
            string prompt = activate
                ? "هل تريد تفعيل حساب " + selected.FullName + "؟"
                : "هل تريد تعطيل حساب " + selected.FullName + "؟";
            if (MessageBox.Show(
                    prompt,
                    Text,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UsersService.SetActive(user, selected.Id, activate);
                RefreshUsers();
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تغيير حالة المستخدم.");
            }
        }

        private void ResetPasswordButton_Click(object sender, EventArgs e)
        {
            ManagedUserRecord selected = SelectedUser();
            if (selected == null || selected.Id == user.Id)
            {
                return;
            }

            using (var resetForm = new PasswordManagementForm(user, selected.Id, selected.FullName))
            {
                if (resetForm.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshUsers();
                }
            }
        }

        private void ShowError(Exception error, string message)
        {
            try
            {
                AppLogger.Write(error);
                message += Environment.NewLine + "حُفظت التفاصيل التقنية في سجل التطبيق.";
            }
            catch (IOException)
            {
                message += Environment.NewLine + "تعذر حفظ سجل الخطأ.";
            }
            catch (UnauthorizedAccessException)
            {
                message += Environment.NewLine + "تعذر حفظ سجل الخطأ بسبب الأذونات.";
            }

            MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public partial class UserEditorForm : Form
    {
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

        public UserEditorForm()
        {
            InitializeComponent();
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
