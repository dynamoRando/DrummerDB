using Drummersoft.DrummerDB.Common;
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
    public class Test_Large_Inserts
    {
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        [Fact]
        public async void Test_Insert_Async()
        {
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
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.LARGE_INSERTS);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            // -- ACT
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

            Assert.InRange(selectResult.Results.First().Rows.Count, 1, 1);

            start++;

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

            Assert.InRange(selectResult.Results.First().Rows.Count, max, max);

            test.StopProcess();
            test.StartProcess();
            test.StartNetwork();

            selectResult = test.ExecuteSQL(test.SelectQuery, dbName);

            Assert.InRange(selectResult.Results.First().Rows.Count, max, max);

            stopwatch.Stop();

            await test.SaveResultToJournal(testId, (int)stopwatch.ElapsedMilliseconds, true);

            start = 0;
            var resultSet = selectResult.Results.First();
            foreach (var row in resultSet.Rows)
            {
                int value = DbBinaryConvert.BinaryToInt(row.Values[0].Value.ToByteArray());
                Assert.Equal(start, value);
                start++;
            }
        }
    }
}
