using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants.TestPortNumbers;
using Xunit;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using static Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Client.Tests.SQL.Cooperative
{
    public class Test_RemoteCRUD
    {
        [Fact]
        public void Test_RemoteDataCRUD()
        {
            // the first half of this test is a copy of Test_GenerateAcceptContract
            // to ensure that the setup between a participant and host is still working

            // this test then attempts to insert a record at a remote participant,
            // read it back, update the value at the host, read it back from the host
            // then update it at the host, then read it back from the host
            // then delete it from the host
            // this should ensure that there are no records returned from the participant

            // it will then insert again from host, update it from the participant, and then delete it from
            // the participant
            // and then read from the host - which should return a RECORD REMOVED notice


            // -- BEGIN REPEAT OF TEST Test_Generate_Accept_Contract
            // --- ARRANGE
            string sysDbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            string rootFolder = "TestRemoteCrud";
            var harness = new TestMultiHarness(rootFolder, COOP_DB_SQL_CRUD_TEST, COOP_DB_DB_CRUD_TEST);

            var company = harness.InstantiateNewProcess("Company");
            var customer = harness.InstantiateNewProcess("Customer1");
            var customer2 = harness.InstantiateNewProcess("Customer2");

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

            var reviewPolicyResult = harness.ExecuteSQL(company,
            $@"DRUMMER BEGIN;
            REVIEW LOGICAL STORAGE FOR {customerTableName};
            DRUMMER END;
            ", dbName);

            Assert.False(reviewPolicyResult.Results.First().IsError);

            byte[] byteCustomerTablePolicy = reviewPolicyResult.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int convertedProductPolicy = DbBinaryConvert.BinaryToInt(new Span<byte>(byteCustomerTablePolicy).Slice(1, 4));

            Assert.Equal((int)LogicalStoragePolicy.ParticipantOwned, convertedProductPolicy);

            // generate host info for both sides
            var generateCompanyHostName = harness.ExecuteSQL(company, $@"
            DRUMMER BEGIN;
            GENERATE HOST INFO AS HOSTNAME {company.Alias};
            DRUMMER END;
            ", sysDbName);

            Assert.False(generateCompanyHostName.Results.First().IsError);

            var generateCustomerHostName = harness.ExecuteSQL(customer, $@"
            DRUMMER BEGIN;
            GENERATE HOST INFO AS HOSTNAME {customer.Alias};
            DRUMMER END;
            ", sysDbName);

            Assert.False(generateCustomerHostName.Results.First().IsError);

            var reviewHostName = harness.ExecuteSQL(company, $@"
            DRUMMER BEGIN;
            REVIEW HOST INFO;
            DRUMMER END;
            ", sysDbName);

            Assert.False(reviewHostName.Results.First().IsError);

            var generateContractResult = harness.ExecuteSQL(company, $@"
            DRUMMER BEGIN;
            GENERATE CONTRACT WITH DESCRIPTION IntroductionMessageGoesHere;
            DRUMMER END;
            ", dbName);

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

            // list the pending contracts we've accepted
            var reviewAcceptedContracts =
            harness.ExecuteSQL(customer,
            $@"DRUMMER BEGIN;
            REVIEW ACCEPTED CONTRACTS;
            DRUMMER END;"
            , systemDbName);

            Assert.False(reviewAcceptedContracts.Results.First().IsError);

            var results = reviewAcceptedContracts.Results.First();
            var acceptedStatusInt = Convert.ToInt32(ContractStatus.Accepted);
            int resultStatusInt = 0;

            foreach (var item in results.Rows.First().Values)
            {
                if (string.Equals(item.Column.ColumnName, Tables.CooperativeContracts.Columns.Status, StringComparison.OrdinalIgnoreCase))
                {
                    resultStatusInt = DbBinaryConvert.BinaryToInt(item.Value.ToByteArray());
                }
            }

            // ensure that the returned contract status is infact accepted
            Assert.Equal(acceptedStatusInt, resultStatusInt);

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

            // END REPEAT OF TEST Test_Generate_Accept_Contract

            // -------------------------------------------------------------
            // insert customer name as a test for remote data saving
            var addCustomerName = harness.ExecuteSQL(company,
            $@"COOP_ACTION FOR PARTICIPANT {customer.Alias};
            INSERT INTO {customerTableName} 
            (
                ID,
                CUSTOMERNAME
            )
            VALUES
            (
                {customer.ProcessId},
                '{customer.Alias}'
            );
            ", dbName, DatabaseType.Host);

            Assert.False(addCustomerName.Results.First().IsError);

            // check to see if we can read the customer name at the host
            var selectCustomerNameAtHost = harness.ExecuteSQL(company,
            $@"
            SELECT * FROM {customerTableName};
            ", dbName, DatabaseType.Host);

            Assert.False(selectCustomerNameAtHost.Results.First().IsError);

            // check to see if we can read the customer name at the participant
            var selectCustomerNameAtParticipant = harness.ExecuteSQL(customer,
            $@"
            SELECT * FROM {customerTableName};
            ", dbName, DatabaseType.Partial);

            Assert.False(selectCustomerNameAtParticipant.Results.First().IsError);
        }
    }
}
