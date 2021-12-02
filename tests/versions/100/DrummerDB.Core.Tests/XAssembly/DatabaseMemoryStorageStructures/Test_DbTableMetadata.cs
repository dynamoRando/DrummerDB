using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
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
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";

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
            var tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();

            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();

            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();

            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto);

            // --- ACT
            manager.TryCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);

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
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            Guid tableId;
            string contracts = "contracts";

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
            var tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();

            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();

            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();

            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto);

            // --- ACT
            manager.TryCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);

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
