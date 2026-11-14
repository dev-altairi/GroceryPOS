using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class RoleEditorForm : Form
    {
        public RoleEditorForm()
        {
            InitializeComponent();
        }

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
            : this()
        {
            Text = current == null ? "إضافة دور" : "تعديل الدور";
            nameTextBox.Text = current == null ? string.Empty : current.Name;
            nameTextBox.ReadOnly = current != null && current.IsSystem;
            descriptionTextBox.Text = current == null ? string.Empty : current.Description;
            foreach (PermissionOption permission in permissions)
            {
                bool selected = current != null && current.PermissionIds.Contains(permission.Id);
                permissionsListBox.Items.Add(permission, selected);
            }

            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, EventArgs e)
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
        }
    }
}
