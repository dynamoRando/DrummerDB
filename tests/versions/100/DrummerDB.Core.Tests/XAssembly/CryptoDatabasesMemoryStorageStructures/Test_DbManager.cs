using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_DbManager
    {
        /// <summary>
        /// Creates a DbManager and checks to see if on startup DbManager creates a system database.
        /// </summary>
        /// <remarks>This test deletes the system database from disk before starting.</remarks>
        [Fact]
        public void Test_Has_System_Database()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Databases
             * Drummersoft.DrummerDB.Core.Storage
             * Drummersoft.DrummerDB.Core.Memory
             * Drummersoft.DrummerDB.Core.Cryptography
             */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestHasSystemDb");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            string contracts = "contracts";

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();
            var crypt = new CryptoManager();

            var dbManager = new DbManager();

            // --- ACT
            dbManager.LoadSystemDatabases(cache, storage, crypt);

            // --- ASSERT
            Assert.True(dbManager.HasSystemDatabaseInCollection(dbName));
        }

        /// <summary>
        /// Creates a DbManager and creates a system login. It then checks to ensure that the 
        /// login is found.
        /// </summary>
        /// <remarks>This test deletes the system database from disk before starting.</remarks>
        [Fact]
        public void Test_Create_System_Login()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Storage
            * Drummersoft.DrummerDB.Core.Memory
            * Drummersoft.DrummerDB.Core.Cryptography
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestCreateLogin");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            string userName = "TestLogin";
            string pwInput = "TestLogin123";
            Guid userGUID = Guid.NewGuid();

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();
            var crypt = new CryptoManager();

            var dbManager = new DbManager();
            dbManager.LoadSystemDatabases(cache, storage, crypt);

            // --- ACT
            // --- ASSERT
            Assert.True(dbManager.CreateLogin(userName, pwInput, userGUID));
            Assert.True(dbManager.ValidateLogin(userName, pwInput));
        }

        /// <summary>
        /// Creates a DbManager and creates a system login. It then checks to ensure that if the wrong
        /// password is supplied, it will fail.
        /// </summary>
        /// <remarks>This test deletes the system database from disk before starting.</remarks>
        [Fact]
        public void Test_System_Incorrect_Login()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Storage
            * Drummersoft.DrummerDB.Core.Memory
            * Drummersoft.DrummerDB.Core.Cryptography
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestInvalidLogin");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            string userName = "TestLogin";
            string pwInput = "TestLogin123";
            Guid userGUID = Guid.NewGuid();

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();
            var crypt = new CryptoManager();

            var dbManager = new DbManager();
            dbManager.LoadSystemDatabases(cache, storage, crypt);

            // --- ACT
            var resultCreateUser = dbManager.CreateLogin(userName, pwInput, userGUID);

            // --- ASSERT
            Assert.False(dbManager.ValidateLogin(userName, "NotThePw"));
        }

        [Fact]
        public void Test_Create_Delete_User_Db()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Storage
            * Drummersoft.DrummerDB.Core.Memory
            * Drummersoft.DrummerDB.Core.Cryptography
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestCreateDeleteUser");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string dbName = "CreateDelete";
            string contracts = "contracts";

            var directory = new DirectoryInfo(storageFolder);
            if (directory.Exists)
            {
                foreach (var f in directory.EnumerateFiles())
                {
                    f.Delete();
                }
            }

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();
            var crypt = new CryptoManager();
            var xManager = new TransactionEntryManager();

            var dbManager = new DbManager(storage, cache, crypt, xManager);
            dbManager.LoadSystemDatabases(cache, storage, crypt);

            // --- ACT
            dbManager.TryCreateNewHostDatabase(dbName, out _);

            // --- ASSERT
            Assert.True(dbManager.DeleteHostDatabase(dbName));
        }
    }
}
