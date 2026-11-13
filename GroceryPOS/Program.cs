using System;
using System.Configuration;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace GroceryPOS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                Form entryForm;
                try
                {
                    entryForm = AuthService.IsOwnerSetupRequired()
                        ? (Form)new OwnerSetupForm()
                        : new LoginForm();
                }
                catch (SqlException error)
                {
                    ShowStartupError(error);
                    return;
                }
                catch (System.InvalidOperationException error)
                {
                    ShowStartupError(error);
                    return;
                }
                catch (ConfigurationErrorsException error)
                {
                    ShowStartupError(error);
                    return;
                }

                AuthenticatedUser user;
                using (entryForm)
                {
                    if (entryForm.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    user = entryForm is OwnerSetupForm
                        ? ((OwnerSetupForm)entryForm).AuthenticatedUser
                        : ((LoginForm)entryForm).AuthenticatedUser;
                }

                if (user.MustChangePassword)
                {
                    using (var passwordForm = new PasswordManagementForm(user, true))
                    {
                        if (passwordForm.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                    }
                }

                using (var dashboard = new DashboardForm(user))
                {
                    if (dashboard.ShowDialog() != DialogResult.Retry)
                    {
                        return;
                    }
                }
            }
        }

        private static void ShowStartupError(System.Exception error)
        {
            string message = "تعذر الاتصال بقاعدة البيانات أو قراءة إعداداتها.";
            try
            {
                AppLogger.Write(error);
                message += Environment.NewLine + "حُفظت التفاصيل في سجل التطبيق.";
            }
            catch (IOException)
            {
                message += Environment.NewLine + "تعذر حفظ سجل الخطأ.";
            }
            catch (System.UnauthorizedAccessException)
            {
                message += Environment.NewLine + "تعذر حفظ سجل الخطأ بسبب الأذونات.";
            }

            MessageBox.Show(message, "GroceryPOS", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
