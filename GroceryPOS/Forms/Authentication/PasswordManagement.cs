using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GroceryPOS
{
    public partial class PasswordManagementForm : Form
    {
        private readonly AuthenticatedUser user;
        private readonly int? targetUserId;
        private readonly bool forcedChange;

        public PasswordManagementForm()
        {
            InitializeComponent();
        }

        internal PasswordManagementForm(AuthenticatedUser authenticatedUser, bool requireChange)
            : this()
        {
            user = authenticatedUser;
            forcedChange = requireChange;
            ConfigureMode(null);
            saveButton.Click += SaveButton_Click;
        }

        internal PasswordManagementForm(
            AuthenticatedUser administrator,
            int targetId,
            string targetName)
            : this()
        {
            user = administrator;
            targetUserId = targetId;
            ConfigureMode(targetName);
            saveButton.Click += SaveButton_Click;
        }

        private void ConfigureMode(string targetName)
        {
            bool isReset = targetUserId.HasValue;
            Text = isReset ? "إعادة تعيين كلمة المرور" : "تغيير كلمة المرور";
            ClientSize = new Size(520, isReset ? 330 : 390);

            instructionLabel.Text = isReset
                ? "أدخل كلمة مرور مؤقتة للمستخدم " + targetName + ". سيُطلب منه تغييرها عند الدخول."
                : forcedChange
                    ? "يجب تغيير كلمة المرور المؤقتة قبل متابعة استخدام النظام."
                    : "أدخل كلمة المرور الحالية ثم كلمة المرور الجديدة.";
            currentPasswordLabel.Visible = !isReset;
            currentPasswordTextBox.Visible = !isReset;
            newPasswordLabel.Location = new Point(235, isReset ? 74 : 129);
            newPasswordTextBox.Location = new Point(35, isReset ? 70 : 125);
            confirmPasswordLabel.Location = new Point(370, isReset ? 129 : 184);
            confirmPasswordTextBox.Location = new Point(35, isReset ? 125 : 180);
            saveButton.Text = isReset ? "إعادة التعيين" : "حفظ";
            saveButton.Location = new Point(295, isReset ? 195 : 250);
            cancelButton.Location = new Point(180, isReset ? 195 : 250);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!string.Equals(
                    newPasswordTextBox.Text,
                    confirmPasswordTextBox.Text,
                    StringComparison.Ordinal))
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                confirmPasswordTextBox.Clear();
                confirmPasswordTextBox.Focus();
                return;
            }

            try
            {
                if (targetUserId.HasValue)
                {
                    PasswordService.ResetPassword(user, targetUserId.Value, newPasswordTextBox.Text);
                }
                else
                {
                    PasswordService.ChangeOwnPassword(
                        user,
                        currentPasswordTextBox.Text,
                        newPasswordTextBox.Text);
                }

                currentPasswordTextBox?.Clear();
                newPasswordTextBox.Clear();
                confirmPasswordTextBox.Clear();
                MessageBox.Show(
                    targetUserId.HasValue
                        ? "تمت إعادة تعيين كلمة المرور. سيُطلب من المستخدم تغييرها عند الدخول التالي."
                        : "تم تغيير كلمة المرور بنجاح.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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
                ShowDatabaseError(error);
            }
        }

        private void ShowDatabaseError(Exception error)
        {
            string message = "تعذر حفظ تغيير كلمة المرور بسبب مشكلة في قاعدة البيانات.";
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

    internal static class PasswordService
    {
        internal static void ChangeOwnPassword(
            AuthenticatedUser user,
            string currentPassword,
            string newPassword)
        {
            if (user == null)
            {
                throw new UnauthorizedAccessException("يجب تسجيل الدخول لتغيير كلمة المرور.");
            }

            ValidateNewPassword(newPassword);
            if (string.IsNullOrEmpty(currentPassword) || currentPassword.Length > 128)
            {
                throw new ArgumentException("أدخل كلمة المرور الحالية.");
            }

            byte[] newSalt = PasswordHasher.CreateSalt();
            byte[] newHash = PasswordHasher.Hash(newPassword, newSalt);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                byte[] currentSalt;
                byte[] currentHash;
                bool isActive;
                using (var command = new SqlCommand(
                    @"SELECT PasswordSalt, PasswordHash, IsActive
                      FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
                      WHERE Id = @UserId AND Username <> N'system';",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("حساب المستخدم غير موجود.");
                        }

                        currentSalt = (byte[])reader[0];
                        currentHash = (byte[])reader[1];
                        isActive = reader.GetBoolean(2);
                    }
                }

                if (!isActive)
                {
                    throw new UnauthorizedAccessException("الحساب غير نشط.");
                }

                if (!PasswordHasher.Verify(currentPassword, currentSalt, currentHash))
                {
                    throw new InvalidOperationException("كلمة المرور الحالية غير صحيحة.");
                }

                if (PasswordHasher.Verify(newPassword, currentSalt, currentHash))
                {
                    throw new ArgumentException("اختر كلمة مرور مختلفة عن الحالية.");
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Users
                      SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt,
                          MustChangePassword = 0, FailedAttempts = 0, LockedUntil = NULL,
                          UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                      WHERE Id = @UserId AND IsActive = 1;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 128).Value = newHash;
                    command.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 64).Value = newSalt;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = user.Id;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تعذر حفظ كلمة المرور الجديدة.");
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    user.Id,
                    "PasswordChanged",
                    "{\"PasswordChanged\":false}",
                    "{\"PasswordChanged\":true,\"MustChangePassword\":false}");
                transaction.Commit();
            }

            user.MarkPasswordChanged();
        }

        internal static void ResetPassword(
            AuthenticatedUser administrator,
            int targetUserId,
            string temporaryPassword)
        {
            RequireManagePermission(administrator);
            if (targetUserId == administrator.Id)
            {
                throw new InvalidOperationException("استخدم تغيير كلمة المرور لتحديث كلمة مرور حسابك.");
            }

            ValidateNewPassword(temporaryPassword);
            byte[] salt = PasswordHasher.CreateSalt();
            byte[] hash = PasswordHasher.Hash(temporaryPassword, salt);

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                bool previousMustChange;
                using (var command = new SqlCommand(
                    @"SELECT MustChangePassword
                      FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
                      WHERE Id = @UserId AND Username <> N'system';",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = targetUserId;
                    object result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("المستخدم المطلوب غير موجود.");
                    }

                    previousMustChange = Convert.ToBoolean(result);
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Users
                      SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt,
                          MustChangePassword = 1, FailedAttempts = 0, LockedUntil = NULL,
                          UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                      WHERE Id = @UserId AND Username <> N'system';",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 128).Value = hash;
                    command.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 64).Value = salt;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = administrator.Id;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = targetUserId;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تعذر إعادة تعيين كلمة المرور.");
                    }
                }

                InsertAudit(
                    connection,
                    transaction,
                    administrator.Id,
                    targetUserId,
                    "PasswordReset",
                    "{\"MustChangePassword\":" + (previousMustChange ? "true" : "false") + "}",
                    "{\"PasswordReset\":true,\"MustChangePassword\":true}");
                transaction.Commit();
            }
        }

        private static void ValidateNewPassword(string password)
        {
            if (password == null || password.Length < 12 || password.Length > 128)
            {
                throw new ArgumentException("كلمة المرور يجب أن تكون من 12 إلى 128 حرفًا.");
            }
        }

        private static void RequireManagePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Users.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إعادة تعيين كلمات المرور.");
            }
        }

        private static void InsertAudit(
            SqlConnection connection,
            SqlTransaction transaction,
            int actorId,
            int targetId,
            string action,
            string oldValues,
            string newValues)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, OldValues, NewValues)
                  VALUES
                    (@ActorId, @Action, N'Users', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@ActorId", SqlDbType.Int).Value = actorId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = action;
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    targetId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value = oldValues;
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }
    }
}
