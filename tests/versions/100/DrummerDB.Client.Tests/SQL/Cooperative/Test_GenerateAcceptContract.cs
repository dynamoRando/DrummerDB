using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants.TestPortNumbers;
using Xunit;
using Drummersoft.DrummerDB.Core.Databases.Version;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.Cooperative
{
    public class Test_GenerateAcceptContract
    {
        [Fact(Skip = "Operators not written yet")]
        public void Test_Generate_Accept_Contract()
        {
            // --- ARRANGE
            string rootFolder = "TestGenAccept";
            var harness = new TestMultiHarness(rootFolder, COOP_SQL_MULTI_TEST, COOP_DB_MULTI_TEST);

            var company = harness.InstantiateNewProcess("Company");
            var customer = harness.InstantiateNewProcess("Customer");

            string dbName = "OnlineStore";
            string systemDbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            string customerTableName = "Customers";

            // -- ACT
            // actions on the company side
            var createDbResult = harness.ExecuteSQL(company, $"CREATE DATABASE {dbName}");

            Assert.False(createDbResult.Results.First().IsError);

            var createTableResult = harness.ExecuteSQL(company,
            $@"CREATE TABLE {customerTableName}
            (
            ID INT IDENTITY(1,1),
            CUSTOMERNAME NVARCHAR(25) NOT NULL
            );"
            , dbName);

            Assert.False(createTableResult.Results.First().IsError);

            var setPolicyResult = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            SET LOGICAL STORAGE FOR {customerTableName} Participant_Owned;
            DRUMMER END;
            ", dbName);

            Assert.False(setPolicyResult.Results.First().IsError);

            var generateContractResult = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            GENERATE CONTRACT AS AUTHOR {company.Alias} DESCRIPTION ThisIsTheDescription;
            DRUMMER END;"
            , dbName);

            Assert.False(generateContractResult.Results.First().IsError);

            var addCustomerAlias = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            ADD PARTICIPANT {customer.Alias} AT 127.0.0.1:{customer.DbPort};
            DRUMMER END;"
            , dbName);

            Assert.False(addCustomerAlias.Results.First().IsError);

            // send message to customer to save the generated contract
            var requestCustomerContract = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            REQUEST PARTICIPANT {customer.Alias} SAVE CONTRACT;
            DRUMMER END;"
            , dbName);

            Assert.False(requestCustomerContract.Results.First().IsError);

            // sanity check - ensure that the contract is in a pending stae
            var pendingContracts = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            REVIEW PENDING CONTRACTS;
            DRUMMER END;"
            , systemDbName);

            Assert.False(pendingContracts.Results.First().IsError);

            // actions on the customer side

            // list the pending contracts we've recieved
            var reviewPendingContracts =
            harness.ExecuteSQL(customer,
            $@"DRUMMER BEGIN;
            REVIEW PENDING CONTRACTS;
            DRUMMER END;"
            , systemDbName);

            Assert.False(reviewPendingContracts.Results.First().IsError);

            // accept a specifc contract
            var acceptPendingContract = harness.ExecuteSQL(customer,
            $@"DRUMMER BEGIN;
            ACCEPT CONTRACT BY {company.Alias};
            DRUMMER END;
            ", systemDbName);

            Assert.False(acceptPendingContract.Results.First().IsError);

            // send message back to host that we accepted the contract
            var acceptMessageResult = harness.ExecuteSQL(customer,
            $@"DRUMMER BEGIN;
            REQUEST HOST NOTIFY ACCEPTED CONTRACT BY {company.Alias};
            DRUMMER END;
            ", systemDbName);

            Assert.False(acceptMessageResult.Results.First().IsError);

            // actions back on the company side

            // ensure that we believe the customer has accepted the data contract
            var acceptedContracts = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            REVIEW ACCEPTED CONTRACTS;
            DRUMMER END;"
            , systemDbName);

            Assert.False(acceptedContracts.Results.First().IsError);

            throw new NotImplementedException();
        }
    }
}
