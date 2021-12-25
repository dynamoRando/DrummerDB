using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Client.Tests
{
    [Collection("Sequential")]
    internal class TestConstants
    {
        public static string TEST_TEMP_FOLDER = Path.Combine(Path.GetTempPath(), "DrummerDB");
        public static string DRUMMER_DB_CLIENT = "DrummerDB.Client";

        internal static class TestPortNumbers
        {
            // 5010
            public const int LOAD_RESTART = 5010;

            // 5020
            public const int ACT_RESTART = 5020;

            // 5030
            public const int RESTART_MODIFY_RESTART = 5030;

            // 5040
            public const int CREATE_DB = 5040;

            // 5050
            public const int SYSTEM_LOGIN = 5050;

            // 5060 - SQL INSERT TESTS
            public const int LARGE_INSERTS = 5060;
            public const int INSERT_DATETIME = 5061;
            public const int INSERT_DATETIME2 = 5062;
            public const int INSERT_DATETIME3 = 5063;
            public const int INSERT_DATETIME4 = 5064;

            // 5070
            public const int SQL_CREATE_DB = 5070;

            // 5080
            public const int GET_DBS = 5080;

            // 5090
            public const int SET_STORAGE_POLICY = 5090;
            public const int TEST_COOP_ACTIONS = 5091;
            public const int TEST_GEN_CONTRACT = 5092;
            public const int TEST_PARTICIPANT_ACCEPT_CONTRACT = 5093;

            // 6000
            public const int SQL_ONLINE = 6000;

            // 6010
            public const int INFO_ONLINE = 6010;

            // 6020
            public const int DATABASE_ONLINE = 6020;

            // 6030
            public const int CREATE_TABLE_DROP = 6030;

            // 7000
            public const int COOP_SQL_MULTI_TEST = 7000;

            // 7050
            public const int COOP_DB_MULTI_TEST = 7050;

            // 9050
            public const int COOP_DB_SQL_CRUD_TEST = 9050;

            // 9060
            public const int COOP_DB_DB_CRUD_TEST = 9060;
        }
    }
}
