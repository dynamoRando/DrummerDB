using System.IO;

namespace Drummersoft.DrummerDB.Core.Tests.Helpers
{
    public static class DeleteDatabase
    {
        public static void DeleteDatabaseItem(string dbName, string rootFolderName, string hostDbExtension, string logDbExtension, string systemDbExtension)
        {
            string fileName = Path.Combine(rootFolderName, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            string dataFileName = Path.Combine(rootFolderName, dbName + hostDbExtension);
            string logFileName = Path.Combine(rootFolderName, dbName + logDbExtension);

            File.Delete(dataFileName);
            File.Delete(logFileName);
        }
    }
}
