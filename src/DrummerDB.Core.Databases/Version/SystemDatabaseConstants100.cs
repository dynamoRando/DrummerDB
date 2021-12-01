using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases.Version
{
    internal class SystemDatabaseConstants100
    {
        internal static class Databases
        {
            public const string DRUM_SYSTEM = "drumSystem";
        }

        /// <summary>
        /// Contains all the definitions for all System Database tables
        /// </summary>
        internal static class Tables
        {
            /// <summary>
            /// A table holding all the logins for the system
            /// </summary>
            /// <remarks>This table mirrors <seealso cref="SystemSchemaConstants100.Tables.Users"/>. It is different in that
            /// it holds the login information, versus users for each database.</remarks>
            internal static class LoginTable
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.LOGIN_TABLE;
                public const string TABLE_NAME = "SystemLogins";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string UserName = "UserName";
                    public const string UserGUID = "UserGUID";
                    public const string ByteLength = "ByteLength";
                    public const string Salt = "Salt";
                    public const string Hash = "Hash";
                    public const string Workfactor = "Workfactor";
                    public const string IsBanned = "IsBanned";
                }

                public static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(8);

                    var userName = new ColumnSchema(Columns.UserName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.UserGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    var byteLength = new ColumnSchema(Columns.ByteLength, new SQLInt(), 3);
                    result.Add(byteLength);

                    var salt = new ColumnSchema(Columns.Salt, new SQLVarbinary(Constants.MAX_LENGTH_OF_SECURITY_ARRAY), 4);
                    result.Add(salt);

                    var hash = new ColumnSchema(Columns.Hash, new SQLVarbinary(Constants.MAX_LENGTH_OF_SECURITY_ARRAY), 5);
                    result.Add(hash);

                    var work = new ColumnSchema(Columns.Workfactor, new SQLInt(), 6);
                    result.Add(work);

                    var isBanned = new ColumnSchema(Columns.IsBanned, new SQLBit(), 7);
                    result.Add(isBanned);

                    return result;
                }
            }

            /// <summary>
            /// A xref table that holds the logins and the roles assigned to that login (username)
            /// </summary>
            internal static class LoginRolesTable
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.LOGIN_ROLE_TABLE;
                public const string TABLE_NAME = "SystemLoginRoles";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string UserName = "UserName";
                    public const string UserGUID = "UserGUID";
                    public const string RoleName = "RoleName";
                    public const string RoleGUID = "RoleGUID";
                }

                private static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(4);

                    var userName = new ColumnSchema(Columns.UserName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.UserGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    var byteLength = new ColumnSchema(Columns.RoleName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 3);
                    result.Add(byteLength);

                    var salt = new ColumnSchema(Columns.RoleGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 4);
                    result.Add(salt);

                    return result;
                }
            }

            /// <summary>
            /// A table that holds all defined system roles. See in this same file <see cref="SystemLoginConstants.SystemRoles"/>.
            /// </summary>
            internal static class SystemRolesTable
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.SYSTEM_ROLE_TABLE;
                public const string TABLE_NAME = "SystemRoles";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string RoleName = "RoleName";
                    public const string RoleGUID = "RoleGUID";
                }

                private static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(2);

                    var userName = new ColumnSchema(Columns.RoleName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.RoleGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    return result;
                }
            }

            /// <summary>
            /// A xref table that holds the <see cref="SystemPermission"/> that are assigned to each
            /// <seealso cref="SystemLoginConstants.SystemRoles"/>.
            /// </summary>
            internal static class SystemRolesPermissions
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.SYSTEM_ROLE_PERMISSIONS_TABLE;
                public const string TABLE_NAME = "SystemRolesPermissions";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string RoleName = "RoleName";
                    public const string RoleGUID = "RoleGUID";

                    /// <summary>
                    /// The system permission. This is a direct cast to 
                    /// INT of the enum <see cref="IdentityAccess.Enum.SystemPermission"/>
                    /// </summary>
                    public const string SystemPermission = "SystemPermission";
                }

                private static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(3);

                    var userName = new ColumnSchema(Columns.RoleName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(userName);

                    var userGuid = new ColumnSchema(Columns.RoleGUID, new SQLChar(Constants.LENGTH_OF_GUID_STRING), 2);
                    result.Add(userGuid);

                    var permission = new ColumnSchema(Columns.SystemPermission, new SQLInt(), 3);
                    result.Add(permission);

                    return result;
                }
            }

            // A table that holds all the current in memory databases that are loaded
            internal static class DatabaseTableDatabses
            {
                public const int TABLE_ID = Constants.SYS_TABLE_ID_LIST.DATABASES_TABLE;
                public const string TABLE_NAME = "Databases";

                public static TableSchema Schema(Guid dbId, string dbName)
                {
                    var schema = new TableSchema(TABLE_ID, TABLE_NAME, dbId, GetColumns(), new DatabaseSchemaInfo(Constants.SYS_SCHEMA, Guid.Parse(Constants.SYS_SCHEMA_GUID)));
                    schema.DatabaseName = dbName;
                    return schema;
                }

                public static class Columns
                {
                    public const string DatabaseName = "DatabaseName";
                    public const string DatabaseType = "DatabaseType";
                }

                private static List<ColumnSchema> GetColumns()
                {
                    var result = new List<ColumnSchema>(2);

                    var databaseName = new ColumnSchema(Columns.DatabaseName, new SQLVarChar(Constants.MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME), 1);
                    result.Add(databaseName);

                    // Drummersoft.DrummerDB.Core.Structures.Enum.DatabaseType
                    var databaseType = new ColumnSchema(Columns.DatabaseType, new SQLInt(), 2);
                    result.Add(databaseType);

                    return result;
                }
            }

            // TO DO: Need a table of hosts that we cooperate with
            // in other words, if I'm booting up as a participant, who do I cooperate with?
            // - who are the hosts?
        }

        internal static class SystemLoginConstants
        {
            internal static class SystemRoles
            {
                internal static class Names
                {
                    public const string SystemAdmin = "SystemAdmin";
                }
            }
        }
    }
}
