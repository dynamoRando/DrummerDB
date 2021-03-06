namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Common constants for the database
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The size of the page in bytes
        /// </summary>
        public static ushort PAGE_SIZE = 8192;

        public const ushort SIZE_OF_INT = 4;
        public const ushort SIZE_OF_UINT = 4;
        public const ushort SIZE_OF_BOOL = 1;
        public const ushort SIZE_OF_GUID = 16;
        public const ushort SIZE_OF_DATETIME = 8;
        public const ushort SIZE_OF_DECIMAL = 8; // this is actually a double, which is 8 bytes. A true decimal is 16 bytes

        // default filename for setting a default administrator
        public const string ADMIN_SETUP = "admin.setup";

        // default schemas in databases
        public const string SYS_SCHEMA = "sys";
        public const string SYS_SCHEMA_GUID = "eb0610b3-f0a2-4c9e-a94b-64c8d74a939e";
        public const string DBO_SCHEMA = "dbo";
        public const string DBO_SCHEMA_GUID = "6cc4df44-95bc-4f0e-a10d-130e6c0b0acf";
        public const string COOP_SCHEMA = "coop";
        public const string COOP_SCHEMA_GUID = "86c05a00-d08f-4c03-803b-7adccaa60995";


        // default user name for database service actions
        public const string DATABASE_SERVICE = "DB_SERVICE";

        // all system defined tables, see SystemSchemaConstants100.cs and SystemDatabaseConstants100.cs
        public static class SYS_TABLE_ID_LIST
        {
            public const ushort LOGIN_TABLE = 1;
            public const ushort LOGIN_ROLE_TABLE = 2;
            public const ushort SYSTEM_ROLE_TABLE = 3;
            public const ushort SYSTEM_ROLE_PERMISSIONS_TABLE = 4;
            public const ushort DATABASES_TABLE = 5;
            public const ushort USER_TABLES = 6;
            public const ushort USER_TABLE_SCHEMAS = 7;
            public const ushort USER_OBJECTS = 8;
            public const ushort USERS = 9;
            public const ushort USER_OBJECT_PERMISSIONS = 10;
            public const ushort DATABASE_SCHEMAS = 11;
            public const ushort DATABASE_SCHEMA_PERMISSIONS = 12;
            public const ushort PARTICIPANTS = 13;
            public const ushort TENANTS = 14;
            public const ushort DATABASE_CONTRACTS = 15;
            public const ushort HOSTS = 16;
            public const ushort HOST_INFO = 17;
            public const ushort COOPERATIVE_CONTRACTS = 18;
            public const ushort COOPERATIVE_TABLES = 19;
            public const ushort COOPERATIVE_TABLE_SCHEMAS = 20;
        }

        /// <summary>
        /// The most recent database version. This should be updated with each release.
        /// </summary>
        public const ushort MAX_DATABASE_VERSION = DatabaseVersions.V100;

        public static class DatabaseVersions
        {
            public const ushort V100 = 100;
        }

        // https://stackoverflow.com/questions/968175/what-is-the-string-length-of-a-guid 
        public const ushort LENGTH_OF_GUID_STRING = 36;

        public const ushort FIXED_LENGTH_OF_OBJECT_NAME = 50;

        public const ushort MAX_LENGTH_OF_SECURITY_ARRAY = 2000;
        public const ushort MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME = 20;
        public const ushort READ_WRITE_LOCK_TIMEOUT_MILLISECONDS = 20000;

        // the total number of system databses
        // see also Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100
        public const ushort SYSTEM_DATABASE_COUNT = 1;
    }
}
