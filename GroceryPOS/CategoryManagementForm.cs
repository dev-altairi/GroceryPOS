using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class CategoryManagementForm : Form
    {
        private AuthenticatedUser user;

        public CategoryManagementForm()
        {
            InitializeComponent();
        }

        internal CategoryManagementForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Click += (sender, args) => OpenEditor(null);
            editButton.Click += (sender, args) =>
            {
                CategoryRecord selected = SelectedCategory();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toggleButton.Click += ToggleButton_Click;
            closeButton.Click += (sender, args) => Close();
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
            Shown += (sender, args) => RefreshCategories();
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

}
