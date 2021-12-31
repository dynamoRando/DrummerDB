using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using drummer = Drummersoft.DrummerDB.Core.Systems;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.DataDefinitionLanguage
{
    public class Test_Create_Db
    {
        [Fact]
        public async void Test_SQL_Create_Db()
        {
            // --- ARRANGE
            string dbName = "SQLCreate";
            string userName = "XUnitAdmin";
            string pw = "XUnit4321";
            Guid loginGuid = Guid.NewGuid();
            Guid userSessionId = Guid.NewGuid();
            int portNumber = TestPortManager.GetNextAvailablePortNumber();
            string sql = $"CREATE DATABASE {dbName}";

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestSQLDb");

            DirectoryInfo directory = new DirectoryInfo(storageFolder);

            if (directory.Exists)
            {
                foreach (var file in directory.GetFiles())
                {
                    file.Delete();
                }
            }

            var process = new drummer.Process(storageFolder, true, false);
            process.Start();
            process.Test_SetupAdminLogin(userName, pw, loginGuid);
            process.StartSQLServer(portNumber, false);

            // -- ACT
            string url = "http://localhost";
            var client = new DrummerSQLClient(url, portNumber);
            var result = await client.ExecuteSQLAsync(sql, userName, pw, userSessionId);

            // ---  ASSERT
            var resultSet = result.Results.FirstOrDefault();
            Debug.WriteLine(resultSet.ResultMessage);
            Assert.False(resultSet.IsError = false);

            process.Stop();
        }
    }
}
