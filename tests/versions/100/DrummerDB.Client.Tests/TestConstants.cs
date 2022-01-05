using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Client.Tests
{
    [Collection("Sequential")]
    internal class TestConstants
    {
        public static string TEST_TEMP_FOLDER = Path.Combine(Path.GetTempPath(), "DrummerDB");
        public static string DRUMMER_DB_CLIENT = "DrummerDB.Client";
    }
}
