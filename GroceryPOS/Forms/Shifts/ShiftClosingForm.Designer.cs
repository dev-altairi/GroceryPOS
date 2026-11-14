namespace GroceryPOS
{
    public partial class ShiftClosingForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyCodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn currencyNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn expectedAmountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actualAmountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn differenceReasonColumn;
        private System.Windows.Forms.Label notesLabel;
        private System.Windows.Forms.TextBox notesInput;
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
            this.expectedAmountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actualAmountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.differenceReasonColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.notesLabel = new System.Windows.Forms.Label();
            this.notesInput = new System.Windows.Forms.TextBox();
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
            this.currencyCodeColumn.FillWeight = 45F;
            this.currencyCodeColumn.HeaderText = "العملة";
            this.currencyCodeColumn.Name = "CurrencyCode";
            this.currencyCodeColumn.ReadOnly = true;
            this.currencyNameColumn.DataPropertyName = "CurrencyName";
            this.currencyNameColumn.FillWeight = 100F;
            this.currencyNameColumn.HeaderText = "الوصف";
            this.currencyNameColumn.Name = "CurrencyName";
            this.currencyNameColumn.ReadOnly = true;
            this.expectedAmountColumn.DataPropertyName = "ExpectedAmount";
            this.expectedAmountColumn.DefaultCellStyle.Format = "N4";
            this.expectedAmountColumn.FillWeight = 75F;
            this.expectedAmountColumn.HeaderText = "المتوقع";
            this.expectedAmountColumn.Name = "ExpectedAmount";
            this.expectedAmountColumn.ReadOnly = true;
            this.actualAmountColumn.DataPropertyName = "ActualAmount";
            this.actualAmountColumn.DefaultCellStyle.Format = "N4";
            this.actualAmountColumn.FillWeight = 75F;
            this.actualAmountColumn.HeaderText = "الفعلي";
            this.actualAmountColumn.Name = "ActualAmount";
            this.differenceReasonColumn.DataPropertyName = "DifferenceReason";
            this.differenceReasonColumn.FillWeight = 125F;
            this.differenceReasonColumn.HeaderText = "سبب الفرق";
            this.differenceReasonColumn.Name = "DifferenceReason";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.currencyCodeColumn,
                this.currencyNameColumn,
                this.expectedAmountColumn,
                this.actualAmountColumn,
                this.differenceReasonColumn
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
            this.grid.Size = new System.Drawing.Size(890, 360);
            //
            // notesLabel
            //
            this.notesLabel.AutoSize = true;
            this.notesLabel.Location = new System.Drawing.Point(800, 405);
            this.notesLabel.Text = "ملاحظات الإغلاق";
            //
            // notesInput
            //
            this.notesInput.Location = new System.Drawing.Point(25, 400);
            this.notesInput.MaxLength = 500;
            this.notesInput.Size = new System.Drawing.Size(750, 25);
            //
            // saveButton
            //
            this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                     System.Windows.Forms.AnchorStyles.Right;
            this.saveButton.Location = new System.Drawing.Point(300, 465);
            this.saveButton.Size = new System.Drawing.Size(95, 34);
            this.saveButton.Text = "حفظ";
            this.saveButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom |
                                       System.Windows.Forms.AnchorStyles.Right;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(185, 465);
            this.cancelButton.Size = new System.Drawing.Size(95, 34);
            this.cancelButton.Text = "إلغاء";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // ShiftClosingForm
            //
            this.AcceptButton = this.saveButton;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(930, 560);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.notesLabel);
            this.Controls.Add(this.notesInput);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.MinimumSize = new System.Drawing.Size(820, 470);
            this.Name = "ShiftClosingForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "إغلاق الوردية";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
