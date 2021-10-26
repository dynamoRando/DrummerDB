using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using drummer = Drummersoft.DrummerDB.Core.Systems;

namespace Drummersoft.DrummerDB.Client.Tests.SQL
{
    public class Test_SQL_DDLActions
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
            int portNumber = TestPortNumbers.SQL_CREATE_DB;
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
