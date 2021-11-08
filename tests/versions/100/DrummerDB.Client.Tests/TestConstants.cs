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
            public const int LOAD_RESTART = 5010;
            public const int ACT_RESTART = 5020;
            public const int RESTART_MODIFY_RESTART = 5030;
            public const int CREATE_DB = 5040;
            public const int SYSTEM_LOGIN = 5050;
            public const int LARGE_INSERTS = 5060;
            public const int SQL_CREATE_DB = 5070;
            public const int GET_DBS = 5080;
            public const int SET_STORAGE_POLICY = 5090;
            public const int SQL_ONLINE = 6000;
            public const int INFO_ONLINE = 6010;
            public const int DATABASE_ONLINE = 6020;
        }
    }
}
