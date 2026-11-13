using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class CategoryManagementForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly DataGridView grid;
        private readonly Button editButton;
        private readonly Button toggleButton;
        private readonly Label status;

        internal CategoryManagementForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "إدارة التصنيفات";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(800, 520);
            MinimumSize = new Size(650, 400);
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
            var addButton = CreateButton("إضافة تصنيف");
            addButton.Click += (sender, args) => OpenEditor(null);
            toolbar.Controls.Add(addButton);

            editButton = CreateButton("تعديل الاسم");
            editButton.Enabled = false;
            editButton.Click += (sender, args) =>
            {
                CategoryRecord selected = SelectedCategory();
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
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name", HeaderText = "اسم التصنيف", FillWeight = 180
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ParentName", HeaderText = "التصنيف الرئيسي", FillWeight = 120
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ProductCount", HeaderText = "عدد المنتجات", FillWeight = 70
            });
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive", HeaderText = "نشط", FillWeight = 55
            });
            grid.SelectionChanged += (sender, args) => UpdateSelection();
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    CategoryRecord selected = SelectedCategory();
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
            Shown += (sender, args) => RefreshCategories();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 115,
                Height = 32,
                Margin = new Padding(4)
            };
        }

        private CategoryRecord SelectedCategory()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as CategoryRecord;
        }

        private void UpdateSelection()
        {
            CategoryRecord category = SelectedCategory();
            editButton.Enabled = category != null;
            toggleButton.Enabled = category != null;
            toggleButton.Text = category != null && category.IsActive ? "تعطيل" : "تفعيل";
        }

        private void RefreshCategories()
        {
            try
            {
                List<CategoryRecord> categories = CategoriesService.Load(user);
                grid.DataSource = categories;
                status.Text = "عدد التصنيفات: " + categories.Count;
                UpdateSelection();
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل التصنيفات من قاعدة البيانات.");
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

        private void OpenEditor(CategoryRecord category)
        {
            try
            {
                List<CategoryRecord> categories = CategoriesService.Load(user);
                using (var editor = new CategoryEditorForm(category, categories))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    CategoriesService.Save(
                        user,
                        category == null ? (int?)null : category.Id,
                        editor.CategoryName,
                        editor.ParentId);
                    RefreshCategories();
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
                ShowError(error, "تعذر حفظ التصنيف في قاعدة البيانات.");
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            CategoryRecord category = SelectedCategory();
            if (category == null)
            {
                return;
            }

            try
            {
                CategoriesService.SetActive(user, category.Id, !category.IsActive);
                RefreshCategories();
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
                ShowError(error, "تعذر تغيير حالة التصنيف.");
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

    internal sealed class CategoryEditorForm : Form
    {
        private readonly TextBox nameTextBox;
        private readonly ComboBox parentComboBox;

        internal string CategoryName
        {
            get { return nameTextBox.Text; }
        }

        internal int? ParentId
        {
            get
            {
                var selected = parentComboBox.SelectedItem as ParentChoice;
                return selected == null || selected.Id == 0 ? (int?)null : selected.Id;
            }
        }

        internal CategoryEditorForm(CategoryRecord current, IList<CategoryRecord> categories)
        {
            Text = current == null ? "إضافة تصنيف" : "تعديل التصنيف";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 230);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            Controls.Add(new Label
            {
                Text = "اسم التصنيف",
                AutoSize = true,
                Location = new Point(345, 35)
            });
            nameTextBox = new TextBox
            {
                Location = new Point(30, 30),
                Width = 295,
                MaxLength = 150,
                Text = current == null ? string.Empty : current.Name
            };
            Controls.Add(nameTextBox);

            Controls.Add(new Label
            {
                Text = "التصنيف الرئيسي",
                AutoSize = true,
                Location = new Point(345, 90)
            });
            var choices = new List<ParentChoice> { new ParentChoice(0, "بدون تصنيف رئيسي") };
            foreach (CategoryRecord category in categories)
            {
                if ((category.IsActive ||
                     (current != null && current.ParentId == category.Id)) &&
                    (current == null || category.Id != current.Id))
                {
                    choices.Add(new ParentChoice(category.Id, category.Name));
                }
            }

            parentComboBox = new ComboBox
            {
                Location = new Point(30, 85),
                Width = 295,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = choices,
                DisplayMember = "Name"
            };
            Controls.Add(parentComboBox);
            if (current != null)
            {
                foreach (ParentChoice choice in choices)
                {
                    if (current.ParentId.GetValueOrDefault() == choice.Id)
                    {
                        parentComboBox.SelectedItem = choice;
                        break;
                    }
                }
            }

            var save = new Button
            {
                Text = "حفظ",
                Location = new Point(245, 155),
                Width = 90,
                Height = 35
            };
            save.Click += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show("أدخل اسم التصنيف.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nameTextBox.Focus();
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(140, 155),
                Width = 90,
                Height = 35,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private sealed class ParentChoice
        {
            internal ParentChoice(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; private set; }
            public string Name { get; private set; }
        }
    }
}
