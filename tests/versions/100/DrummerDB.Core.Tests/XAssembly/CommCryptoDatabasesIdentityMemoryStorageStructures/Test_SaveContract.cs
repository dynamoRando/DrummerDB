using Drummersoft.DrummerDB.Core.Communication;
using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_SaveContract
    {
        [Fact]
        public void Test_Create_Save_Contract()
        {
            /*
           * ASSEMBLIES:
           * Drummersoft.DrummerDB.Core.Tests.Mocks
           * 
           * Drummersoft.DrummerDB.Core.Structures
           * Drummersoft.DrummerDB.Core.Databases
           * Drummersoft.DrummerDB.Core.Storage
           * Drummersoft.DrummerDB.Core.Memory
           * Drummersoft.DrummerDB.Core.Cryptography
           * Drummersoft.DrummerDB.Core.IdentityAccess
           */

            // --- ARRANGE
            string sysLogin = "testSystem";
            string sysLoginPw = "testSystemPw";

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestComCon");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logHostDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string userDbName = "TestAuthUser";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";
            string partLogDbExtension = ".drumpartlog";

            // contract variables
            string authorName = "TestAuthor";
            string description = "This is a test contract";
            Guid contractGuid = Guid.NewGuid();
            DateTime contractGenDate = DateTime.Now;
            byte[] contractToken = CryptoManager.GenerateToken();
            string partialDbName = "testPart";
            Guid partialDbId = Guid.NewGuid();
            Guid contractVersion = Guid.NewGuid();
            Guid contractId = Guid.NewGuid();

            string colName = "Col1";
            var dataType = new SQLInt();
            uint ordinal = 1;
            var column = new ColumnSchema(colName, dataType, ordinal);
            var columns = new List<ColumnSchema>();
            columns.Add(column);

            Guid dbId = Guid.NewGuid();
            Guid tableObjectId = Guid.NewGuid();
            string tableName = "TestTable";
            uint tableId = 999;
            LogicalStoragePolicy policy = LogicalStoragePolicy.ParticipantOwned;
            var table = new TableSchema(tableId, tableName, dbId, columns, tableObjectId, userDbName);
            table.SetStoragePolicy(policy);
            table.ContractGUID = contractGuid;
            var tables = new List<ITableSchema>();
            tables.Add(table);

            if (!Directory.Exists(storageFolder))
            {
                Directory.CreateDirectory(storageFolder);
            }

            foreach (var file in Directory.EnumerateFiles(storageFolder))
            {
                var f = new FileInfo(file);
                f.Delete();
            }

            var logService = new LogService();
            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logHostDbExtension, partLogDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var notifications = new SystemNotifications();
            var manager = new DbManager(storage, cache, crypto, xManager, logService, notifications);
            var auth = new AuthenticationManager(manager);
            
            manager.LoadSystemDatabases(cache, storage, crypto, new HostInfo());
            auth.SetInitalSystemAdmin(sysLogin, sysLoginPw);

            // -- ACT
            var contract = new Contract();
            contract.Host = new HostInfo();
            contract.ContractGUID = contractGuid;
            contract.GeneratedDate = contractGenDate;
            contract.Description = description;
            contract.DatabaseName = partialDbName;
            contract.DatabaseId = partialDbId;
            contract.Tables = tables;
            contract.Version = contractVersion;
            contract.Status = ContractStatus.Pending;

            var result = storage.SaveContractToDisk(contract);

            // -- ASSERT
            Assert.True(result);
        }
    }
}
