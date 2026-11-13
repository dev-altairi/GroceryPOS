using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class ProductUnitsForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly ProductRecord product;
        private readonly BindingList<ProductUnitDraft> rows;
        private readonly DataGridView grid;
        private readonly bool canEditPrices;

        internal ProductUnitsForm(
            AuthenticatedUser authenticatedUser,
            ProductRecord productRecord,
            IList<ProductUnitRecord> existingRows,
            IList<CatalogOption> unitChoices)
        {
            user = authenticatedUser;
            product = productRecord;
            canEditPrices = user.HasPermission("Products.EditPrice");
            rows = new BindingList<ProductUnitDraft>();
            foreach (ProductUnitRecord row in existingRows)
            {
                rows.Add(new ProductUnitDraft
                {
                    UnitId = row.UnitId,
                    FactorToBase = row.FactorToBase,
                    Barcode = row.Barcode,
                    RetailPrice = row.RetailPrice,
                    WholesalePrice = row.WholesalePrice,
                    IsDefaultSaleUnit = row.IsDefaultSaleUnit,
                    IsActive = row.IsActive
                });
            }

            Text = "الوحدات والباركود - " + product.Name;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1040, 510);
            MinimumSize = new Size(850, 430);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var note = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(12, 0, 12, 0),
                Text = "معامل التحويل: كم وحدة أساسية يحتويها صف الوحدة. يجب أن تبقى الوحدة الأساسية بمعامل 1."
            };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White,
                DataSource = rows
            };
            var choices = new List<CatalogOption>(unitChoices);
            choices.Insert(0, new CatalogOption { Id = 0, Name = "اختر الوحدة" });
            grid.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = "UnitId",
                DataPropertyName = "UnitId",
                HeaderText = "الوحدة",
                DataSource = choices,
                DisplayMember = "Name",
                ValueMember = "Id",
                FillWeight = 120,
                FlatStyle = FlatStyle.Flat
            });
            AddTextColumn("FactorToBase", "معامل التحويل", 90, "N6");
            AddTextColumn("Barcode", "الباركود", 140);
            AddTextColumn("RetailPrice", "سعر البيع", 95, "N4", canEditPrices);
            AddTextColumn("WholesalePrice", "سعر الجملة", 95, "N4", canEditPrices);
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsDefaultSaleUnit",
                DataPropertyName = "IsDefaultSaleUnit",
                HeaderText = "افتراضية",
                FillWeight = 70
            });
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsActive",
                DataPropertyName = "IsActive",
                HeaderText = "نشطة",
                FillWeight = 60
            });
            grid.CellValueChanged += Grid_CellValueChanged;
            grid.CurrentCellDirtyStateChanged += (sender, args) =>
            {
                if (grid.IsCurrentCellDirty)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.DataError += Grid_DataError;

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8)
            };
            var saveButton = CreateButton("حفظ");
            saveButton.Click += SaveButton_Click;
            footer.Controls.Add(saveButton);
            var cancelButton = CreateButton("إلغاء");
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Click += (sender, args) => Close();
            footer.Controls.Add(cancelButton);
            var addButton = CreateButton("إضافة وحدة");
            addButton.Click += (sender, args) =>
            {
                rows.Add(new ProductUnitDraft
                {
                    UnitId = 0,
                    FactorToBase = 1,
                    RetailPrice = 0,
                    IsActive = true
                });
            };
            footer.Controls.Add(addButton);

            if (!canEditPrices)
            {
                footer.Controls.Add(new Label
                {
                    AutoSize = true,
                    Padding = new Padding(12, 8, 12, 0),
                    Text = "تعديل الأسعار يتطلب إذن Products.EditPrice."
                });
            }

            Controls.Add(grid);
            Controls.Add(footer);
            Controls.Add(note);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 100,
                Height = 32,
                Margin = new Padding(4)
            };
        }

        private void AddTextColumn(
            string property,
            string title,
            float fillWeight,
            string format = null,
            bool editable = true)
        {
            var column = new DataGridViewTextBoxColumn
            {
                Name = property,
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = fillWeight,
                ReadOnly = !editable
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                grid.Columns[e.ColumnIndex].Name != "IsDefaultSaleUnit")
            {
                return;
            }

            ProductUnitDraft selected = grid.Rows[e.RowIndex].DataBoundItem as ProductUnitDraft;
            if (selected == null || !selected.IsDefaultSaleUnit)
            {
                return;
            }

            for (int i = 0; i < rows.Count; i++)
            {
                if (i != e.RowIndex)
                {
                    rows[i].IsDefaultSaleUnit = false;
                }
            }

            grid.Refresh();
        }

        private void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(
                "قيمة الوحدة أو الرقم غير صالحة. راجع الصف وأعد إدخال القيمة.",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            e.Cancel = true;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                grid.EndEdit();
                Validate();
                ProductsUnitSave();
                DialogResult = DialogResult.OK;
                Close();
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
                ShowError(error);
            }
        }

        private void ProductsUnitSave()
        {
            ProductUnitsService.Save(user, product.Id, new List<ProductUnitDraft>(rows));
        }

        private void ShowError(SqlException error)
        {
            string message;
            if (error.Number == 2601 || error.Number == 2627)
            {
                message = "الوحدة أو الباركود مكرر. تأكد أن الباركود غير مستخدم في منتج آخر.";
            }
            else
            {
                message = "تعذر حفظ وحدات المنتج في قاعدة البيانات.";
            }

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
