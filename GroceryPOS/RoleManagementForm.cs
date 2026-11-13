using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class RoleManagementForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly DataGridView grid;
        private readonly Button editButton;
        private readonly Button toggleButton;
        private readonly Label status;

        internal RoleManagementForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "إدارة الأدوار والصلاحيات";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(980, 560);
            MinimumSize = new Size(780, 430);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 55,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8)
            };
            var addButton = CreateButton("إضافة دور");
            addButton.Click += (sender, args) => OpenEditor(null);
            toolbar.Controls.Add(addButton);

            editButton = CreateButton("تعديل الصلاحيات");
            editButton.Enabled = false;
            editButton.Click += (sender, args) =>
            {
                ManagedRoleRecord selected = SelectedRole();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toolbar.Controls.Add(editButton);

            toggleButton = CreateButton("تعطيل");
            toggleButton.Enabled = false;
            toggleButton.Click += ToggleButton_Click;
            toolbar.Controls.Add(toggleButton);

            var refreshButton = CreateButton("تحديث");
            refreshButton.Click += (sender, args) => RefreshRoles();
            toolbar.Controls.Add(refreshButton);

            var closeButton = CreateButton("إغلاق");
            closeButton.Click += (sender, args) => Close();
            toolbar.Controls.Add(closeButton);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id", Visible = false
            });
            AddColumn("Name", "اسم الدور", 95);
            AddColumn("Description", "الوصف", 120);
            AddColumn("PermissionNames", "الصلاحيات", 210);
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsSystem", HeaderText = "نظام", FillWeight = 50
            });
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive", HeaderText = "نشط", FillWeight = 50
            });
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

            status = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 10, 0)
            };
            Controls.Add(grid);
            Controls.Add(status);
            Controls.Add(toolbar);
            Shown += (sender, args) => RefreshRoles();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 120,
                Height = 32,
                Margin = new Padding(4)
            };
        }

        private void AddColumn(string property, string title, float weight)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            });
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

    internal sealed class RoleEditorForm : Form
    {
        private readonly TextBox nameTextBox;
        private readonly TextBox descriptionTextBox;
        private readonly CheckedListBox permissionsListBox;

        internal string RoleName
        {
            get { return nameTextBox.Text; }
        }

        internal string Description
        {
            get { return descriptionTextBox.Text; }
        }

        internal IList<int> SelectedPermissionIds
        {
            get
            {
                return permissionsListBox.CheckedItems
                    .Cast<PermissionOption>()
                    .Select(permission => permission.Id)
                    .ToList();
            }
        }

        internal RoleEditorForm(ManagedRoleRecord current, IList<PermissionOption> permissions)
        {
            Text = current == null ? "إضافة دور" : "تعديل الدور";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(600, 650);
            MinimumSize = new Size(600, 650);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "اسم الدور",
                AutoSize = true,
                Location = new Point(490, 28)
            });
            nameTextBox = new TextBox
            {
                Location = new Point(35, 25),
                Width = 420,
                MaxLength = 100,
                Text = current == null ? string.Empty : current.Name,
                ReadOnly = current != null && current.IsSystem
            };
            Controls.Add(nameTextBox);

            Controls.Add(new Label
            {
                Text = "الوصف",
                AutoSize = true,
                Location = new Point(520, 78)
            });
            descriptionTextBox = new TextBox
            {
                Location = new Point(35, 75),
                Width = 420,
                MaxLength = 300,
                Text = current == null ? string.Empty : current.Description
            };
            Controls.Add(descriptionTextBox);

            Controls.Add(new Label
            {
                Text = "الصلاحيات الممنوحة",
                AutoSize = true,
                Location = new Point(430, 125)
            });
            permissionsListBox = new CheckedListBox
            {
                Location = new Point(35, 150),
                Width = 520,
                Height = 420,
                CheckOnClick = true,
                IntegralHeight = false,
                HorizontalScrollbar = true
            };
            foreach (PermissionOption permission in permissions)
            {
                bool selected = current != null && current.PermissionIds.Contains(permission.Id);
                permissionsListBox.Items.Add(permission, selected);
            }

            Controls.Add(permissionsListBox);
            var save = new Button
            {
                Text = "حفظ",
                Location = new Point(355, 590),
                Width = 90,
                Height = 34
            };
            save.Click += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("أدخل اسم الدور.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nameTextBox.Focus();
                    return;
                }

                if (permissionsListBox.CheckedItems.Count == 0)
                {
                    MessageBox.Show("اختر صلاحية واحدة على الأقل.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(245, 590),
                Width = 90,
                Height = 34,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }
    }
}
