using Drummersoft.DrummerDB.Core.Systems;
using System.IO;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests
{
    [Collection("Sequential")]
    public class Test_ServiceOnline
    {
        [Fact]
        public async void Test_ProcessAndClientSQLPing()
        {

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestSQLClientOnline");

            DirectoryInfo directory = new DirectoryInfo(storageFolder);

            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var file in directory.EnumerateFiles())
            {
                file.Delete();
            }

            int portNumber = TestPortNumbers.SQL_ONLINE;
            var process = new Process(storageFolder, true, true);
            process.Start();
            process.StartSQLServer(portNumber, false);

            // for some reason this bombs in .NET when using the IP Address instead of the domain
            // will need to consider how this will work moving forward
            string url = "http://localhost";
            

            var client = new DrummerSQLClient(url, portNumber);
            var result = await client.IsClientOnlineAsync();

            Assert.True(result);
            process.Stop();
        }

        [Fact]
        public async void Test_ProcessAndClientInfoPing()
        {
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestInfoClientOnline");

            DirectoryInfo directory = new DirectoryInfo(storageFolder);

            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var file in directory.EnumerateFiles())
            {
                file.Delete();
            }

            int portNumber = TestPortNumbers.INFO_ONLINE;
            var process = new Process(storageFolder, true, true);
            process.Start();
            process.StartInfoServer(portNumber, false);

            // for some reason this bombs in .NET when using the IP Address instead of the domain
            // will need to consider how this will work moving forward
            string url = "http://localhost";
            

            var client = new DrummerInfoClient(url, portNumber);
            var result = await client.IsClientOnlineAsync();

            Assert.True(result);

            process.Stop();
        }

        [Fact]
        public async void Test_ProcessAndClientDatabasePing()
        {

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestDbClientOnline");

            DirectoryInfo directory = new DirectoryInfo(storageFolder);

            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var file in directory.EnumerateFiles())
            {
                file.Delete();
            }

            int portNumber = TestPortNumbers.DATABASE_ONLINE;
            var process = new Process(storageFolder, true, true);
            process.Start();
            process.StartDbServer(portNumber, false);

            // for some reason this bombs in .NET when using the IP Address instead of the domain
            // will need to consider how this will work moving forward
            string url = "http://localhost";
           
            var client = new DrummerDatabaseClient(url, portNumber);
            var result = await client.IsClientOnlineAsync();

            Assert.True(result);
            process.Stop();
        }
    }
}
