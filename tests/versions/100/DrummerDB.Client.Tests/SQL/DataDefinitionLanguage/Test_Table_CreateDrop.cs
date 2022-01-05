using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;


namespace Drummersoft.DrummerDB.Client.Tests.SQL.DataDefinitionLanguage
{
    public class Test_Table_CreateDrop
    {
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        [Fact]
        public async Task Test_CreateDropTable()
        {
            string testName = GetCurrentMethod();
            string dbName = "TestDrop";
            string tableName = "TCreateDrop";
            string storageFolder = "TCreateDrop";
            var test = new TestHarness();

            test.LoadJournalSettings();
            await test.ConfigureJournalForProjectAsync(DRUMMER_DB_CLIENT);

            int testId = await test.ConfigureJournalForTestAsync(testName);

            if (testId == 0)
            {
                throw new InvalidOperationException("Unable to configure test");
            }

            // Stopwatch stopwatch = Stopwatch.StartNew();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortManager.GetNextAvailablePortNumber());
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            // --- ACT
            test.ExecuteSQL($"CREATE DATABASE {dbName}");
            test.ExecuteSQL($@"
            CREATE TABLE {tableName}
            (
                ID INT IDENTITY(1,1),
                EMPLOYEENAME NVARCHAR(25) NOT NULL
            );
            ", dbName);

            // --- ASSERT
            // should return 1 table
            var oneTable = test.ExecuteSQL($@"
            SELECT * FROM sys.UserTables
            ;
            ", dbName);

            Assert.InRange(oneTable.Results.First().Rows.Count(), 1, 1);

            // --- ACT
            test.ExecuteSQL($@"
            DROP TABLE IF EXISTS {tableName};
            ", dbName);

            // --- ASSERT
            // should return no tables
            var noTable = test.ExecuteSQL($@"
            SELECT * FROM sys.UserTables
            ;
            ", dbName);

            Assert.Equal("No rows found", noTable.Results.First().ResultMessage);
        }
    }
}
