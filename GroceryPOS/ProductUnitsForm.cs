using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class ProductUnitsForm : Form
    {
        private AuthenticatedUser user;
        private ProductRecord product;
        private readonly BindingList<ProductUnitDraft> rows;
        private bool canEditPrices;

        public ProductUnitsForm()
        {
            InitializeComponent();
            rows = new BindingList<ProductUnitDraft>();
        }

        internal ProductUnitsForm(
            AuthenticatedUser authenticatedUser,
            ProductRecord productRecord,
            IList<ProductUnitRecord> existingRows,
            IList<CatalogOption> unitChoices)
            : this()
        {
            user = authenticatedUser;
            product = productRecord;
            canEditPrices = user.HasPermission("Products.EditPrice");
            unitColumn.DataSource = CreateUnitChoices(unitChoices);
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
            grid.DataSource = rows;
            retailPriceColumn.ReadOnly = !canEditPrices;
            wholesalePriceColumn.ReadOnly = !canEditPrices;
            grid.CellValueChanged += Grid_CellValueChanged;
            grid.CurrentCellDirtyStateChanged += (sender, args) =>
            {
                if (grid.IsCurrentCellDirty)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.DataError += Grid_DataError;
            saveButton.Click += SaveButton_Click;
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

            if (!canEditPrices)
            {
                pricePermissionLabel.Visible = true;
            }
        }

        private static List<CatalogOption> CreateUnitChoices(IList<CatalogOption> unitChoices)
        {
            var choices = new List<CatalogOption>(unitChoices);
            choices.Insert(0, new CatalogOption { Id = 0, Name = "اختر الوحدة" });
            return choices;
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
