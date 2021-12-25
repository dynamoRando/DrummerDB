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
        public static int PAGE_SIZE = 8192;

        public const int SIZE_OF_INT = 4;
        public const int SIZE_OF_BOOL = 1;
        public const int SIZE_OF_GUID = 16;
        public const int SIZE_OF_DATETIME = 8;
        public const int SIZE_OF_DECIMAL = 8; // this is actually a double, which is 8 bytes. A true decimal is 16 bytes

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
            public const int LOGIN_TABLE = 1;
            public const int LOGIN_ROLE_TABLE = 2;
            public const int SYSTEM_ROLE_TABLE = 3;
            public const int SYSTEM_ROLE_PERMISSIONS_TABLE = 4;
            public const int DATABASES_TABLE = 5;
            public const int USER_TABLES = 6;
            public const int USER_TABLE_SCHEMAS = 7;
            public const int USER_OBJECTS = 8;
            public const int USERS = 9;
            public const int USER_OBJECT_PERMISSIONS = 10;
            public const int DATABASE_SCHEMAS = 11;
            public const int DATABASE_SCHEMA_PERMISSIONS = 12;
            public const int PARTICIPANTS = 13;
            public const int TENANTS = 14;
            public const int DATABASE_CONTRACTS = 15;
            public const int HOSTS = 16;
            public const int HOST_INFO = 17;
            public const int COOPERATIVE_CONTRACTS = 18;
            public const int COOPERATIVE_TABLES = 19;
            public const int COOPERATIVE_TABLE_SCHEMAS = 20;
        }

        /// <summary>
        /// The most recent database version. This should be updated with each release.
        /// </summary>
        public const int MAX_DATABASE_VERSION = DatabaseVersions.V100;

        public static class DatabaseVersions
        {
            public const int V100 = 100;
        }

        // https://stackoverflow.com/questions/968175/what-is-the-string-length-of-a-guid 
        public const int LENGTH_OF_GUID_STRING = 36;

        public const int FIXED_LENGTH_OF_OBJECT_NAME = 50;

        public const int MAX_LENGTH_OF_SECURITY_ARRAY = 2000;
        public const int MAX_LENGTH_OF_USER_NAME_OR_ROLE_NAME = 20;
        public const int READ_WRITE_LOCK_TIMEOUT_MILLISECONDS = 20000;

        // the total number of system databses
        // see also Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100
        public const int SYSTEM_DATABASE_COUNT = 1;
    }
}
