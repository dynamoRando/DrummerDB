using Drummersoft.DrummerDB.Core.Systems;
using System;
using System.IO;

namespace DrummerDB.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string TEST_TEMP_FOLDER = Path.Combine(Path.GetTempPath(), "DrummerDB");

            string userName = "TestSystemLogin1";
            string pw = "PwTestSystemLogin1";
            Guid guid = Guid.NewGuid();
            string dbName = "CreateDbClient";

            string storageFolder = Path.Combine(TEST_TEMP_FOLDER, "ConsoleServer");

            if (Directory.Exists(storageFolder))
            {
                Directory.Delete(storageFolder, true);
            }

            Directory.CreateDirectory(storageFolder);

            var process = new Process(storageFolder, true, true);
            process.Start();
            process.StartDbServer();
            process.StartInfoServer();
            process.StartSQLServer();
            process.Test_SetupAdminLogin(userName, pw, guid);
            Console.WriteLine("Press any key to shutdown");
            Console.ReadLine();
        }
    }
}
