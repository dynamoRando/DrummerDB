using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases.Version;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests.SQL
{
    public class Test_System_Tables
    {
        [Fact]
        public void Test_Create_Get_Databases()
        {
            string dbName = "TestSysGet";
            string dbName2 = "TestSysGet2";
            string tableName = "TestDbTable";
            string storageFolder = "TestSysInfo";
            var test = new TestHarness();
            string systemDbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.GET_DBS);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            test.ExecuteSQL($"CREATE DATABASE {dbName}");
            test.ExecuteSQL($@"
            CREATE TABLE {tableName}
            (
                ID INT
            );
            ", dbName);

            test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID
            )
            VALUES
            (
                1
            );
            ", dbName);

            test.ExecuteSQL($"CREATE DATABASE {dbName2}");

            test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID
            )
            VALUES
            (
                500
            );
            ", dbName2);

            var selectDbNames = test.ExecuteSQL($"SELECT * FROM sys.Databases", systemDbName);
            Assert.InRange(selectDbNames.Results.First().Rows.Count, 2, 2);

            foreach (var row in selectDbNames.Results.First().Rows)
            {
                var returnedName = DbBinaryConvert.BinaryToString(row.Values[0].Value.ToByteArray());
                Debug.WriteLine(returnedName);
            }

            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            selectDbNames = test.ExecuteSQL($"SELECT * FROM sys.Databases", systemDbName);
            Assert.InRange(selectDbNames.Results.First().Rows.Count, 2, 2);

            foreach (var row in selectDbNames.Results.First().Rows)
            {
                var returnedName = DbBinaryConvert.BinaryToString(row.Values[0].Value.ToByteArray());
                Debug.WriteLine(returnedName);
            }
        }
    }
}
