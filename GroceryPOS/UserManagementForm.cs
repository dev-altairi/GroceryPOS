using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class UserManagementForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly DataGridView grid;
        private readonly Button editButton;
        private readonly Button toggleButton;
        private readonly Button resetPasswordButton;
        private readonly Label status;

        internal UserManagementForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "إدارة المستخدمين";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(920, 540);
            MinimumSize = new Size(760, 420);
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
            var addButton = CreateButton("إضافة مستخدم");
            addButton.Click += (sender, args) => OpenEditor(null);
            toolbar.Controls.Add(addButton);

            editButton = CreateButton("تعديل");
            editButton.Enabled = false;
            editButton.Click += (sender, args) =>
            {
                ManagedUserRecord selected = SelectedUser();
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

            resetPasswordButton = CreateButton("إعادة كلمة المرور");
            resetPasswordButton.Enabled = false;
            resetPasswordButton.Click += ResetPasswordButton_Click;
            toolbar.Controls.Add(resetPasswordButton);

            var refreshButton = CreateButton("تحديث");
            refreshButton.Click += (sender, args) => RefreshUsers();
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
            AddColumn("FullName", "الاسم", 120);
            AddColumn("Username", "اسم المستخدم", 90);
            AddColumn("RoleNames", "الأدوار", 150);
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive", HeaderText = "نشط", FillWeight = 55
            });
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "MustChangePassword", HeaderText = "يلزم تغيير كلمة المرور", FillWeight = 95
            });
            AddColumn("LockedUntil", "القفل حتى", 90, "g");
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
            Shown += (sender, args) => RefreshUsers();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 112,
                Height = 32,
                Margin = new Padding(4)
            };
        }

        private void AddColumn(string property, string title, float weight, string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
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

    internal sealed class UserEditorForm : Form
    {
        private readonly TextBox fullNameTextBox;
        private readonly TextBox usernameTextBox;
        private readonly TextBox passwordTextBox;
        private readonly TextBox confirmPasswordTextBox;
        private readonly CheckedListBox rolesListBox;

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
        {
            Text = current == null ? "إضافة مستخدم" : "تعديل مستخدم";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(500, 585);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "الاسم الكامل",
                AutoSize = true,
                Location = new Point(380, 28)
            });
            fullNameTextBox = AddTextField(25, 150, 300);
            fullNameTextBox.MaxLength = 150;
            fullNameTextBox.Text = current == null ? string.Empty : current.FullName;

            Controls.Add(new Label
            {
                Text = "اسم المستخدم",
                AutoSize = true,
                Location = new Point(380, 78)
            });
            usernameTextBox = AddTextField(75, 150, 300);
            usernameTextBox.MaxLength = 50;
            usernameTextBox.Text = current == null ? string.Empty : current.Username;

            Controls.Add(new Label
            {
                Text = current == null ? "كلمة المرور المؤقتة (12 حرفًا على الأقل)" : "كلمة المرور لا تتغير هنا",
                AutoSize = true,
                Location = new Point(380, 128)
            });
            passwordTextBox = AddTextField(125, 150, 300);
            passwordTextBox.MaxLength = 128;
            passwordTextBox.UseSystemPasswordChar = true;
            passwordTextBox.Enabled = current == null;

            Controls.Add(new Label
            {
                Text = "تأكيد كلمة المرور",
                AutoSize = true,
                Location = new Point(380, 178)
            });
            confirmPasswordTextBox = AddTextField(175, 150, 300);
            confirmPasswordTextBox.MaxLength = 128;
            confirmPasswordTextBox.UseSystemPasswordChar = true;
            confirmPasswordTextBox.Enabled = current == null;

            Controls.Add(new Label
            {
                Text = "الأدوار",
                AutoSize = true,
                Location = new Point(410, 232)
            });
            rolesListBox = new CheckedListBox
            {
                Location = new Point(35, 255),
                Width = 400,
                Height = 230,
                CheckOnClick = true,
                IntegralHeight = false
            };
            foreach (RoleOption role in roles)
            {
                bool selected = current != null && current.RoleIds.Contains(role.Id);
                rolesListBox.Items.Add(role, selected);
            }

            Controls.Add(rolesListBox);
            Controls.Add(new Label
            {
                Text = "يُطلب من المستخدم تغيير كلمة المرور المؤقتة عند أول دخول.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(95, 490)
            });

            var save = new Button
            {
                Text = "حفظ",
                Location = new Point(275, 525),
                Width = 90,
                Height = 32
            };
            save.Click += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(fullNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(usernameTextBox.Text))
                {
                    MessageBox.Show("أدخل الاسم واسم المستخدم.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (current == null && passwordTextBox.Text.Length < 12)
                {
                    MessageBox.Show("كلمة المرور يجب أن تكون من 12 حرفًا على الأقل.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    passwordTextBox.Focus();
                    return;
                }

                if (current == null &&
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
            };
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(165, 525),
                Width = 90,
                Height = 32,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private TextBox AddTextField(int top, int left, int width)
        {
            var textBox = new TextBox
            {
                Location = new Point(left, top),
                Width = width
            };
            Controls.Add(textBox);
            return textBox;
        }
    }
}
