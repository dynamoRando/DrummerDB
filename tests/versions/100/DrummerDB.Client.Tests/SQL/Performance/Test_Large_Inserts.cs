using Drummersoft.DrummerDB.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.Performance
{
    public class Test_Large_Inserts
    {
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        /// <summary>
        /// Attempts to insert 2,000 records into a table and records the total test time execution to PerfJournal. It also tests for persistance of those values by restarting the Process.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if PerfJournal is unable to configure the test.</exception>
        [Fact]
        public async Task Test_Insert_Async()
        {
            // --- ARRANGE
            string testName = GetCurrentMethod();

            string dbName = "TestLarge";
            string tableName = "LargeTable";
            string storageFolder = "TestLargeInsert";
            var test = new TestHarness();

            test.LoadJournalSettings();
            await test.ConfigureJournalForProjectAsync(DRUMMER_DB_CLIENT);

            int testId = await test.ConfigureJournalForTestAsync(testName);

            if (testId == 0)
            {
                throw new InvalidOperationException("Unable to configure test");
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            int start = 0;
            int max = 2_000;

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortManager.GetNextAvailablePortNumber());
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            // -- ACT
            // create objects and insert 1 row
            // this is done just to ensure that everything is setup correctly
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
                {start.ToString()}
            );
            ", dbName);

            var selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // -- ASSERT
            Assert.InRange(selectResult.Results.First().Rows.Count, 1, 1);

            start++;

            // -- ACT
            // insert the remaining rows
            while (start < max)
            {
                test.ExecuteSQL($@"
                INSERT INTO {tableName}
                (
                    ID
                )
                VALUES
                (
                    {start.ToString()}
                );
                ", dbName);

                start++;

                Debug.WriteLine($"Current Insert: {start.ToString()}");
            }

            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // -- ASSERT
            Assert.InRange(selectResult.Results.First().Rows.Count, max, max);

            // -- ACT
            // test for persistance by restarting the Process
            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            // -- ASSERT
            Assert.InRange(selectResult.Results.First().Rows.Count, max, max);

            stopwatch.Stop();

            await test.SaveResultToJournal(testId, (int)stopwatch.ElapsedMilliseconds, true);

            start = 0;
            var resultSet = selectResult.Results.First();

            // -- ASSERT
            // sanity check that what we inserted is in fact there
            foreach (var row in resultSet.Rows)
            {
                int value = DbBinaryConvert.BinaryToInt(row.Values[0].Value.ToByteArray());
                Assert.Equal(start, value);
                start++;
            }
        }
    }
}
