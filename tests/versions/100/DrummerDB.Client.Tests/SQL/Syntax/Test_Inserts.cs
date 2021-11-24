using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.Syntax
{
    public class Test_Inserts
    {
        /// <summary>
        /// Used to record the current method name if the calling method is an async method
        /// </summary>
        /// <param name="callerName"></param>
        /// <returns>The calling method</returns>
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        [Fact]
        public void Test_Insert_DateTime_Pass_Date_Only()
        {
            string dbName = "TestSynInsertPass1";
            string tableName = "TestInsertDT";
            string storageFolder = "TestSynInsertPass1";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            test.ExecuteSQL($"CREATE DATABASE {dbName}");

            test.ExecuteSQL($@"
            CREATE TABLE {tableName}
            (
                StartDate DATETIME
            );
            ", dbName);

            // --- ACT
            // this should work

            var insert1 = test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                StartDate
            )
            VALUES
            (
                '2021-11-24'
            );
            ", dbName);

            bool isError = false;

            var resultItem = insert1.Results.FirstOrDefault();
            isError = resultItem.IsError;


            // --- ASSERT
            Assert.False(isError);

            string selectQuery = test.SelectQuery;
            var selectResult = test.ExecuteSQL(selectQuery, dbName);

            var resultSelect = selectResult.Results.FirstOrDefault();

            // --- ASSERT
            Assert.InRange(selectResult.Results.First().Rows.Count(), 1, 1);
        }

        [Fact]
        public void Test_Insert_DateTime_Pass_DateTime()
        {
            string dbName = "TestSynInsertPass2";
            string tableName = "TestInsertDT";
            string storageFolder = "TestSynInsertPass2";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            test.ExecuteSQL($"CREATE DATABASE {dbName}");

            test.ExecuteSQL($@"
            CREATE TABLE {tableName}
            (
                StartDate DATETIME
            );
            ", dbName);

            string dateTime = DateTime.Now.ToString();
            string insertStatement = $@"
            INSERT INTO {tableName}
            (
                StartDate
            )
            VALUES
            (
                '{dateTime}'
            );";

            // --- ACT
            // this should work
            var insert1 = test.ExecuteSQL(insertStatement, dbName);

            bool isError = false;
            var resultItem = insert1.Results.FirstOrDefault();
            isError = resultItem.IsError;


            // --- ASSERT
            Assert.False(isError);

            string selectQuery = test.SelectQuery;
            var selectResult = test.ExecuteSQL(selectQuery, dbName);

            // sanity check, should return 1 row
            var resultSelect = selectResult.Results.FirstOrDefault();

            // --- ASSERT
            Assert.InRange(selectResult.Results.First().Rows.Count(), 1, 1);
        }

        [Fact]
        public void Test_Insert_DateTime_Fail_Wrong_DataType()
        {
            string dbName = "TestSynInsertFail";
            string tableName = "TestInsertDT";
            string storageFolder = "TestSynInsertFail";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            test.ExecuteSQL($"CREATE DATABASE {dbName}");

            test.ExecuteSQL($@"
            CREATE TABLE {tableName}
            (
                StartDate DATETIME
            );
            ", dbName);

            // --- ACT
            // this should work

            var insert1 = test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                StartDate
            )
            VALUES
            (
                1234,
            );
            ", dbName);

            bool isError = false;

            var resultItem = insert1.Results.FirstOrDefault();
            if (resultItem is not null)
            {
                isError = resultItem.IsError;
            }

            // --- ASSERT
            Assert.True(isError);
        }
    }
}
