using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class OwnerSetupForm : Form
    {
        private readonly TextBox fullNameTextBox;
        private readonly TextBox usernameTextBox;
        private readonly TextBox passwordTextBox;
        private readonly TextBox confirmPasswordTextBox;

        internal AuthenticatedUser AuthenticatedUser { get; private set; }

        internal OwnerSetupForm()
        {
            Text = "الإعداد الأولي - GroceryPOS";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 390);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "إنشاء حساب المالك",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(135, 20)
            };
            Controls.Add(title);
            Controls.Add(new Label
            {
                Text = "أنشئ الحساب الأول. كلمة المرور لا تقل عن 12 حرفًا.",
                AutoSize = true,
                Location = new Point(67, 65)
            });

            fullNameTextBox = AddField("الاسم الكامل", 110);
            usernameTextBox = AddField("اسم المستخدم", 160);
            passwordTextBox = AddField("كلمة المرور", 210, true);
            confirmPasswordTextBox = AddField("تأكيد كلمة المرور", 260, true);

            var createButton = new Button
            {
                Text = "إنشاء الحساب",
                Location = new Point(165, 320),
                Width = 150,
                Height = 38
            };
            createButton.Click += CreateButton_Click;
            Controls.Add(createButton);
            AcceptButton = createButton;
        }

        private TextBox AddField(string label, int top, bool password = false)
        {
            Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(355, top + 4) });
            var textBox = new TextBox
            {
                Location = new Point(35, top),
                Width = 290,
                MaxLength = password ? 128 : (label == "الاسم الكامل" ? 150 : 50),
                UseSystemPasswordChar = password
            };
            Controls.Add(textBox);
            return textBox;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            if (!string.Equals(passwordTextBox.Text, confirmPasswordTextBox.Text, StringComparison.Ordinal))
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                confirmPasswordTextBox.Clear();
                confirmPasswordTextBox.Focus();
                return;
            }

            if (passwordTextBox.Text.Length < 12)
            {
                MessageBox.Show("كلمة المرور يجب أن تكون من 12 حرفًا على الأقل.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordTextBox.Focus();
                return;
            }

            try
            {
                AuthenticatedUser = AuthService.CreateFirstOwner(
                    fullNameTextBox.Text,
                    usernameTextBox.Text,
                    passwordTextBox.Text);
                passwordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SqlException error)
            {
                ShowLoggedError(error, "تعذر إنشاء حساب المالك بسبب مشكلة في قاعدة البيانات.");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowLoggedError(Exception error, string message)
        {
            try
            {
                AppLogger.Write(error);
                message += Environment.NewLine + "حُفظت التفاصيل في سجل التطبيق.";
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
