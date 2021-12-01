using System;
using System.IO;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using drummer = Drummersoft.DrummerDB.Core.Systems;

namespace Drummersoft.DrummerDB.Client.Tests.Database
{
    [Collection("Sequential")]
    public class Test_DbLogin
    {
        [Fact]
        public async void Test_SystemHasLogin()
        {
            // --- ARRANGE
            string userName = "TestSystemLogin";
            string pw = "PwTestSystemLogin";
            Guid guid = Guid.NewGuid();
            int portNumber = TestPortNumbers.SYSTEM_LOGIN;

            string storageFolder = Path.Combine(TEST_TEMP_FOLDER, "TestSysLog");

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
            process.Test_SetupLogin(userName, pw, guid);

            // for some reason this bombs in .NET when using the IP Address instead of the domain
            // will need to consider how this will work moving forward
            string url = "http://localhost";
            var client = new DrummerDatabaseClient(url, portNumber);

            // ---  ACT
            var result = await client.IsLoginValidAsync(userName, pw);
            client.Shutdown();

            // ---  ASSERT
            Assert.True(result);
            process.Stop();
        }
    }
}
