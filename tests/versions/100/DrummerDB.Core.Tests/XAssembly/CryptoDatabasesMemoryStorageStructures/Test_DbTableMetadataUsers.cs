using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
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
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";

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
            var tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, crypto, xManager);
            manager.LoadSystemDatabases(cache, storage, crypto);

            manager.TryCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);

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
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";

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
            var tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var mockXEntry = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, crypto, mockXEntry);
            manager.LoadSystemDatabases(cache, storage, crypto);

            manager.TryCreateNewHostDatabase(dbName, out _);

            var db = manager.GetUserDatabase(dbName);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);

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
