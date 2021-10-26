using Drummersoft.DrummerDB.Core.Systems;
using System.IO;
using System;

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

            var process = new Process(storageFolder, true, false);
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
