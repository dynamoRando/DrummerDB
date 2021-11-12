using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;


namespace Drummersoft.DrummerDB.Client.Tests.SQL
{
    public class Test_TableActions
    {
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        [Fact(Skip ="Drop table operator not written yet")]
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

            //Stopwatch stopwatch = Stopwatch.StartNew();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.CREATE_TABLE_DROP);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            // -- ACT
            test.ExecuteSQL($"CREATE DATABASE {dbName}");
            test.ExecuteSQL($@"
            CREATE TABLE {tableName}
            (
                ID INT IDENTITY(1,1),
                EMPLOYEENAME NVARCHAR(25) NOT NULL
            );
            ", dbName);

            // should return 1 table
            var oneTable = test.ExecuteSQL($@"
            SELECT * FROM sys.UserTables
            ;
            ", dbName);

            test.ExecuteSQL($@"
            DROP TABLE IF EXISTS {tableName}
            ;
            ", dbName);

            // should return no tables
            var noTable = test.ExecuteSQL($@"
            SELECT * FROM sys.UserTables
            ;
            ", dbName);

            throw new NotImplementedException();
        }
    }
}
