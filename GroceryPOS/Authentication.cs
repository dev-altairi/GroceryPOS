using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GroceryPOS
{
    internal static class Database
    {
        internal static SqlConnection OpenConnection()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["GroceryPOS"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException("إعداد اتصال قاعدة البيانات غير موجود.");
            }

            var connection = new SqlConnection(settings.ConnectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (SqlException)
            {
                connection.Dispose();
                throw;
            }
            catch (InvalidOperationException)
            {
                connection.Dispose();
                throw;
            }
        }
    }

    internal static class PasswordHasher
    {
        private const int Iterations = 600000;
        private const int SaltSize = 32;
        private const int HashSize = 32;

        internal static byte[] CreateSalt()
        {
            var salt = new byte[SaltSize];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            return salt;
        }

        internal static byte[] Hash(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(
                password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(HashSize);
            }
        }

        internal static bool Verify(string password, byte[] salt, byte[] expectedHash)
        {
            if (salt == null || salt.Length != SaltSize ||
                expectedHash == null || expectedHash.Length != HashSize)
            {
                return false;
            }

            byte[] actualHash = Hash(password, salt);
            int difference = 0;
            for (int i = 0; i < actualHash.Length; i++)
            {
                difference |= actualHash[i] ^ expectedHash[i];
            }

            return difference == 0;
        }
    }

    internal sealed class AuthenticatedUser
    {
        internal AuthenticatedUser(
            int id,
            string fullName,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            bool mustChangePassword)
        {
            Id = id;
            FullName = fullName;
            Roles = new List<string>(roles).AsReadOnly();
            Permissions = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
            MustChangePassword = mustChangePassword;
        }

        internal int Id { get; private set; }
        internal string FullName { get; private set; }
        internal IList<string> Roles { get; private set; }
        internal ISet<string> Permissions { get; private set; }
        internal bool MustChangePassword { get; private set; }

        internal void MarkPasswordChanged()
        {
            MustChangePassword = false;
        }

        internal bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }
    }

    internal sealed class AuthenticationResult
    {
        private AuthenticationResult(bool succeeded, AuthenticatedUser user)
        {
            Succeeded = succeeded;
            User = user;
        }

        internal bool Succeeded { get; private set; }
        internal AuthenticatedUser User { get; private set; }

        internal static AuthenticationResult Success(AuthenticatedUser user)
        {
            return new AuthenticationResult(true, user);
        }

        internal static AuthenticationResult Failure()
        {
            return new AuthenticationResult(false, null);
        }
    }

    internal static class AuthService
    {
        private const string InvalidCredentialsMessage = "بيانات الدخول غير صحيحة أو الحساب مقفل مؤقتًا.";
        private static readonly byte[] DummySalt = PasswordHasher.CreateSalt();
        private static readonly byte[] DummyHash = PasswordHasher.Hash("invalid-user-placeholder", DummySalt);

        internal static bool IsOwnerSetupRequired()
        {
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                "SELECT COUNT_BIG(*) FROM dbo.Users WHERE Username <> N'system';", connection))
            {
                return Convert.ToInt64(command.ExecuteScalar()) == 0;
            }
        }

        internal static AuthenticatedUser CreateFirstOwner(
            string fullName,
            string username,
            string password)
        {
            fullName = (fullName ?? string.Empty).Trim();
            username = (username ?? string.Empty).Trim();
            ValidateOwnerDetails(fullName, username, password);

            byte[] salt = PasswordHasher.CreateSalt();
            byte[] passwordHash = PasswordHasher.Hash(password, salt);
            int userId;

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                if (ExecuteInt(
                    connection,
                    transaction,
                    "SELECT COUNT(*) FROM dbo.Users WITH (UPDLOCK, HOLDLOCK) WHERE Username <> N'system';") != 0)
                {
                    throw new InvalidOperationException("تم إنشاء مستخدم بالفعل. أغلق هذه النافذة وسجّل الدخول.");
                }

                int systemUserId = ExecuteInt(
                    connection,
                    transaction,
                    "SELECT Id FROM dbo.Users WHERE Username = N'system';");
                int ownerRoleId;
                using (var roleCommand = new SqlCommand(
                    "SELECT Id FROM dbo.Roles WHERE Name = @RoleName AND IsSystem = 1 AND IsActive = 1;",
                    connection,
                    transaction))
                {
                    roleCommand.Parameters.Add("@RoleName", SqlDbType.NVarChar, 100).Value = "المالك";
                    object roleId = roleCommand.ExecuteScalar();
                    if (roleId == null)
                    {
                        throw new InvalidOperationException("دور المالك غير موجود في قاعدة البيانات.");
                    }

                    ownerRoleId = Convert.ToInt32(roleId);
                }

                using (var insertUser = new SqlCommand(
                    @"INSERT dbo.Users (Username, FullName, PasswordHash, PasswordSalt, IsActive, CreatedBy)
                      VALUES (@Username, @FullName, @PasswordHash, @PasswordSalt, 1, @CreatedBy);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    connection,
                    transaction))
                {
                    insertUser.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                    insertUser.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                    insertUser.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 128).Value = passwordHash;
                    insertUser.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 64).Value = salt;
                    insertUser.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = systemUserId;
                    userId = Convert.ToInt32(insertUser.ExecuteScalar());
                }

                using (var assignRole = new SqlCommand(
                    @"INSERT dbo.UserRoles (UserId, RoleId, CreatedBy)
                      VALUES (@UserId, @RoleId, @CreatedBy);",
                    connection,
                    transaction))
                {
                    assignRole.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    assignRole.Parameters.Add("@RoleId", SqlDbType.Int).Value = ownerRoleId;
                    assignRole.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = systemUserId;
                    assignRole.ExecuteNonQuery();
                }

                using (var audit = new SqlCommand(
                    @"INSERT dbo.AuditLogs (UserId, Action, EntityName, EntityId, NewValues)
                      VALUES (@UserId, N'FirstOwnerCreated', N'Users', @EntityId, N'{}');",
                    connection,
                    transaction))
                {
                    audit.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                    audit.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value = userId.ToString();
                    audit.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return LoadUser(userId);
        }

        internal static AuthenticationResult Authenticate(string username, string password)
        {
            username = (username ?? string.Empty).Trim();
            password = password ?? string.Empty;

            int userId = 0;
            string fullName = null;
            byte[] salt = null;
            byte[] storedHash = null;
            bool isActive = false;
            bool isLocked = false;

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT Id, FullName, PasswordHash, PasswordSalt, IsActive,
                         CASE WHEN LockedUntil IS NOT NULL AND LockedUntil > SYSDATETIME()
                              THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
                  FROM dbo.Users
                  WHERE Username = @Username;",
                connection))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                        fullName = reader.GetString(1);
                        storedHash = (byte[])reader[2];
                        salt = (byte[])reader[3];
                        isActive = reader.GetBoolean(4);
                        isLocked = reader.GetBoolean(5);
                    }
                }
            }

            if (userId == 0)
            {
                PasswordHasher.Verify(password, DummySalt, DummyHash);
                return AuthenticationResult.Failure();
            }

            bool passwordMatches = PasswordHasher.Verify(password, salt, storedHash);
            if (!isActive || isLocked)
            {
                return AuthenticationResult.Failure();
            }

            if (!passwordMatches)
            {
                RegisterFailedAttempt(userId);
                return AuthenticationResult.Failure();
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"UPDATE dbo.Users
                  SET FailedAttempts = 0, LockedUntil = NULL, LastLoginAt = SYSDATETIME()
                  WHERE Id = @UserId AND IsActive = 1
                    AND (LockedUntil IS NULL OR LockedUntil <= SYSDATETIME());",
                connection))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                if (command.ExecuteNonQuery() != 1)
                {
                    return AuthenticationResult.Failure();
                }
            }

            return AuthenticationResult.Success(LoadUser(userId));
        }

        private static void ValidateOwnerDetails(string fullName, string username, string password)
        {
            if (fullName.Length == 0 || fullName.Length > 150)
            {
                throw new ArgumentException("أدخل اسمًا لا يتجاوز 150 حرفًا.", "fullName");
            }

            if (username.Length < 3 || username.Length > 50 ||
                string.Equals(username, "system", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("اسم المستخدم يجب أن يكون من 3 إلى 50 حرفًا، ولا يمكن استخدام الاسم system.", "username");
            }

            if (password == null || password.Length < 12 || password.Length > 128)
            {
                throw new ArgumentException("كلمة المرور يجب أن تكون من 12 إلى 128 حرفًا.", "password");
            }
        }

        private static int ExecuteInt(
            SqlConnection connection,
            SqlTransaction transaction,
            string query)
        {
            using (var command = new SqlCommand(query, connection, transaction))
            {
                object result = command.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("بيانات الإعداد الأولي غير مكتملة.");
                }

                return Convert.ToInt32(result);
            }
        }

        private static void RegisterFailedAttempt(int userId)
        {
            int maximumAttempts;
            int lockoutMinutes;
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT
                    MAX(CASE WHEN [Key] = N'MaxFailedLogins' THEN TRY_CONVERT(int, [Value]) END),
                    MAX(CASE WHEN [Key] = N'LockoutMinutes' THEN TRY_CONVERT(int, [Value]) END)
                  FROM dbo.Settings
                  WHERE [Key] IN (N'MaxFailedLogins', N'LockoutMinutes');",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read() || reader.IsDBNull(0) || reader.IsDBNull(1))
                {
                    throw new InvalidOperationException("إعدادات قفل الدخول غير مكتملة.");
                }

                maximumAttempts = reader.GetInt32(0);
                lockoutMinutes = reader.GetInt32(1);
            }

            if (maximumAttempts < 1 || maximumAttempts > 100 ||
                lockoutMinutes < 1 || lockoutMinutes > 1440)
            {
                throw new InvalidOperationException("إعدادات قفل الدخول خارج النطاق المسموح.");
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"UPDATE dbo.Users
                  SET FailedAttempts =
                        CASE WHEN (CASE WHEN LockedUntil IS NOT NULL AND LockedUntil <= SYSDATETIME()
                                        THEN 0 ELSE FailedAttempts END) + 1 >= @MaximumAttempts
                             THEN @MaximumAttempts
                             ELSE (CASE WHEN LockedUntil IS NOT NULL AND LockedUntil <= SYSDATETIME()
                                        THEN 0 ELSE FailedAttempts END) + 1
                        END,
                      LockedUntil =
                        CASE WHEN (CASE WHEN LockedUntil IS NOT NULL AND LockedUntil <= SYSDATETIME()
                                        THEN 0 ELSE FailedAttempts END) + 1 >= @MaximumAttempts
                             THEN DATEADD(MINUTE, @LockoutMinutes, SYSDATETIME())
                             ELSE NULL
                        END
                  WHERE Id = @UserId AND IsActive = 1
                    AND (LockedUntil IS NULL OR LockedUntil <= SYSDATETIME());",
                connection))
            {
                command.Parameters.Add("@MaximumAttempts", SqlDbType.Int).Value = maximumAttempts;
                command.Parameters.Add("@LockoutMinutes", SqlDbType.Int).Value = lockoutMinutes;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                command.ExecuteNonQuery();
            }
        }

        private static AuthenticatedUser LoadUser(int userId)
        {
            string fullName = null;
            var roles = new List<string>();
            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool mustChangePassword = false;

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT u.FullName, r.Name, p.Code, u.MustChangePassword
                  FROM dbo.Users AS u
                  LEFT JOIN dbo.UserRoles AS ur ON ur.UserId = u.Id
                  LEFT JOIN dbo.Roles AS r ON r.Id = ur.RoleId AND r.IsActive = 1
                  LEFT JOIN dbo.RolePermissions AS rp ON rp.RoleId = r.Id
                  LEFT JOIN dbo.Permissions AS p ON p.Id = rp.PermissionId
                  WHERE u.Id = @UserId AND u.IsActive = 1;",
                connection))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fullName = reader.GetString(0);
                        mustChangePassword = reader.GetBoolean(3);
                        if (!reader.IsDBNull(1) && !roles.Contains(reader.GetString(1)))
                        {
                            roles.Add(reader.GetString(1));
                        }

                        if (!reader.IsDBNull(2))
                        {
                            permissions.Add(reader.GetString(2));
                        }
                    }
                }
            }

            if (fullName == null)
            {
                throw new InvalidOperationException("تعذر تحميل بيانات المستخدم بعد تسجيل الدخول.");
            }

            return new AuthenticatedUser(userId, fullName, roles, permissions, mustChangePassword);
        }
    }

    internal static class AppLogger
    {
        internal static void Write(Exception error)
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GroceryPOS",
                "logs");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "application.log");
            string entry = string.Format(
                "[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}{2}",
                DateTime.Now,
                error,
                Environment.NewLine);
            File.AppendAllText(path, entry, Encoding.UTF8);
        }
    }
}
