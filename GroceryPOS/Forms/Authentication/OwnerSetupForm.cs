using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class OwnerSetupForm : Form
    {
        internal AuthenticatedUser AuthenticatedUser { get; private set; }

        public OwnerSetupForm()
        {
            InitializeComponent();
            createButton.Click += CreateButton_Click;
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
