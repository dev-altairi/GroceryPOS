using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class CategoryEditorForm : Form
    {
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

        public CategoryEditorForm()
        {
            InitializeComponent();
        }

        internal CategoryEditorForm(CategoryRecord current, IList<CategoryRecord> categories)
            : this()
        {
            Text = current == null ? "إضافة تصنيف" : "تعديل التصنيف";
            nameTextBox.Text = current == null ? string.Empty : current.Name;

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

            parentComboBox.DataSource = choices;
            parentComboBox.DisplayMember = "Name";
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
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("أدخل اسم التصنيف.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
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
