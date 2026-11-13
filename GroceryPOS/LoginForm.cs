using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class LoginForm : Form
    {
        internal AuthenticatedUser AuthenticatedUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            loginButton.Click += LoginButton_Click;
            Shown += LoginForm_Shown;
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            usernameTextBox.Focus();
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
