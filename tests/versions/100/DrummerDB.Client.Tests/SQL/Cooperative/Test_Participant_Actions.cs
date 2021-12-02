using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.Cooperative
{
    public class Test_Participant_Actions
    {
        [Fact(Skip ="Accept Pending Contract operator not written")]
        public void Test_Accept_Pending_Contract()
        {
            // need to modify test harness to account for both a host and a participant
            // in other words need two instances booted up

            string contractAuthor = "TestAuthor";
            string dbName = "TestAcceptCon";
            string tableName = "TestContract";
            string storageFolder = "TestPendingContract";
            var test = new TestHarness();
            string systemDbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.TEST_PARTICIPANT_ACCEPT_CONTRACT);
            test.SetupTempDirectory();
            test.SetupProcess();
            test.StartNetwork();
            test.SetupClient();

            // -- ACT
            // get the list of pending contracts
            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            REVIEW PENDING CONTRACTS;
            DRUMMER END;
            ", systemDbName);

            // accept a specifc contract
            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            ACCEPT CONTRACT BY {contractAuthor};
            DRUMMER END;
            ", systemDbName);

            // send message back to host that we accepted the contract
            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {contractAuthor};
            DRUMMER END;
            ", systemDbName);

            throw new NotImplementedException();
        }
    }
}
