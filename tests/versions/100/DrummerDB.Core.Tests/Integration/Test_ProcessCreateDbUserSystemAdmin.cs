using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Systems;
using Drummersoft.DrummerDB.Core.Tests.XAssembly;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.Integration
{
    public class Test_ProcessCreateDbUserSystemAdmin
    {
        /// <summary>
        /// Tests standing up a process and ensuring that a system user can access newly created objects.
        /// 
        /// The relationship between system logins (managed by <see cref="IAuthenticationManager"/>) and db permissions is being tested here. Ideally, we should be 
        /// verifying all permission directly thru <see cref="IAuthenticationManager"/>. The other tests in the remarks highlight this, where a db may grant permission
        /// but the Auth Manager has no idea about it.
        /// </summary>
        /// <remarks>This test is closely related to the test in the
        /// <seealso cref="Test_DbTableMetadataUserAuth"/> class.
        /// </remarks>
        [Fact]
        public void Test_Process_CreateDb_SystemAdmin()
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

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestDbSysAdmin");

            DirectoryInfo directory = new DirectoryInfo(storageFolder);

            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var file in directory.EnumerateFiles())
            {
                file.Delete();
            }

            // --- ARRANGE
            string sysLogin = "testSystem";
            string sysLoginPw = "testSystemPw";
            string userDbName = "GrantAdminPerms";

            var process = new Process(storageFolder, true, false);
            process.Start();
            var authManager = process.AuthenticationManager;
            var dbManager = process.DbManager;

            // --- ACT
            authManager.SetInitalSystemAdmin(sysLogin, sysLoginPw);
            Guid dbId;
            dbManager.XactCreateNewHostDatabase(userDbName, out dbId);

            var tbName = "TableTestUsers";
            var tbId = 1;
            Guid tableObjectId;

            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("col1", new SQLInt(), 1);

            cols.Add(col1);

            var db = dbManager.GetUserDatabase(userDbName, DatabaseType.Host);
            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);
            db.AddTable(tableSchema, out tableObjectId);

            // --- ASSERT
            Assert.True(authManager.IsUserInSystemRole(sysLogin));

            // this should return true, because the auth manager first checks to see if the user is an admin of the system, which it is, therefore it has rights
            // to any database object. The test "Test_DbTableMetadataUserAuth" shows how these are seperate concepts (system level permissions and db permissions).
            Assert.True(authManager.UserHasDbPermission(sysLogin, sysLoginPw, userDbName, DbPermission.Select, tableObjectId));
        }
    }
}
