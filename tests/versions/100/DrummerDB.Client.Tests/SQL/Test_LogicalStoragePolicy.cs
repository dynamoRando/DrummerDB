using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Drummersoft.DrummerDB.Client.Tests.TestConstants;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;

namespace Drummersoft.DrummerDB.Client.Tests.SQL
{
    public class Test_LogicalStoragePolicy
    {
        private string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        [Fact]
        public void Test_Set_Review_LogicalStoragePolicy()
        {
            string dbName = "TestLSP";
            string tableName = "CUSTOMERS";
            string storageFolder = "TestLSPx";
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

            // -- ASSERT
            // check the policies on each table

            // should assert policy == Host_Only;
            var policyForProducts = test.ExecuteSQL($@"
            DRUMMER BEGIN;
            REVIEW LOGICAL STORAGE FOR PRODUCTS;
            DRUMMER END;
            ", dbName);

            byte[] byteProductPolicy = policyForProducts.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int convertedProductPolicy = DbBinaryConvert.BinaryToInt(new Span<byte>(byteProductPolicy).Slice(1, 4));

            Assert.Equal((int)LogicalStoragePolicy.HostOnly, convertedProductPolicy);

            // should assert policy == Participant_Owned;
            var policyForCustomers = test.ExecuteSQL($@"
            DRUMMER BEGIN;
            REVIEW LOGICAL STORAGE FOR CUSTOMERS;
            DRUMMER END;
            ", dbName);

            byte[] byteCustomerPolicy = policyForCustomers.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int convertedCustomersPolicy = DbBinaryConvert.BinaryToInt(new Span<byte>(byteCustomerPolicy).Slice(1, 4));

            Assert.Equal((int)LogicalStoragePolicy.ParticipantOwned, convertedCustomersPolicy);

            // should assert policy == Shared;
            var policyForOrders = test.ExecuteSQL($@"
            DRUMMER BEGIN;
            REVIEW LOGICAL STORAGE FOR ORDERS;
            DRUMMER END;
            ", dbName);


            byte[] byteOrdersPolicy = policyForOrders.Results.First().Rows[0].Values[0].Value.ToByteArray();
            int convertedOrdersPolicy = DbBinaryConvert.BinaryToInt(new Span<byte>(byteOrdersPolicy).Slice(1, 4));

            Assert.Equal((int)LogicalStoragePolicy.Shared, convertedOrdersPolicy);
        }
    }
}
