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

        [Fact(Skip ="Test not written yet")]
        public async Task Test_Insert_DateTime()
        {
            string dbName = "TestSynInsert";
            string tableName = "TestInsertDT";
            string storageFolder = "TestSynInsert";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.INSERT_DATETIME);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            throw new NotImplementedException();
        }
    }
}
