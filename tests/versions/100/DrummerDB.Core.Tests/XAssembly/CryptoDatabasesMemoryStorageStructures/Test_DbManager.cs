using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.Diagnostics;
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
            string hostLogDbExtension = ".drumlog";
            string partLogDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, hostLogDbExtension, partLogDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypt = new CryptoManager();
            var logService = new LogService();
            var xManager = new TransactionEntryManager();
            var notifications = new SystemNotifications();
            var dbManager = new DbManager(xManager, logService, notifications);

            // --- ACT
            dbManager.LoadSystemDatabases(cache, storage, crypt, new HostInfo());

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
            string hostLogDbExtension = ".drumlog";
            string partLogDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            string userName = "TestLogin";
            string pwInput = "TestLogin123";
            Guid userGUID = Guid.NewGuid();

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, hostLogDbExtension, partLogDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypt = new CryptoManager();
            var logService = new LogService();
            var xManager = new TransactionEntryManager();
            var notifications = new SystemNotifications();

            var dbManager = new DbManager(xManager, logService, notifications);
            dbManager.LoadSystemDatabases(cache, storage, crypt, new HostInfo());

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
            var logService = new LogService();
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestInvalidLogin");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logHostDbExtension = ".drumlog";
            string logPartDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";
            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            string userName = "TestLogin";
            string pwInput = "TestLogin123";
            Guid userGUID = Guid.NewGuid();

            string fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logHostDbExtension, logPartDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypt = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var notifications = new SystemNotifications();

            var dbManager = new DbManager(xManager, logService, notifications);
            dbManager.LoadSystemDatabases(cache, storage, crypt, new HostInfo());

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
            var logService = new LogService();
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestCreateDeleteUser");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string hostLogDbExtension = ".drumlog";
            string partLogDbExtension = ".drumpartlog";
            string systemDbExtension = ".drumsys";
            string dbName = "CreateDelete";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

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

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, hostLogDbExtension, partLogDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var crypt = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var notifications = new SystemNotifications();

            var dbManager = new DbManager(storage, cache, crypt, xManager, logService, notifications);
            dbManager.LoadSystemDatabases(cache, storage, crypt, new HostInfo());

            // --- ACT
            dbManager.XactCreateNewHostDatabase(dbName, out _);

            // --- ASSERT
            Assert.True(dbManager.DeleteHostDatabase(dbName));
        }
    }
}
