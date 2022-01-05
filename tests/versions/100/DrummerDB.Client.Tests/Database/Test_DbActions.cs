using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using drummer = Drummersoft.DrummerDB.Core.Systems;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Drummersoft.DrummerDB.Client.Tests.Database
{
    [Collection("Sequential")]
    public class Test_DbActions
    {
        [Fact]
        public async void Test_CreateDatabaseFromClient()
        {
            // --- ARRANGE
            string userName = "TestSystemLogin1";
            string pw = "PwTestSystemLogin1";
            Guid guid = Guid.NewGuid();
            string dbName = "CreateDbClient";
            int portNumber = TestPortManager.GetNextAvailablePortNumber();

            string storageFolder = Path.Combine(TEST_TEMP_FOLDER, "TestCreateDbClient");

            DirectoryInfo directory = new DirectoryInfo(storageFolder);

            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var file in directory.EnumerateFiles())
            {
                file.Delete();
            }

            var process = new drummer.Process(storageFolder, true, false);
            process.Start();
            process.StartDbServer(portNumber, false);
            process.Test_SetupAdminLogin(userName, pw, guid);

            // for some reason this bombs in .NET when using the IP Address instead of the domain
            // will need to consider how this will work moving forward
            string url = "http://localhost";
            var client = new DrummerDatabaseClient(url, portNumber);

            // ---  ACT
            var result = await client.CreateUserDatabase(userName, pw, dbName);
            client.Shutdown();

            Debug.WriteLine(result.DatabaseId);

            // ---  ASSERT
            Assert.True(result.IsSuccessful);

            process.Stop();
        }
    }
}
