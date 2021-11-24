using Drummersoft.DrummerDB.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.General
{
    /// <summary>
    /// Tests multiple actions to ensure that the Process has no errors during typical startup, shutdown, and re-start. This test suite is intended to mirror real-world actions on a production instance.
    /// </summary>
    public class Test_Various_Actions_And_Persistence_Thru_Restart
    {
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        [Fact]
        public async void Test_Restart_Async()
        {
            string testName = GetCurrentMethod();
            /*
            * 
            * This is an "up/down" test. We will bring online a new Drummer Process, add an admin login, create a new SQL database, table, and add data to it, and verify that we have information in that table.
            * Then we will shutdown the process, then bring it back online, and ensure that we still have data for it.
            * 
            * The majority of this test is done thru SQL queries from the client object. We want this test suite to simulate real-world actions that a user would perform against an instance.
            * 
            * This test differents from the above test in that we do further data manipulation to ensure that data persists between starts.
            * 
            */

            string dbName = "Test0";
            string tableName = "TestTable0";
            string storageFolder = "TestPersistance0";
            var test = new TestHarness();

            test.LoadJournalSettings();
            await test.ConfigureJournalForProjectAsync(DRUMMER_DB_CLIENT);

            int testId = await test.ConfigureJournalForTestAsync(testName);

            if (testId == 0)
            {
                throw new InvalidOperationException("Unable to configure test");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.LOAD_RESTART);
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

            test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID,
                EMPLOYEENAME
            )
            VALUES
            (
                1,
                Bobby
            ),
            (
                2,
                Billy
            );
            ", dbName);

            var selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // there should be two rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 2, 2);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // insert a new employee into database
            var addEmployeeBrent = test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID,
                EMPLOYEENAME
            )
            VALUES
            (
                3,
                Brent
            )", dbName);

            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // there should be three rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 3, 3);

            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // ensure that we still get 3 rows back
            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // verify we still have three rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 3, 3);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // insert a new employee into database
            var addEmployeeBrandon = test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID,
                EMPLOYEENAME
            )
            VALUES
            (
                4,
                Brandon
            )", dbName);

            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // there should be three rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 4, 4);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // try to only bring back brandon
            var selectBrandon = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // ---  ASSERT (x of y)
            // we should only have one row, employee brandon
            Assert.InRange(selectBrandon.Results.First().Rows.Count, 1, 1);

            // verify that we only get Brandon
            foreach (var row in selectBrandon.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // restart system
            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // bring back only brandon
            var selectBrandonRestart = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // --- ASSERT (x of y)
            // ensure that we still have brandon after restart
            Assert.InRange(selectBrandonRestart.Results.First().Rows.Count, 1, 1);

            // verify that we only get Brandon
            foreach (var row in selectBrandonRestart.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // update Brandon's employee id to 99
            var updateResult = test.ExecuteSQL($@"
            UPDATE {tableName} 
            SET ID = 99
            WHERE EMPLOYEENAME = 'Brandon'
            "
            , dbName);

            // bring back only brandon to verify his id
            var selectBrandonId = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // ensure that we only get back Brandon
            Assert.InRange(selectBrandonId.Results.First().Rows.Count, 1, 1);

            // try to get brandon's id
            var brandonEmpIdBinary = selectBrandonId.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int brandonConvertedId = DbBinaryConvert.BinaryToInt(brandonEmpIdBinary);

            // -- ASSERT (x of y)
            // ensure that brandon's id is 99
            Assert.Equal(99, brandonConvertedId);

            // restart system
            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // bring back only brandon
            var selectBrandonRestart2 = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // --- ASSERT (x of y)
            // ensure that we still have brandon after restart
            Assert.InRange(selectBrandonRestart2.Results.First().Rows.Count, 1, 1);

            // verify that we only get Brandon
            foreach (var row in selectBrandonRestart2.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // try to get brandon's id
            var brandonEmpIdBinary2 = selectBrandonId.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int brandonConvertedId2 = DbBinaryConvert.BinaryToInt(brandonEmpIdBinary);

            // -- ASSERT (x of y)
            // ensure that brandon's id is 99 after restart
            Assert.Equal(99, brandonConvertedId2);

            stopwatch.Stop();

            await test.SaveResultToJournal(testId, (int)stopwatch.ElapsedMilliseconds, true);
        }


        [Fact]
        #region Tests
        public void Test_Start_Act_Restart()
        {

            /*
             * 
             * This is an "up/down" test. We will bring online a new Drummer Process, add an admin login, create a new SQL database, table, and add data to it, and verify that we have information in that table.
             * Then we will shutdown the process, then bring it back online, and ensure that we still have data for it.
             * 
             * The majority of this test is done thru SQL queries from the client object. We want this test suite to simulate real-world actions that a user would perform against an instance.
             * 
             */

            string dbName = "Test1";
            string tableName = "TestTable1";
            string storageFolder = "TestPersistance";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.ACT_RESTART);
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

            test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID,
                EMPLOYEENAME
            )
            VALUES
            (
                1,
                Bobby
            ),
            (
                2,
                Billy
            );
            ", dbName);

            var selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (1 of 2)
            // there should be two rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 2, 2);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // ensure that we still get 2 rows back
            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (2 of 2)
            // verify we still have two rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 2, 2);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }
        }

        [Fact]
        public void Test_Start_Action_Restart_Modify_Restart()
        {
            /*
            * 
            * This is an "up/down" test. We will bring online a new Drummer Process, add an admin login, create a new SQL database, table, and add data to it, and verify that we have information in that table.
            * Then we will shutdown the process, then bring it back online, and ensure that we still have data for it.
            * 
            * The majority of this test is done thru SQL queries from the client object. We want this test suite to simulate real-world actions that a user would perform against an instance.
            * 
            * This test differents from the above test in that we do further data manipulation to ensure that data persists between starts.
            * 
            */

            string dbName = "Test2";
            string tableName = "TestTable2";
            string storageFolder = "TestPersistance2";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.RESTART_MODIFY_RESTART);
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

            test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID,
                EMPLOYEENAME
            )
            VALUES
            (
                1,
                Bobby
            ),
            (
                2,
                Billy
            );
            ", dbName);

            var selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // there should be two rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 2, 2);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // ensure that we still get 2 rows back
            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // verify we still have two rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 2, 2);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // insert a new employee into database
            var addEmployee = test.ExecuteSQL($@"
            INSERT INTO {tableName}
            (
                ID,
                EMPLOYEENAME
            )
            VALUES
            (
                3,
                Brandon
            )", dbName);

            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // ---  ASSERT (x of y)
            // there should be three rows
            Assert.InRange(selectResult.Results.First().Rows.Count, 3, 3);

            // verify the actual names we get back
            foreach (var row in selectResult.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // try to only bring back brandon
            var selectBrandon = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // ---  ASSERT (x of y)
            // we should only have one row, employee brandon
            Assert.InRange(selectBrandon.Results.First().Rows.Count, 1, 1);

            // verify that we only get Brandon
            foreach (var row in selectBrandon.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // restart system
            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // bring back only brandon
            var selectBrandonRestart = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // --- ASSERT (x of y)
            // ensure that we still have brandon after restart
            Assert.InRange(selectBrandonRestart.Results.First().Rows.Count, 1, 1);

            // verify that we only get Brandon
            foreach (var row in selectBrandonRestart.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // update Brandon's employee id to 99
            var updateResult = test.ExecuteSQL($@"
            UPDATE {tableName} 
            SET ID = 99
            WHERE EMPLOYEENAME = 'Brandon'
            "
            , dbName);

            // bring back only brandon to verify his id
            var selectBrandonId = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // ensure that we only get back Brandon
            Assert.InRange(selectBrandonId.Results.First().Rows.Count, 1, 1);

            // try to get brandon's id
            var brandonEmpIdBinary = selectBrandonId.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int brandonConvertedId = DbBinaryConvert.BinaryToInt(brandonEmpIdBinary);

            // -- ASSERT (x of y)
            // ensure that brandon's id is 99
            Assert.Equal(99, brandonConvertedId);

            // restart system
            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            // bring back only brandon
            var selectBrandonRestart2 = test.ExecuteSQL($"SELECT * FROM {tableName} WHERE EMPLOYEENAME = 'Brandon'", dbName);

            // --- ASSERT (x of y)
            // ensure that we still have brandon after restart
            Assert.InRange(selectBrandonRestart2.Results.First().Rows.Count, 1, 1);

            // verify that we only get Brandon
            foreach (var row in selectBrandonRestart2.Results.First().Rows)
            {
                string employeeName = DbBinaryConvert.BinaryToString(row.Values[1].Value.ToByteArray());
                Debug.WriteLine(employeeName);
            }

            // try to get brandon's id
            var brandonEmpIdBinary2 = selectBrandonId.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int brandonConvertedId2 = DbBinaryConvert.BinaryToInt(brandonEmpIdBinary);

            // -- ASSERT (x of y)
            // ensure that brandon's id is 99 after restart
            Assert.Equal(99, brandonConvertedId2);
        }
        #endregion
    }
}
