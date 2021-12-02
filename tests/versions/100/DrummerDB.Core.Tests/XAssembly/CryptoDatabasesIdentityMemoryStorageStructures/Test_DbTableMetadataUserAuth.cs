using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.IdentityAccess;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Tests.Integration;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_DbTableMetadataUserAuth
    {
        /// <summary>
        /// Tests creating a system level login and if it will authorize the user.
        /// 
        /// Authentication should verify that the user is a system admin, but the database should show that the same user doesn't have rights. This is expected.
        /// The authentication manager is a seperate entity from the database permission. A full integration test that is similiar to this (that will validate that the admin
        /// has total permissions) is available in 
        /// <seealso cref="Drummersoft.DrummerDB.Core.Tests.Integration_Core.Test_ProcessCreateDbUserSystemAdmin.Test_Process_CreateDb_SystemAdmin"/>
        /// </summary>
        /// <remarks>This test will create both a test database and remove if it already exists as well as wipe the sytem database from disk if it already exists.</remarks>
        [Fact]
        public void Test_Authorize_User_Login_Is_System_Admin()
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

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestAuthLogin");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string userDbName = "TestAuthUser";
            string contracts = "contracts";

            string fileName = Path.Combine(storageFolder, userDbName + hostDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var tbName = "TableTestUsers";
            var tbId = 1;
            var dbId = Guid.NewGuid();

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, crypto, xManager);
            var auth = new AuthenticationManager(manager);

            manager.LoadSystemDatabases(cache, storage, crypto);
            auth.SetInitalSystemAdmin(sysLogin, sysLoginPw);

            manager.TryCreateNewHostDatabase(userDbName, out _);

            var db = manager.GetUserDatabase(userDbName);

            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);

            var addTableResult = db.AddTable(tableSchema, out _);

            // --- ACT
            var createUserResult = db.CreateUser(sysLogin, sysLoginPw);
            var hasUser = db.HasUser(sysLogin);

            // --- ASSERT
            Assert.True(auth.IsUserInSystemRole(sysLogin));
            Assert.False(db.AuthorizeUser(sysLogin, sysLoginPw, DbPermission.Select, Guid.NewGuid()));
        }

        /// <summary>
        /// Tests creating a system level login and if it will authorize the user.
        /// 
        /// Authentication should verify that the user is a system admin, but the database should show that the same user doesn't have rights intially. This is expected.
        /// The authentication manager is a seperate entity from the database permission. A full integration test that is similiar to this (that will validate that the admin
        /// has total permissions) is available in 
        /// <seealso cref="Test_ProcessCreateDbUserSystemAdmin.Test_Process_CreateDb_SystemAdmin"/>. This test after verifying that the system user doesn't have permission to the db object,
        /// then turns around and grants permission to the db object and then verifies that the user has permisions to it.
        /// </summary>
        /// <remarks>This test will create both a test database and remove if it already exists as well as wipe the sytem database from disk if it already exists.</remarks>
        [Fact]
        public void Test_Grant_User_Permission_To_Object()
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

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestPermissions");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string userDbName = "TestAuthUserPerm";
            string contracts = "contracts";

            string fileName = Path.Combine(storageFolder, userDbName + hostDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            string dbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            fileName = Path.Combine(storageFolder, dbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var tbName = "TableTestUsers";
            var tbId = 1;
            var dbId = Guid.NewGuid();
            Guid tableObjectId;

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, crypto, xManager);
            var auth = new AuthenticationManager(manager);

            manager.LoadSystemDatabases(cache, storage, crypto);
            auth.SetInitalSystemAdmin(sysLogin, sysLoginPw);
            manager.TryCreateNewHostDatabase(userDbName, out _);
            var db = manager.GetUserDatabase(userDbName);
            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);
            var addTableResult = db.AddTable(tableSchema, out tableObjectId);

            // --- ACT

            bool userIsInSystemRole = auth.IsUserInSystemRole(sysLogin);

            var createUserResult = db.CreateUser(sysLogin, sysLoginPw);
            var hasUser = db.HasUser(sysLogin);
            bool userIsNotPreviouslyAuthorized = db.AuthorizeUser(sysLogin, sysLoginPw, DbPermission.Select, tableObjectId);
            db.GrantUserPermission(sysLogin, DbPermission.Select, tableObjectId);

            // --- ASSERT
            Assert.True(userIsInSystemRole);
            Assert.False(userIsNotPreviouslyAuthorized);
            Assert.True(db.AuthorizeUser(sysLogin, sysLoginPw, DbPermission.Select, tableObjectId));
        }
    }
}
