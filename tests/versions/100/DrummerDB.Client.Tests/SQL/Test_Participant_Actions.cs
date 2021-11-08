using System;
using Xunit;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;

namespace Drummersoft.DrummerDB.Client.Tests.SQL
{
    public class Test_Participant_Actions
    {
        [Fact(Skip = "Logical Storage Policy Not Implemented")]
        public void Test_Set_Logical_Storage_Policy()
        {
            string dbName = "TestRemote";
            string tableName = "Customers";
            string storageFolder = "TestParticipantActions";
            var test = new TestHarness();

            // --- ARRANGE
            test.SetTestObjectNames(dbName, tableName, storageFolder, TestPortNumbers.SET_STORAGE_POLICY);
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
                CUSTOMERNAME NVARCHAR(25) NOT NULL
            );
            ", dbName);

            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            SET LOGICAL STORAGE FOR {tableName} Participant_Owned;
            DRUMMER END;
            ", dbName);

            test.ExecuteSQL($@"
            CREATE TABLE PRODUCTS
            (
                ID INT IDENTITY(1,1),
                PRODUCTNAME NVARCHAR(25) NOT NULL
            );
            ", dbName);

            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            SET LOGICAL STORAGE FOR PRODUCTS Host_Only;
            DRUMMER END;
            ", dbName);

            test.ExecuteSQL($@"
            CREATE TABLE ORDERS
            (
                ID INT IDENTITY(1,1),
                ORDERED_ITEMS NVARCHAR(25) NOT NULL
            );
            ", dbName);

            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            SET LOGICAL STORAGE FOR ORDERS Shared;
            DRUMMER END;
            ", dbName);

            // need syntax to generate a contract

            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            GENERATE CONTRACT AS AUTHOR RetailerCorporation DESCRIPTION IntroductionMessageGoesHere;
            DRUMMER END;
            ", dbName);
            // need to test failure modes where if not all tables have been assigned a logical storage
            // policy, throw an error
            // on contract generation. we want the authors to be explicit about contract generation.

            // need syntax to add participants
            test.ExecuteSQL($@"
            DRUMMER BEGIN;
            ADD PARTICIPANT AliasName AT 127.0.0.1:9000;
            DRUMMER END;
            ", dbName);

            // need to check for the policies set on the tables
            test.ExecuteSQL($@"
            SELECT * FROM sys.UserTables
            ", dbName);

            // this should return the logical storage policy, which we should use to ASSERT that they are saved correctly

            throw new NotImplementedException();
        }
    }
}
