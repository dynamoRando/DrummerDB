using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_DbTableMetadata
    {
        /// <summary>
        /// Tests adding a table to a <seealso cref="HostDb"/>. Constructs the test using a <seealso cref="StorageManager"/>,
        /// <seealso cref="CacheManager"/>, and a <seealso cref="Fakes.FakeCrypto"/> stand-in. It also creates a <seealso cref="DbManager"/>.
        /// </summary>
        /// <remarks>This test will delete the test database from disk before starting.</remarks>
        [Fact]
        public void Test_Add_Table()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Storage
            * Drummersoft.DrummerDB.Core.Memory
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestAddTable");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logHostDbExtension = ".drumlog";
            string logPartDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestAddTable";

            var directory = new DirectoryInfo(storageFolder);
            if (directory.Exists)
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            string dbFilePath = Path.Combine(storageFolder, dbName + hostDbExtension);

            if (File.Exists(dbFilePath))
            {
                File.Delete(dbFilePath);
            }

            var tbName = "Table1";
            uint tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();

            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logHostDbExtension, logPartDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();

            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var logService = new LogService();
            var notifications = new SystemNotifications();

            var manager = new DbManager(storage, cache, mockCrypto, xManager, logService, notifications);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());

            // --- ACT
            manager.XactCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName, DatabaseType.Host);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols, string.Empty);

            // --- ASSERT
            var result = db.AddTable(tableSchema, out _);

            if (result)
            {
                Assert.True(db.HasTable(tbName));
            }
        }

        /// <summary>
        /// Tests adding a table to a <seealso cref="HostDb"/> and ensures that <see cref="DbMetaSystemDataPages"/> returns the correct ObjectId for the created table. Constructs the test using a <seealso cref="StorageManager"/>,
        /// <seealso cref="CacheManager"/>, and a <seealso cref="Fakes.FakeCrypto"/> stand-in. It also creates a <seealso cref="DbManager"/>.
        /// </summary>
        /// <remarks>This test will delete the test database from disk before starting.</remarks>
        [Fact]
        public void Test_Get_Object_Id()
        {
            /*
             *ASSEMBLIES:
             *Drummersoft.DrummerDB.Core.Tests.Mocks
             *
             *Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             * Drummersoft.DrummerDB.Core.Storage
             * Drummersoft.DrummerDB.Core.Memory
             */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestObjectId");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string hostLogDbExtension = ".drumlog";
            string partLogDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            Guid tableId;
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestAddTable2";

            var directory = new DirectoryInfo(storageFolder);

            if (directory.Exists)
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            string dbFilePath = Path.Combine(storageFolder, dbName + hostDbExtension);

            if (File.Exists(dbFilePath))
            {
                File.Delete(dbFilePath);
            }

            var tbName = "Table1";
            uint tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();

            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, hostLogDbExtension, partLogDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();

            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var logService = new LogService();
            var notifications = new SystemNotifications();
            var manager = new DbManager(storage, cache, mockCrypto, xManager, logService, notifications);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());

            // --- ACT
            manager.XactCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName, DatabaseType.Host);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols, string.Empty);

            var result = db.AddTable(tableSchema, out tableId);

            Guid returnedId = Guid.Empty;

            // --- ASSERT

            if (result)
            {
                returnedId = db.GetTableObjectId(tbName);
            }

            Assert.Equal(tableId, returnedId);
        }
    }
}
