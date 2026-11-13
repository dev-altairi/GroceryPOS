using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace GroceryPOS
{
    internal sealed class ManagedRoleRecord
    {
        private readonly List<int> permissionIds = new List<int>();
        private readonly List<string> permissionNames = new List<string>();

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public bool IsActive { get; set; }
        public string PermissionNames { get; internal set; }

        internal IList<int> PermissionIds
        {
            get { return permissionIds.AsReadOnly(); }
        }

        internal void AddPermission(int id, string name)
        {
            permissionIds.Add(id);
            permissionNames.Add(name);
            PermissionNames = string.Join("، ", permissionNames);
        }
    }

    internal sealed class PermissionOption
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Module { get; private set; }

        internal PermissionOption(int id, string code, string name, string module)
        {
            Id = id;
            Code = code;
            Name = name;
            Module = module;
        }

        public override string ToString()
        {
            return Name + " (" + Module + ")";
        }
    }

    internal static class RolesService
    {
        internal static List<ManagedRoleRecord> Load(AuthenticatedUser user)
        {
            RequireManagePermission(user);
            var roles = new List<ManagedRoleRecord>();
            var rolesById = new Dictionary<int, ManagedRoleRecord>();

            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                @"SELECT r.Id, r.Name, r.Description, r.IsSystem, r.IsActive,
                         p.Id, p.Name
                  FROM dbo.Roles r
                  LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = r.Id
                  LEFT JOIN dbo.Permissions p ON p.Id = rp.PermissionId
                  ORDER BY r.IsSystem DESC, r.Name, p.Module, p.Name;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int roleId = reader.GetInt32(0);
                    ManagedRoleRecord role;
                    if (!rolesById.TryGetValue(roleId, out role))
                    {
                        role = new ManagedRoleRecord
                        {
                            Id = roleId,
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            IsSystem = reader.GetBoolean(3),
                            IsActive = reader.GetBoolean(4),
                            PermissionNames = string.Empty
                        };
                        rolesById.Add(roleId, role);
                        roles.Add(role);
                    }

                    if (!reader.IsDBNull(5))
                    {
                        role.AddPermission(reader.GetInt32(5), reader.GetString(6));
                    }
                }
            }

            return roles;
        }

        internal static List<PermissionOption> LoadPermissions(AuthenticatedUser user)
        {
            RequireManagePermission(user);
            var permissions = new List<PermissionOption>();
            using (SqlConnection connection = Database.OpenConnection())
            using (var command = new SqlCommand(
                "SELECT Id, Code, Name, Module FROM dbo.Permissions ORDER BY Module, Name;",
                connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    permissions.Add(new PermissionOption(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3)));
                }
            }

            return permissions;
        }

        internal static void Save(
            AuthenticatedUser user,
            int? roleId,
            string name,
            string description,
            IEnumerable<int> selectedPermissionIds)
        {
            RequireManagePermission(user);
            name = (name ?? string.Empty).Trim();
            description = (description ?? string.Empty).Trim();
            if (name.Length == 0 || name.Length > 100)
            {
                throw new ArgumentException("اسم الدور مطلوب ولا يتجاوز 100 حرف.");
            }

            if (description.Length > 300)
            {
                throw new ArgumentException("وصف الدور لا يتجاوز 300 حرف.");
            }

            List<int> permissionIds = (selectedPermissionIds ?? Enumerable.Empty<int>())
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            if (permissionIds.Count == 0)
            {
                throw new ArgumentException("اختر صلاحية واحدة على الأقل للدور.");
            }

            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                ManagedRoleRecord previous = roleId.HasValue
                    ? LoadOne(connection, transaction, roleId.Value, true)
                    : null;
                if (roleId.HasValue && previous == null)
                {
                    throw new InvalidOperationException("الدور المطلوب تعديله غير موجود.");
                }

                if (previous != null && previous.IsSystem && name != previous.Name)
                {
                    throw new InvalidOperationException("لا يمكن تغيير اسم دور النظام.");
                }

                EnsureNameAvailable(connection, transaction, roleId, name);
                foreach (int permissionId in permissionIds)
                {
                    EnsurePermissionExists(connection, transaction, permissionId);
                }

                int savedRoleId;
                if (previous == null)
                {
                    using (var command = new SqlCommand(
                        @"INSERT dbo.Roles (Name, Description, IsSystem, IsActive, CreatedBy)
                          VALUES (@Name, @Description, 0, 1, @CreatedBy);
                          SELECT CAST(SCOPE_IDENTITY() AS int);",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar, 300).Value =
                            description.Length == 0 ? (object)DBNull.Value : description;
                        command.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                        savedRoleId = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                else
                {
                    savedRoleId = previous.Id;
                    using (var command = new SqlCommand(
                        @"UPDATE dbo.Roles
                          SET Name = @Name, Description = @Description,
                              UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                          WHERE Id = @Id;",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                        command.Parameters.Add("@Description", SqlDbType.NVarChar, 300).Value =
                            description.Length == 0 ? (object)DBNull.Value : description;
                        command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = savedRoleId;
                        if (command.ExecuteNonQuery() != 1)
                        {
                            throw new InvalidOperationException("تعذر حفظ تعديلات الدور.");
                        }
                    }
                }

                using (var deletePermissions = new SqlCommand(
                    "DELETE dbo.RolePermissions WHERE RoleId = @RoleId;",
                    connection,
                    transaction))
                {
                    deletePermissions.Parameters.Add("@RoleId", SqlDbType.Int).Value = savedRoleId;
                    deletePermissions.ExecuteNonQuery();
                }

                foreach (int permissionId in permissionIds)
                {
                    using (var assignPermission = new SqlCommand(
                        @"INSERT dbo.RolePermissions (RoleId, PermissionId, CreatedBy)
                          VALUES (@RoleId, @PermissionId, @CreatedBy);",
                        connection,
                        transaction))
                    {
                        assignPermission.Parameters.Add("@RoleId", SqlDbType.Int).Value = savedRoleId;
                        assignPermission.Parameters.Add("@PermissionId", SqlDbType.Int).Value = permissionId;
                        assignPermission.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = user.Id;
                        assignPermission.ExecuteNonQuery();
                    }
                }

                EnsureActiveManagerRemains(connection, transaction);
                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    savedRoleId,
                    previous == null ? "RoleCreated" : "RoleUpdated",
                    previous == null ? null : RoleJson(previous),
                    RoleJson(name, description, previous == null || previous.IsSystem, previous == null || previous.IsActive, permissionIds));
                transaction.Commit();
            }
        }

        internal static void SetActive(AuthenticatedUser user, int roleId, bool isActive)
        {
            RequireManagePermission(user);
            using (SqlConnection connection = Database.OpenConnection())
            using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                ManagedRoleRecord previous = LoadOne(connection, transaction, roleId, true);
                if (previous == null)
                {
                    throw new InvalidOperationException("الدور غير موجود.");
                }

                if (previous.IsSystem && !isActive)
                {
                    throw new InvalidOperationException("لا يمكن تعطيل أدوار النظام.");
                }

                if (previous.IsActive == isActive)
                {
                    transaction.Commit();
                    return;
                }

                using (var command = new SqlCommand(
                    @"UPDATE dbo.Roles
                      SET IsActive = @IsActive, UpdatedAt = SYSDATETIME(), UpdatedBy = @UpdatedBy
                      WHERE Id = @Id AND IsSystem = 0;",
                    connection,
                    transaction))
                {
                    command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = isActive;
                    command.Parameters.Add("@UpdatedBy", SqlDbType.Int).Value = user.Id;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = roleId;
                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("تعذر تغيير حالة الدور.");
                    }
                }

                EnsureActiveManagerRemains(connection, transaction);
                InsertAudit(
                    connection,
                    transaction,
                    user.Id,
                    roleId,
                    isActive ? "RoleActivated" : "RoleDeactivated",
                    RoleJson(previous),
                    RoleJson(previous.Name, previous.Description, previous.IsSystem, isActive, previous.PermissionIds));
                transaction.Commit();
            }
        }

        private static ManagedRoleRecord LoadOne(
            SqlConnection connection,
            SqlTransaction transaction,
            int roleId,
            bool lockRow)
        {
            string lockHint = lockRow ? " WITH (UPDLOCK, HOLDLOCK)" : string.Empty;
            using (var command = new SqlCommand(
                @"SELECT r.Id, r.Name, r.Description, r.IsSystem, r.IsActive,
                         p.Id, p.Name
                  FROM dbo.Roles r" + lockHint + @"
                  LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = r.Id
                  LEFT JOIN dbo.Permissions p ON p.Id = rp.PermissionId
                  WHERE r.Id = @RoleId
                  ORDER BY p.Module, p.Name;",
                connection,
                transaction))
            {
                command.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleId;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var role = new ManagedRoleRecord
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        IsSystem = reader.GetBoolean(3),
                        IsActive = reader.GetBoolean(4),
                        PermissionNames = string.Empty
                    };
                    if (!reader.IsDBNull(5))
                    {
                        role.AddPermission(reader.GetInt32(5), reader.GetString(6));
                    }

                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(5))
                        {
                            role.AddPermission(reader.GetInt32(5), reader.GetString(6));
                        }
                    }

                    return role;
                }
            }
        }

        private static void EnsureNameAvailable(
            SqlConnection connection,
            SqlTransaction transaction,
            int? roleId,
            string name)
        {
            using (var command = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.Roles WITH (UPDLOCK, HOLDLOCK)
                  WHERE Name = @Name AND (@Id IS NULL OR Id <> @Id);",
                connection,
                transaction))
            {
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                command.Parameters.Add("@Id", SqlDbType.Int).Value =
                    roleId.HasValue ? (object)roleId.Value : DBNull.Value;
                if (Convert.ToInt32(command.ExecuteScalar()) > 0)
                {
                    throw new InvalidOperationException("اسم الدور مستخدم بالفعل.");
                }
            }
        }

        private static void EnsurePermissionExists(
            SqlConnection connection,
            SqlTransaction transaction,
            int permissionId)
        {
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.Permissions WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id;",
                connection,
                transaction))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = permissionId;
                if (Convert.ToInt32(command.ExecuteScalar()) != 1)
                {
                    throw new InvalidOperationException("إحدى الصلاحيات المحددة غير موجودة.");
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
            int roleId,
            string action,
            string oldValues,
            string newValues)
        {
            using (var command = new SqlCommand(
                @"INSERT dbo.AuditLogs
                    (UserId, Action, EntityName, EntityId, OldValues, NewValues)
                  VALUES
                    (@UserId, @Action, N'Roles', @EntityId, @OldValues, @NewValues);",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.Int).Value = actorId;
                command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = action;
                command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value =
                    roleId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                command.Parameters.Add("@OldValues", SqlDbType.NVarChar, -1).Value =
                    oldValues == null ? (object)DBNull.Value : oldValues;
                command.Parameters.Add("@NewValues", SqlDbType.NVarChar, -1).Value = newValues;
                command.ExecuteNonQuery();
            }
        }

        private static string RoleJson(ManagedRoleRecord role)
        {
            return RoleJson(role.Name, role.Description, role.IsSystem, role.IsActive, role.PermissionIds);
        }

        private static string RoleJson(
            string name,
            string description,
            bool isSystem,
            bool isActive,
            IEnumerable<int> permissionIds)
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{{\"Name\":\"{0}\",\"Description\":\"{1}\",\"IsSystem\":{2},\"IsActive\":{3},\"PermissionIds\":[{4}]}}",
                EscapeJson(name),
                EscapeJson(description),
                isSystem ? "true" : "false",
                isActive ? "true" : "false",
                string.Join(",", permissionIds.Select(id => id.ToString(System.Globalization.CultureInfo.InvariantCulture))));
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

        private static void RequireManagePermission(AuthenticatedUser user)
        {
            if (user == null || !user.HasPermission("Roles.Manage"))
            {
                throw new UnauthorizedAccessException("لا تملك صلاحية إدارة الأدوار والصلاحيات.");
            }
        }
    }
}
