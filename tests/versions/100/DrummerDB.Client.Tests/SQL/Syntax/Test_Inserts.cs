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
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME2);
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
            var insert1 = test.ExecuteSQL(insertStatement, dbName);

            bool isError = false;
            var resultItem = insert1.Results.FirstOrDefault();
            isError = resultItem.IsError;


            // --- ASSERT
            Assert.False(isError);

            string selectQuery = test.SelectQuery;
            var selectResult = test.ExecuteSQL(selectQuery, dbName);

            // --- ASSERT
            Assert.InRange(selectResult.Results.First().Rows.Count(), 1, 1);
        }

        [Fact]
        public void Test_Insert_DateTime_Fail_Wrong_DataType()
        {
            string dbName = "TestSynInsertFail1";
            string tableName = "TestInsertDT";
            string storageFolder = "TestSynInsertFail1";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME3);
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

        [Fact]
        public void Test_Insert_DateTime_Fail_No_Single_Quote()
        {
            string dbName = "TestSynInsertFail2";
            string tableName = "TestInsertDT";
            string storageFolder = "TestSynInsertFail2";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME4);
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

            var insert1 = test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                StartDate
            )
            VALUES
            (
                {DateTime.Now.ToString()},
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
