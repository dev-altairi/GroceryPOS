using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class RoleManagementForm : Form
    {
        private AuthenticatedUser user;

        public RoleManagementForm()
        {
            InitializeComponent();
        }

        internal RoleManagementForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Click += (sender, args) => OpenEditor(null);
            editButton.Click += (sender, args) =>
            {
                ManagedRoleRecord selected = SelectedRole();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toggleButton.Click += ToggleButton_Click;
            refreshButton.Click += (sender, args) => RefreshRoles();
            closeButton.Click += (sender, args) => Close();
            grid.SelectionChanged += (sender, args) => UpdateSelection();
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    ManagedRoleRecord selected = SelectedRole();
                    if (selected != null)
                    {
                        OpenEditor(selected);
                    }
                }
            };
            Shown += (sender, args) => RefreshRoles();
        }

        private ManagedRoleRecord SelectedRole()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as ManagedRoleRecord;
        }

        private void UpdateSelection()
        {
            ManagedRoleRecord selected = SelectedRole();
            editButton.Enabled = selected != null;
            toggleButton.Enabled = selected != null && !selected.IsSystem;
            toggleButton.Text = selected != null && selected.IsActive ? "تعطيل" : "تفعيل";
        }

        private void RefreshRoles()
        {
            try
            {
                List<ManagedRoleRecord> roles = RolesService.Load(user);
                grid.DataSource = roles;
                status.Text = "عدد الأدوار: " + roles.Count;
                UpdateSelection();
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل الأدوار من قاعدة البيانات.");
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

        private void OpenEditor(ManagedRoleRecord role)
        {
            try
            {
                List<PermissionOption> permissions = RolesService.LoadPermissions(user);
                using (var editor = new RoleEditorForm(role, permissions))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    RolesService.Save(
                        user,
                        role == null ? (int?)null : role.Id,
                        editor.RoleName,
                        editor.Description,
                        editor.SelectedPermissionIds);
                    RefreshRoles();
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
                ShowError(error, "تعذر حفظ الدور في قاعدة البيانات.");
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            ManagedRoleRecord selected = SelectedRole();
            if (selected == null || selected.IsSystem)
            {
                return;
            }

            bool activate = !selected.IsActive;
            string prompt = activate
                ? "هل تريد تفعيل الدور " + selected.Name + "؟"
                : "هل تريد تعطيل الدور " + selected.Name + "؟";
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
                RolesService.SetActive(user, selected.Id, activate);
                RefreshRoles();
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
                ShowError(error, "تعذر تغيير حالة الدور.");
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

}
