using System;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SupplierEditorForm : Form
    {
        public SupplierEditorForm()
        {
            InitializeComponent();
        }

        internal string SupplierName { get { return nameTextBox.Text; } }
        internal string Phone { get { return phoneTextBox.Text; } }
        internal string Address { get { return addressTextBox.Text; } }
        internal string Notes { get { return notesTextBox.Text; } }

        internal SupplierEditorForm(SupplierRecord current)
            : this()
        {
            Text = current == null ? "إضافة مورد" : "تعديل مورد";
            if (current != null)
            {
                nameTextBox.Text = current.Name;
                phoneTextBox.Text = current.Phone;
                addressTextBox.Text = current.Address;
                notesTextBox.Text = current.Notes;
            }

            saveButton.Click += SaveButton_Click;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("اسم المورد مطلوب.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nameTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
