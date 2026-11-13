using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    internal sealed class LoginForm : Form
    {
        private readonly TextBox usernameTextBox;
        private readonly TextBox passwordTextBox;

        internal AuthenticatedUser AuthenticatedUser { get; private set; }

        internal LoginForm()
        {
            Text = "تسجيل الدخول - GroceryPOS";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 290);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Segoe UI", 10F);

            var title = new Label
            {
                Text = "نظام إدارة البقالة",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(105, 25)
            };
            Controls.Add(title);

            Controls.Add(new Label { Text = "اسم المستخدم", AutoSize = true, Location = new Point(300, 95) });
            usernameTextBox = new TextBox { Location = new Point(35, 91), Width = 250, MaxLength = 50 };
            Controls.Add(usernameTextBox);

            Controls.Add(new Label { Text = "كلمة المرور", AutoSize = true, Location = new Point(300, 145) });
            passwordTextBox = new TextBox
            {
                Location = new Point(35, 141),
                Width = 250,
                MaxLength = 128,
                UseSystemPasswordChar = true
            };
            Controls.Add(passwordTextBox);

            var loginButton = new Button
            {
                Text = "دخول",
                Location = new Point(145, 205),
                Width = 130,
                Height = 38
            };
            loginButton.Click += LoginButton_Click;
            Controls.Add(loginButton);
            AcceptButton = loginButton;
            var closeButton = new Button
            {
                Text = "إغلاق",
                Location = new Point(35, 205),
                Width = 85,
                Height = 38,
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(closeButton);
            CancelButton = closeButton;
            Shown += (sender, args) => usernameTextBox.Focus();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || passwordTextBox.Text.Length == 0)
            {
                MessageBox.Show("أدخل اسم المستخدم وكلمة المرور.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                AuthenticationResult result = AuthService.Authenticate(
                    usernameTextBox.Text,
                    passwordTextBox.Text);
                if (!result.Succeeded)
                {
                    passwordTextBox.Clear();
                    passwordTextBox.Focus();
                    MessageBox.Show(
                        "بيانات الدخول غير صحيحة أو الحساب مقفل مؤقتًا.",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                AuthenticatedUser = result.User;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException error)
            {
                ShowDatabaseError(error);
            }
            catch (InvalidOperationException error)
            {
                ShowApplicationError(error);
            }
        }

        private void ShowDatabaseError(SqlException error)
        {
            ShowLoggedError(
                error,
                "تعذر إكمال تسجيل الدخول بسبب مشكلة في قاعدة البيانات.");
        }

        private void ShowApplicationError(Exception error)
        {
            ShowLoggedError(error, "تعذر إكمال تسجيل الدخول بسبب إعداد غير صالح.");
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
