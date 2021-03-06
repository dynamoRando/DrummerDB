using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_DbTableMetadataUsers
    {
        /// <summary>
        /// Tests adding a user to a <seealso cref="HostDb"/>. It does this by leveraging a <seealso cref="StorageManager"/>,
        /// <seealso cref="CacheManager"/>, and a <seealso cref="CryptoManager"/> and a <seealso cref="DbManager"/>.
        /// </summary>
        /// <remarks>This tset will delete the test database form disk before starting.</remarks>
        [Fact]
        public void Test_Add_User()
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
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestAddUser");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logHostDbExtension = ".drumlog";
            string logPartDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestAddUser";

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

            var tbName = "TableTestUsers";
            uint tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logHostDbExtension, logPartDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var logService = new LogService();
            var notifications = new SystemNotifications();
            var manager = new DbManager(storage, cache, crypto, xManager, logService, notifications);
            manager.LoadSystemDatabases(cache, storage, crypto, new HostInfo());

            manager.XactCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName, DatabaseType.Host);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols, string.Empty);

            var addTableResult = db.AddTable(tableSchema, out _);

            var userName = "TestAddUserUser";
            var pwInput = "Test12345";

            // --- ACT
            var createUserResult = db.CreateUser(userName, pwInput);

            // --- ASSERT
            Assert.True(db.HasUser(userName));

        }


        /// <summary>
        /// Tests creating a login for a user in a database and verifies that the login information is correct. It leverages the following objects:
        /// <seealso cref="StorageManager"/>, <seealso cref="CacheManager"/>, <see cref="CryptoManager"/>, <seealso cref="DbManager"/>.
        /// </summary>
        /// <remarks>This test will delete the test database from disk before starting.</remarks>
        [Fact]
        public void Test_User_Login()
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
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestUserLogin");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string hostLogDbExtension = ".drumlog";
            string partLogDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestAddUser";

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

            var tbName = "TableTestUsers";
            uint tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, hostLogDbExtension, partLogDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var mockXEntry = new TransactionEntryManager();
            var logService = new LogService();
            var notifications = new SystemNotifications();
            var manager = new DbManager(storage, cache, crypto, mockXEntry, logService, notifications);
            manager.LoadSystemDatabases(cache, storage, crypto, new HostInfo());

            manager.XactCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName, DatabaseType.Host);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols, string.Empty);

            var addTableResult = db.AddTable(tableSchema, out _);

            var userName = "TestAddUserUser";
            var pwInput = "Test12345";

            // --- ACT
            var createUserResult = db.CreateUser(userName, pwInput);

            var hasUser = db.HasUser(userName);

            // --- ASSERT
            Assert.True(db.ValidateUser(userName, pwInput));
        }
    }
}
