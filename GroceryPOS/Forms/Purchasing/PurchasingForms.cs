using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class SupplierManagementForm : Form
    {
        private AuthenticatedUser user;
        private Timer searchTimer;

        internal SupplierManagementForm(AuthenticatedUser authenticatedUser)
            : this()
        {
            user = authenticatedUser;
            addButton.Visible = user.HasPermission("Suppliers.Manage");
            addButton.Click += (sender, args) => OpenEditor(null);
            editButton.Visible = user.HasPermission("Suppliers.Manage");
            editButton.Click += (sender, args) =>
            {
                SupplierRecord selected = SelectedSupplier();
                if (selected != null)
                {
                    OpenEditor(selected);
                }
            };
            toggleButton.Visible = user.HasPermission("Suppliers.Manage");
            toggleButton.Click += ToggleButton_Click;
            searchTimer = new Timer { Interval = 300 };
            searchTimer.Tick += (sender, args) =>
            {
                searchTimer.Stop();
                RefreshSuppliers();
            };
            searchTextBox.TextChanged += (sender, args) =>
            {
                searchTimer.Stop();
                searchTimer.Start();
            };
            refreshButton.Click += (sender, args) => RefreshSuppliers();
            closeButton.Click += (sender, args) => Close();
            grid.SelectionChanged += (sender, args) => UpdateSelection();
            grid.CellDoubleClick += (sender, args) =>
            {
                if (args.RowIndex >= 0 && user.HasPermission("Suppliers.Manage"))
                {
                    SupplierRecord supplier = SelectedSupplier();
                    if (supplier != null)
                    {
                        OpenEditor(supplier);
                    }
                }
            };
            Shown += (sender, args) => RefreshSuppliers();
            FormClosed += (sender, args) => searchTimer.Dispose();
        }

        private SupplierRecord SelectedSupplier()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as SupplierRecord;
        }

        private void UpdateSelection()
        {
            SupplierRecord supplier = SelectedSupplier();
            bool canManage = user.HasPermission("Suppliers.Manage");
            editButton.Enabled = canManage && supplier != null;
            toggleButton.Enabled = canManage && supplier != null;
            toggleButton.Text = supplier != null && supplier.IsActive ? "تعطيل" : "تفعيل";
        }

        private void RefreshSuppliers()
        {
            try
            {
                List<SupplierRecord> suppliers = SuppliersService.Search(user, searchTextBox.Text);
                grid.DataSource = suppliers;
                status.Text = "عدد الموردين: " + suppliers.Count.ToString(CultureInfo.CurrentCulture);
                UpdateSelection();
            }
            catch (SqlException error)
            {
                ShowError(error, "تعذر تحميل الموردين من قاعدة البيانات.");
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

        private void OpenEditor(SupplierRecord supplier)
        {
            using (var editor = new SupplierEditorForm(supplier))
            {
                if (editor.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    SuppliersService.Save(user, new SupplierDraft
                    {
                        Id = supplier == null ? (int?)null : supplier.Id,
                        Name = editor.SupplierName,
                        Phone = editor.Phone,
                        Address = editor.Address,
                        Notes = editor.Notes
                    });
                    RefreshSuppliers();
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
                    ShowError(error, "تعذر حفظ المورد.");
                }
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            SupplierRecord supplier = SelectedSupplier();
            if (supplier == null)
            {
                return;
            }

            bool activate = !supplier.IsActive;
            string prompt = activate
                ? "هل تريد تفعيل المورد " + supplier.Name + "؟"
                : "هل تريد تعطيل المورد " + supplier.Name + "؟";
            if (MessageBox.Show(prompt, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SuppliersService.SetActive(user, supplier.Id, activate);
                RefreshSuppliers();
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
                ShowError(error, "تعذر تغيير حالة المورد.");
            }
        }

        private void ShowError(Exception error, string message)
        {
            PurchasingUiErrors.ShowLogged(this, error, message);
        }
    }





    internal static class PurchasingUiErrors
    {
        internal static void ShowLogged(IWin32Window owner, Exception error, string message)
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

            MessageBox.Show(owner, message, "GroceryPOS", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
