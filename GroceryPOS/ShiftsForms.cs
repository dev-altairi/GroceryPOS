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
    internal sealed class ShiftManagementForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly DataGridView grid;
        private readonly Button openButton;
        private readonly Button movementButton;
        private readonly Button closeButton;
        private readonly Label status;

        internal ShiftManagementForm(AuthenticatedUser authenticatedUser)
        {
            user = authenticatedUser;
            Text = "الورديات والصندوق";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 500);
            MinimumSize = new Size(720, 380);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(8)
            };
            openButton = CreateButton("فتح وردية");
            openButton.Visible = user.HasPermission("Shifts.Operate");
            openButton.Click += OpenButton_Click;
            toolbar.Controls.Add(openButton);

            movementButton = CreateButton("حركة صندوق");
            movementButton.Visible = user.HasPermission("Shifts.CashMovement");
            movementButton.Enabled = false;
            movementButton.Click += MovementButton_Click;
            toolbar.Controls.Add(movementButton);

            closeButton = CreateButton("إغلاق الوردية");
            closeButton.Visible = user.HasPermission("Shifts.Operate");
            closeButton.Enabled = false;
            closeButton.Click += CloseButton_Click;
            toolbar.Controls.Add(closeButton);

            var refreshButton = CreateButton("تحديث");
            refreshButton.Click += (sender, args) => RefreshShifts();
            toolbar.Controls.Add(refreshButton);
            var closeWindowButton = CreateButton("إغلاق");
            closeWindowButton.Click += (sender, args) => Close();
            toolbar.Controls.Add(closeWindowButton);

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
                DataPropertyName = "Id",
                Visible = false
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CashierId",
                Visible = false
            });
            AddColumn("CashierName", "الكاشير", 100);
            AddColumn("DeviceName", "الجهاز", 80);
            AddColumn("OpenedAt", "وقت الفتح", 90, "g");
            grid.SelectionChanged += (sender, args) => UpdateSelection();

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
            Shown += (sender, args) => RefreshShifts();
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 120,
                Height = 32,
                Margin = new Padding(4)
            };
        }

        private void AddColumn(string property, string title, float weight, string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
        }

        private ShiftRecord SelectedShift()
        {
            return grid.CurrentRow == null
                ? null
                : grid.CurrentRow.DataBoundItem as ShiftRecord;
        }

        private void UpdateSelection()
        {
            ShiftRecord shift = SelectedShift();
            bool selected = shift != null;
            bool ownShift = selected && shift.CashierId == user.Id;
            movementButton.Enabled = ownShift && user.HasPermission("Shifts.CashMovement");
            closeButton.Enabled = selected &&
                                  (ownShift || user.HasPermission("Shifts.ApproveDifference"));
            openButton.Enabled = !((grid.DataSource as List<ShiftRecord> ?? new List<ShiftRecord>())
                .Any(item => item.CashierId == user.Id));
        }

        private void RefreshShifts()
        {
            try
            {
                List<ShiftRecord> shifts = ShiftsService.LoadOpenShifts(user);
                grid.DataSource = shifts;
                status.Text = "الورديات المفتوحة: " + shifts.Count.ToString(CultureInfo.CurrentCulture);
                UpdateSelection();
            }
            catch (SqlException error)
            {
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تحميل الورديات.");
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

        private void OpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<ShiftCurrency> currencies = ShiftsService.LoadActiveCurrencies(user);
                using (var editor = new ShiftOpeningForm(currencies))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        ShiftsService.OpenShift(user, editor.OpeningAmounts);
                        RefreshShifts();
                    }
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
                PurchasingUiErrors.ShowLogged(this, error, "تعذر فتح الوردية.");
            }
        }

        private void MovementButton_Click(object sender, EventArgs e)
        {
            ShiftRecord shift = SelectedShift();
            if (shift == null)
            {
                return;
            }

            try
            {
                List<ShiftCurrency> currencies = ShiftsService.LoadActiveCurrencies(user);
                using (var editor = new CashMovementForm(currencies))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        ShiftsService.AddCashMovement(
                            user,
                            shift.Id,
                            editor.CurrencyId,
                            editor.IsCashIn,
                            editor.Amount,
                            editor.Reason);
                        RefreshShifts();
                    }
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
                PurchasingUiErrors.ShowLogged(this, error, "تعذر تسجيل حركة الصندوق.");
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ShiftRecord shift = SelectedShift();
            if (shift == null)
            {
                return;
            }

            try
            {
                IList<ShiftCashAmount> expected = ShiftsService.LoadExpectedCash(user, shift.Id);
                using (var editor = new ShiftClosingForm(shift, expected))
                {
                    if (editor.ShowDialog(this) == DialogResult.OK)
                    {
                        ShiftsService.CloseShift(
                            user,
                            shift.Id,
                            editor.ClosingAmounts,
                            editor.CloseNotes);
                        MessageBox.Show(
                            "تم إغلاق الوردية وحفظ العدّ والمطابقة.",
                            Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        RefreshShifts();
                    }
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
                PurchasingUiErrors.ShowLogged(this, error, "تعذر إغلاق الوردية.");
            }
        }
    }

    internal sealed class ShiftOpeningRow
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public decimal Amount { get; set; }
    }

    internal sealed class ShiftOpeningForm : Form
    {
        private readonly DataGridView grid;
        private readonly BindingList<ShiftOpeningRow> rows;

        internal IList<ShiftOpeningAmount> OpeningAmounts
        {
            get
            {
                return rows.Select(row => new ShiftOpeningAmount
                {
                    CurrencyId = row.CurrencyId,
                    Amount = row.Amount
                }).ToList();
            }
        }

        internal ShiftOpeningForm(IList<ShiftCurrency> currencies)
        {
            Text = "فتح وردية";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(560, 390);
            MinimumSize = new Size(500, 330);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);
            rows = new BindingList<ShiftOpeningRow>(currencies.Select(currency => new ShiftOpeningRow
            {
                CurrencyId = currency.Id,
                CurrencyCode = currency.Code,
                CurrencyName = currency.Name,
                Amount = 0m
            }).ToList());
            grid = ShiftFormHelpers.CreateGrid(rows, new Point(20, 20), new Size(520, 285));
            ShiftFormHelpers.AddColumn(grid, "CurrencyCode", "العملة", 55, true);
            ShiftFormHelpers.AddColumn(grid, "CurrencyName", "الوصف", 120, true);
            ShiftFormHelpers.AddColumn(grid, "Amount", "رصيد البداية", 80, false, "N4");
            Controls.Add(grid);
            ShiftFormHelpers.AddButtons(this, Save_Click, 320);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                grid.EndEdit();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                ShowInputError(error);
            }
        }

        private void ShowInputError(Exception error)
        {
            MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    internal sealed class CashMovementForm : Form
    {
        private readonly ComboBox currencyComboBox;
        private readonly RadioButton cashInRadio;
        private readonly NumericUpDown amountInput;
        private readonly TextBox reasonInput;

        internal int CurrencyId
        {
            get
            {
                ShiftCurrency currency = currencyComboBox.SelectedItem as ShiftCurrency;
                return currency == null ? 0 : currency.Id;
            }
        }

        internal bool IsCashIn { get { return cashInRadio.Checked; } }
        internal decimal Amount { get { return amountInput.Value; } }
        internal string Reason { get { return reasonInput.Text; } }

        internal CashMovementForm(IList<ShiftCurrency> currencies)
        {
            Text = "حركة صندوق";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(500, 330);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            currencyComboBox = AddCombo("العملة", 30, currencies);
            cashInRadio = new RadioButton
            {
                Text = "إيداع",
                Checked = true,
                AutoSize = true,
                Location = new Point(355, 85)
            };
            var cashOutRadio = new RadioButton
            {
                Text = "سحب",
                AutoSize = true,
                Location = new Point(245, 85)
            };
            Controls.Add(cashInRadio);
            Controls.Add(cashOutRadio);
            Controls.Add(new Label
            {
                Text = "المبلغ",
                AutoSize = true,
                Location = new Point(390, 135)
            });
            amountInput = new NumericUpDown
            {
                Location = new Point(40, 130),
                Width = 330,
                DecimalPlaces = 4,
                Increment = 0.0001m,
                Maximum = 99999999999999.9999m,
                TextAlign = HorizontalAlignment.Right
            };
            Controls.Add(amountInput);
            Controls.Add(new Label
            {
                Text = "السبب",
                AutoSize = true,
                Location = new Point(390, 185)
            });
            reasonInput = new TextBox
            {
                Location = new Point(40, 180),
                Width = 330,
                MaxLength = 300
            };
            Controls.Add(reasonInput);

            var save = new Button
            {
                Text = "حفظ",
                Location = new Point(280, 255),
                Width = 90,
                Height = 34
            };
            save.Click += (sender, args) =>
            {
                if (CurrencyId <= 0 || Amount <= 0 || string.IsNullOrWhiteSpace(Reason))
                {
                    MessageBox.Show("اختر العملة وأدخل مبلغًا موجبًا وسبب الحركة.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(save);
            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(170, 255),
                Width = 90,
                Height = 34,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => Close();
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private ComboBox AddCombo(string label, int top, IList<ShiftCurrency> currencies)
        {
            Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Location = new Point(390, top + 4)
            });
            var combo = new ComboBox
            {
                Location = new Point(40, top),
                Width = 330,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = new List<ShiftCurrency>(currencies)
            };
            Controls.Add(combo);
            return combo;
        }
    }

    internal sealed class ShiftClosingRow
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public string DifferenceReason { get; set; }
    }

    internal sealed class ShiftClosingForm : Form
    {
        private readonly DataGridView grid;
        private readonly BindingList<ShiftClosingRow> rows;
        private readonly TextBox notesInput;

        internal IList<ShiftClosingAmount> ClosingAmounts
        {
            get
            {
                return rows.Select(row => new ShiftClosingAmount
                {
                    CurrencyId = row.CurrencyId,
                    ActualAmount = row.ActualAmount,
                    DifferenceReason = row.DifferenceReason
                }).ToList();
            }
        }

        internal string CloseNotes { get { return notesInput.Text; } }

        internal ShiftClosingForm(ShiftRecord shift, IList<ShiftCashAmount> expected)
        {
            Text = "إغلاق الوردية — " + shift.CashierName;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(930, 560);
            MinimumSize = new Size(820, 470);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);
            rows = new BindingList<ShiftClosingRow>(expected.Select(amount => new ShiftClosingRow
            {
                CurrencyId = amount.CurrencyId,
                CurrencyCode = amount.CurrencyCode,
                CurrencyName = amount.CurrencyName,
                ExpectedAmount = amount.ExpectedAmount,
                ActualAmount = amount.ExpectedAmount,
                DifferenceReason = string.Empty
            }).ToList());
            grid = ShiftFormHelpers.CreateGrid(rows, new Point(20, 20), new Size(890, 360));
            ShiftFormHelpers.AddColumn(grid, "CurrencyCode", "العملة", 45, true);
            ShiftFormHelpers.AddColumn(grid, "CurrencyName", "الوصف", 100, true);
            ShiftFormHelpers.AddColumn(grid, "ExpectedAmount", "المتوقع", 75, true, "N4");
            ShiftFormHelpers.AddColumn(grid, "ActualAmount", "الفعلي", 75, false, "N4");
            ShiftFormHelpers.AddColumn(grid, "DifferenceReason", "سبب الفرق", 125, false);
            Controls.Add(grid);

            Controls.Add(new Label
            {
                Text = "ملاحظات الإغلاق",
                AutoSize = true,
                Location = new Point(800, 405)
            });
            notesInput = new TextBox
            {
                Location = new Point(25, 400),
                Width = 750,
                MaxLength = 500
            };
            Controls.Add(notesInput);
            ShiftFormHelpers.AddButtons(this, Save_Click, 465);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                grid.EndEdit();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    internal static class ShiftFormHelpers
    {
        internal static DataGridView CreateGrid<T>(
            BindingList<T> rows,
            Point location,
            Size size)
        {
            var grid = new DataGridView
            {
                Location = location,
                Size = size,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White,
                DataSource = rows
            };
            grid.DataError += (sender, args) =>
            {
                if (args.Exception != null)
                {
                    args.ThrowException = false;
                    PurchasingUiErrors.ShowLogged(
                        grid,
                        args.Exception,
                        "القيمة المدخلة غير صالحة. راجع المبلغ والسبب.");
                }
            };
            return grid;
        }

        internal static void AddColumn(
            DataGridView grid,
            string property,
            string title,
            float weight,
            bool readOnly,
            string format = null)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight,
                ReadOnly = readOnly
            };
            if (format != null)
            {
                column.DefaultCellStyle.Format = format;
            }

            grid.Columns.Add(column);
        }

        internal static void AddButtons(Form form, EventHandler saveHandler, int top)
        {
            var save = new Button
            {
                Text = "حفظ",
                Location = new Point(300, top),
                Width = 95,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            save.Click += saveHandler;
            form.Controls.Add(save);

            var cancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(185, top),
                Width = 95,
                Height = 34,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel
            };
            cancel.Click += (sender, args) => form.Close();
            form.Controls.Add(cancel);
            form.AcceptButton = save;
            form.CancelButton = cancel;
        }
    }

}
