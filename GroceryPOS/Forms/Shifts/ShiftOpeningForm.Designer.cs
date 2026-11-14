namespace GroceryPOS
{
    public partial class ShiftOpeningForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyCodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn amountColumn;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grid = new System.Windows.Forms.DataGridView();
            this.currencyCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.currencyNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.amountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoGenerateColumns = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.currencyCodeColumn.DataPropertyName = "CurrencyCode";
            this.currencyCodeColumn.FillWeight = 55F;
            this.currencyCodeColumn.HeaderText = "العملة";
            this.currencyCodeColumn.Name = "CurrencyCode";
            this.currencyCodeColumn.ReadOnly = true;
            this.currencyNameColumn.DataPropertyName = "CurrencyName";
            this.currencyNameColumn.FillWeight = 120F;
            this.currencyNameColumn.HeaderText = "الوصف";
            this.currencyNameColumn.Name = "CurrencyName";
            this.currencyNameColumn.ReadOnly = true;
            this.amountColumn.DataPropertyName = "Amount";
            this.amountColumn.DefaultCellStyle.Format = "N4";
            this.amountColumn.FillWeight = 80F;
            this.amountColumn.HeaderText = "رصيد البداية";
            this.amountColumn.Name = "Amount";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.currencyCodeColumn,
                this.currencyNameColumn,
                this.amountColumn
            });
            this.grid.Anchor = System.Windows.Forms.AnchorStyles.Top |
                               System.Windows.Forms.AnchorStyles.Bottom |
                               System.Windows.Forms.AnchorStyles.Left |
                               System.Windows.Forms.AnchorStyles.Right;
            this.grid.Location = new System.Drawing.Point(20, 20);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grid.Size = new System.Drawing.Size(520, 285);
            //
            // saveButton
            //
            this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                     System.Windows.Forms.AnchorStyles.Right;
            this.saveButton.Location = new System.Drawing.Point(300, 320);
            this.saveButton.Size = new System.Drawing.Size(95, 34);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                       System.Windows.Forms.AnchorStyles.Right;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(185, 320);
            this.cancelButton.Size = new System.Drawing.Size(95, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // ShiftOpeningForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(560, 390);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(500, 330);
            this.Name = "ShiftOpeningForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "فتح وردية";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
