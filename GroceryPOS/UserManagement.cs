using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace GroceryPOS
{
    internal sealed class ManagedUserRecord
    {
        private readonly List<int> roleIds = new List<int>();
        private readonly List<string> roleNames = new List<string>();

        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string RoleNames { get; internal set; }
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime? LockedUntil { get; set; }

        internal IList<int> RoleIds
        {
            get { return roleIds.AsReadOnly(); }
        }

        internal void AddRole(int id, string name)
        {
            roleIds.Add(id);
            roleNames.Add(name);
            RoleNames = string.Join("، ", roleNames);
        }
    }

    internal sealed class RoleOption
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        internal RoleOption(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal static class UsersService
    {
        internal static List<ManagedUserRecord> Load(AuthenticatedUser user)
        {
            RequireManagePermission(user);
            var users = new List<ManagedUserRecord>();
            var usersById = new Dictionary<int, ManagedUserRecord>();

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT u.Id, u.Username, u.FullName, u.IsActive, u.MustChangePassword, u.LockedUntil,
                         r.Id, r.Name
                  FROM dbo.Users u
                  LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.Id
                  LEFT JOIN dbo.Roles r ON r.Id = ur.RoleId
                  WHERE u.Username <> N'system'
                  ORDER BY u.Username, r.Name;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int userId = reader.GetInt32(0);
                    ManagedUserRecord record;
                    if (!usersById.TryGetValue(userId, out record))
                    {
                        record = new ManagedUserRecord
                        {
                            Id = userId,
                            Username = reader.GetString(1),
                            FullName = reader.GetString(2),
                            IsActive = reader.GetBoolean(3),
                            MustChangePassword = reader.GetBoolean(4),
                            LockedUntil = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                            RoleNames = string.Empty
                        };
                        usersById.Add(userId, record);
                        users.Add(record);
                    }

                    if (!reader.IsDBNull(6))
                    {
                        record.AddRole(reader.GetInt32(6), reader.GetString(7));
                    }
                }
            }

            return users;
        }

        internal static List<RoleOption> LoadActiveRoles(AuthenticatedUser user)
        {
            RequireManagePermission(user);
            var roles = new List<RoleOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                "SELECT Id, Name FROM dbo.Roles WHERE IsActive = 1 ORDER BY Name;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    roles.Add(new RoleOption(reader.GetInt32(0), reader.GetString(1)));
                }
            }

            return roles;
        }

        internal static void Save(
            AuthenticatedUser user,
            int? userId,
            string username,
            string fullName,
            string password,
            IEnumerable<int> selectedRoleIds)
        {
            RequireManagePermission(user);
            username = (username ?? string.Empty).Trim();
            fullName = (fullName ?? string.Empty).Trim();
            ValidateUserDetails(username, fullName, password, userId.HasValue);

            List<int> roleIds = (selectedRoleIds ?? Enumerable.Empty<int>())
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            if (roleIds.Count == 0)
            {
                throw new ArgumentException("يجب إسناد دور واحد على الأقل للمستخدم.");
            }

            byte[] passwordSalt = null;
            byte[] passwordHash = null;
            if (!userId.HasValue)
            {
                passwordSalt = PasswordHasher.CreateSalt();
                passwordHash = PasswordHasher.Hash(password, passwordSalt);
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                ManagedUserRecord previous = userId.HasValue
                    ? LoadOne(connection, transaction, userId.Value, true)
                    : null;
                if (userId.HasValue && previous == null)
                {
                    throw new InvalidOperationException("المستخدم المطلوب تعديله غير موجود.");
                }

                EnsureUsernameAvailable(connection, transaction, userId, username);
                foreach (int roleId in roleIds)
                {
                    EnsureRoleIsActive(connection, transaction, roleId);
                }

                int savedUserId;
                if (previous == null)
                {
                    using (var command = new SqlCommand(
                        @"INSERT dbo.Users
                            (Username, FullName, PasswordHash, PasswordSalt, IsActive, MustChangePassword, CreatedBy)
                          VALUES
                            (@Username, @FullName, @PasswordHash, @PasswordSalt, 1, 1, @CreatedBy);
                          SELECT CAST(SCOPE_IDENTITY() AS int);",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                        command.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 128).Value = passwordHash;
                        command.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 64).Value = passwordSalt;
                        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                        savedUserId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                else
                {
                    savedUserId = previous.Id;
                    using (var command = new SqlCommand(
                        @"UPDATE dbo.Users
                          SET Username = @Username, FullName = @FullName,
                              UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                          WHERE Id = @Id AND Username <> N'system';",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = fullName;
                        command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = savedUserId;
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("تعذر حفظ تعديلات المستخدم.");
                        }
                    }
                }

                using (var deleteRoles = new SqlCommand(
                    "DELETE dbo.UserRoles WHERE UserId = @UserId;",
                    connection,
                    transaction))
                {
                    deleteRoles.Parameters.Add("@UserId", SqlDbType.Int).Value = savedUserId;
                    deleteRoles.ExecuteNonQuery();
                }

                foreach (int roleId in roleIds)
                {
                    using (var assignRole = new SqlCommand(
                        @"INSERT dbo.UserRoles (UserId, RoleId, CreatedBy)
                          VALUES (@UserId, @RoleId, @CreatedBy);",
                        connection,
                        transaction))
                    {
                        assignRole.Parameters.Add("@UserId", SqlDbType.Int).Value = savedUserId;
                        assignRole.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                        assignRole.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                        assignRole.ExecuteNonQuery();
                    }
                }

                EnsureActiveManagerRemains(connection, transaction);
                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    savedUserId,
                    previous == null ? "UserCreated" : "UserUpdated",
                    previous == null ? null : UserJson(previous),
                    UserJson(
                        username,
                        fullName,
                        previous == null || previous.IsActive,
                        previous == null || previous.MustChangePassword,
                        roleIds));
                transaction.Commit();
            }
        }

        internal static void SetActive(AuthenticatedUser user, int userId, bool isActive)
        {
            RequireManagePermission(user);
            if (userId == user.Id && !isActive)
            {
                throw new InvalidOperationException("لا يمكنك تعطيل حسابك أثناء تسجيل الدخول.");
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                ManagedUserRecord previous = LoadOne(connection, transaction, userId, true);
                if (previous == null)
                {
                    throw new InvalidOperationException("المستخدم غير موجود.");
                }

                if (previous.IsActive == isActive)
                {
                    transaction.Commit();
                    return;
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Users
                      SET IsActive = @IsActive, UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy,
                          FailedAttempts = 0, LockedUntil = NULL
                      WHERE Id = @Id AND Username <> N'system';",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = userId;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تعذر تغيير حالة المستخدم.");
                    }
                }

                EnsureActiveManagerRemains(connection, transaction);
                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    userId,
                    isActive ? "UserActivated" : "UserDeactivated",
                    UserJson(previous),
                    UserJson(
                        previous.Username,
                        previous.FullName,
                        isActive,
                        previous.MustChangePassword,
                        previous.RoleIds));
                transaction.Commit();
            }
        }

        private static ManagedUserRecord LoadOne(
            SqlConnection connection,
            SqlTransaction transaction,
            int userId,
            bool lockRow)
        {
            string lockHint = lockRow ? " WITH (UPDLOCK, HOLDLOCK)" : string.Empty;
            var record = new ManagedUserRecord();
            using (var command = new SqlCommand(
                @"SELECT u.Id, u.Username, u.FullName, u.IsActive, u.MustChangePassword, u.LockedUntil,
                         r.Id, r.Name
                  FROM dbo.Users u" + lockHint + @"
                  LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.Id
                  LEFT JOIN dbo.Roles r ON r.Id = ur.RoleId
                  WHERE u.Id = @UserId AND u.Username <> N'system'
                  ORDER BY r.Name;",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    record.Id = reader.GetInt32(0);
                    record.Username = reader.GetString(1);
                    record.FullName = reader.GetString(2);
                    record.IsActive = reader.GetBoolean(3);
                    record.MustChangePassword = reader.GetBoolean(4);
                    record.LockedUntil = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                    if (!reader.IsDBNull(6))
                    {
                        record.AddRole(reader.GetInt32(6), reader.GetString(7));
                    }

                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(6))
                        {
                            record.AddRole(reader.GetInt32(6), reader.GetString(7));
                        }
                    }
                }
            }

            return record;
        }

        private static void EnsureUsernameAvailable(
            SqlConnection connection,
            SqlTransaction transaction,
            int? userId,
            string username)
        {
            using (var command = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
                  WHERE Username = @Username
                    AND (@UserId IS NULL OR Id <> @UserId);",
                connection,
                transaction))
            {
                command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = username;
                command.Parameters.Add("@UserId", SqlDbType.Int).Value =
                    userId.HasValue ? (object)userId.Value : DBNull.Value;
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    throw new InvalidOperationException("اسم المستخدم مستخدم بالفعل.");
                }
            }
        }

        private static void EnsureRoleIsActive(
            SqlConnection connection,
            SqlTransaction transaction,
            int roleId)
        {
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Roles WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id AND IsActive = 1;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = roleId;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException("أحد الأدوار المحددة غير موجود أو غير نشط.");
                }
            }
        }

        private static void EnsureActiveManagerRemains(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            using (var command = new SqlCommand(
                @"SELECT COUNT(DISTINCT u.Id)
                  FROM dbo.Users u WITH (UPDLOCK, HOLDLOCK)
                  INNER JOIN dbo.UserRoles ur ON ur.UserId = u.Id
                  INNER JOIN dbo.Roles r ON r.Id = ur.RoleId AND r.IsActive = 1
                  INNER JOIN dbo.RolePermissions rp ON rp.RoleId = r.Id
                  INNER JOIN dbo.Permissions p ON p.Id = rp.PermissionId
                  WHERE u.IsActive = 1 AND p.Code = N'Users.Manage';",
                connection,
                transaction))
            {
                if (Convert.ToInt32(command.ExecuteScalar()) < 1)
                {
                    throw new InvalidOperationException(
                        "يجب الإبقاء على مستخدم نشط واحد على الأقل لديه صلاحية إدارة المستخدمين.");
                }
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
                    (@UserId, @Action, N'Users', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = actorId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = action;
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    targetId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value =
                    oldValues == null ? (object)DBNull.Value : oldValues;
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static string UserJson(ManagedUserRecord record)
        {
            return UserJson(
                record.Username,
                record.FullName,
                record.IsActive,
                record.MustChangePassword,
                record.RoleIds);
        }

        private static string UserJson(
            string username,
            string fullName,
            bool isActive,
            bool mustChangePassword,
            IEnumerable<int> roleIds)
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{{\"Username\":\"{0}\",\"FullName\":\"{1}\",\"IsActive\":{2},\"MustChangePassword\":{3},\"RoleIds\":[{4}]}}",
                EscapeJson(username),
                EscapeJson(fullName),
                isActive ? "true" : "false",
                mustChangePassword ? "true" : "false",
                string.Join(",", roleIds.Select(id => id.ToString(System.Globalization.CultureInfo.InvariantCulture))));
        }

        private static string EscapeJson(string value)
        {
            var result = new StringBuilder();
            foreach (char character in value ?? string.Empty)
            {
                switch (character)
                {
                    case '"': result.Append("\\\""); break;
                    case '\\': result.Append("\\\\"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    default:
                        if (character < 0x20)
                        {
                            result.Append("\\u").Append(((int)character).ToString("x4"));
                        }
                        else
                        {
                            result.Append(character);
                        }

                        break;
                }
            }

            return result.ToString();
        }

        private static void ValidateUserDetails(
            string username,
            string fullName,
            string password,
            bool isEditing)
        {
            if (username.Length < 3 || username.Length > 50 ||
                string.Equals(username, "system", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("اسم المستخدم يجب أن يكون من 3 إلى 50 حرفًا، ولا يمكن استخدام الاسم system.");
            }

            if (fullName.Length == 0 || fullName.Length > 150)
            {
                throw new ArgumentException("أدخل اسمًا لا يتجاوز 150 حرفًا.");
            }

            if (!isEditing && (password == null || password.Length < 12 || password.Length > 128))
            {
                throw new ArgumentException("كلمة المرور يجب أن تكون من 12 إلى 128 حرفًا.");
            }
        }

        private static void RequireManagePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Users.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة المستخدمين.");
            }
        }
    }
}
