using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Cryptography;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Version;
using Drummersoft.DrummerDB.Core.IdentityAccess;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_QueryManager
    {
        private DbManager _dbManager;

        /// <summary>
        /// Sets up a common set of data for Query Manager tests
        /// </summary>
        /// <param name="userDbName">The datbase name</param>
        /// <param name="userName">The user requesting the query</param>
        /// <param name="password">The pw of the user</param>
        /// <param name="userSesionId">The session id - currently, this is just a new GUID</param>
        /// <returns>A <see cref="QueryManager"/> populated with 5 rows of data</returns>
        private QueryManager Arrange_Query_Manager(string userDbName, string userName, string password, Guid userSesionId)
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
            * Drummerosft.DrummerDB.Core.QueryTransaction
            * Drummersoft.DrummerDB.Core.QueryTransaction.Structures
            */

            // --- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestQueryManager");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string systemDbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            string fileName = Path.Combine(storageFolder, userDbName + hostDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            fileName = Path.Combine(storageFolder, userDbName + logDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            fileName = Path.Combine(storageFolder, systemDbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            fileName = Path.Combine(storageFolder, systemDbName + logDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, crypto, xManager);
            var auth = new AuthenticationManager(manager);

            _dbManager = manager;

            manager.LoadSystemDatabases(cache, storage, crypto);
            Guid dbId;
            manager.TryCreateNewHostDatabase(userDbName, out dbId);

            var db = manager.GetUserDatabase(userDbName);
            manager.CreateAdminLogin(userName, password, Guid.NewGuid());

            int tableId = 990;
            string tableName = "TestTable";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1);

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4, true);

            var sqlIntRank = new SQLInt();
            var columnRank = new ColumnSchema("Rank", sqlIntRank, 5, true);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);
            columns.Add(columnRank);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns);

            Guid tableGuidId;
            bool tableCreated = db.AddTable(tableSchema, out tableGuidId);

            if (tableCreated)
            {
                var table = db.GetTable(tableName);
                var row = table.GetNewLocalRow();

                int rowId = row.Id;
                row.SortBinaryOrder();

                row.SetValue("Name", "Randy");
                row.SetValue("Id", "1");
                row.SetValue("Age", "-1");
                row.SetValue("NickName", "Randster");
                row.SetValue("Rank", "1");

                table.TryAddRow(row);

                var row2 = table.GetNewLocalRow();
                row2.SortBinaryOrder();

                int row2Id = row2.Id;
                row2.SortBinaryOrder();

                row2.SetValue("Name", "Megan");
                row2.SetValue("Id", "888");
                row2.SetValue("Age", "36");
                row2.SetValue("NickName", "Way");
                row2.SetValue("Rank", "1");

                table.TryAddRow(row2);

                var row3 = table.GetNewLocalRow();
                row3.SortBinaryOrder();

                int row3Id = row3.Id;
                row3.SortBinaryOrder();

                row3.SetValue("Name", "Mark");
                row3.SetValue("Id", "777");
                row3.SetValue("Age", "34");
                row3.SetValue("NickName", "Marky");
                row3.SetValue("Rank", "6");

                table.TryAddRow(row3);

                var row4 = table.GetNewLocalRow();
                row4.SortBinaryOrder();

                int row4Id = row4.Id;
                row4.SortBinaryOrder();

                row4.SetValue("Name", "Jennifer");
                row4.SetValue("Id", "535");
                row4.SetValue("Age", "26");
                row4.SetValue("NickName", "Jen");
                row4.SetValue("Rank", "55");

                table.TryAddRow(row4);

                var row5 = table.GetNewLocalRow();
                row5.SortBinaryOrder();

                int row5Id = row5.Id;
                row4.SortBinaryOrder();

                row5.SetValue("Name", "Jackie");
                row5.SetValue("Id", "555");
                row5.SetValue("Age", "27");
                row5.SetValue("NickName", "Jack");
                row5.SetValue("Rank", "44");

                table.TryAddRow(row5);
            }

            var queryManager = new QueryManager(manager, auth, xManager);

            return queryManager;
        }

        [Fact]
        public void Test_Generate_Execute_Select_Basic_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestQueryPlan";
            Guid userSessionId = Guid.NewGuid();
            string sqlStatement = $"SELECT Id, Name, Age, NickName, Rank FROM TestTable";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            // --- ACT
            string errorMessage = string.Empty;
            var isStatementValid = queryManager.IsStatementValid(sqlStatement, userDbName, out errorMessage);
            var results = queryManager.ExecuteValidatedStatement(sqlStatement, userDbName, userName, password, userSessionId);

            var row1NameValue = results.Rows[0][1].Value;
            var randyByteNameValue = DbBinaryConvert.StringToBinary("Randy");

            // --- ASSERT
            Assert.True(isStatementValid);
            Assert.InRange<int>(results.Rows.Count, 1, 5);
            Assert.Equal(randyByteNameValue, row1NameValue);
        }

        [Fact]
        public void Test_Generate_Execute_Select_Where_Query_Plan()
        {
            // -- ARRANGE
            string userDbName = "TestQueryPlan";
            string userName = "Tester";
            string password = "TestPass";
            Guid userSessionId = Guid.NewGuid();

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            string sqlStatement = $"SELECT Id, Name, Age, NickName, Rank FROM TestTable WHERE Name = 'Megan'";

            // --- ACT
            string errorMessage = string.Empty;
            var isStatementValid = queryManager.IsStatementValid(sqlStatement, userDbName, out errorMessage);
            var results = queryManager.ExecuteValidatedStatement(sqlStatement, userDbName, userName, password, userSessionId);

            byte[] row1RankValue = results.Rows[0][4].Value;
            byte[] meganRankValue = DbBinaryConvert.IntToBinary(1);

            // need to trim the leading IS NULL prefix
            byte[] row1resizedValue = new byte[4];
            Array.Copy(row1RankValue, 1, row1resizedValue, 0, row1resizedValue.Length);

            // --- ASSERT
            Assert.True(isStatementValid);
            Assert.InRange<int>(results.Rows.Count, 1, 1);
            Assert.Equal(meganRankValue, row1resizedValue);
        }

        [Fact]
        public void Test_Generate_Execute_Select_Where_Multiple_Query_Plan()
        {
            // -- ARRANGE
            string userDbName = "TestMultiQueryPlan";
            string userName = "Tester";
            string password = "TestPass";
            Guid userSessionId = Guid.NewGuid();

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            string sqlStatement = $"SELECT Id, Name, Age, NickName, Rank FROM TestTable WHERE (Rank = 1 AND Name = 'Megan') OR Rank > 20";

            // --- ACT
            string errorMessage = string.Empty;
            var isStatementValid = queryManager.IsStatementValid(sqlStatement, userDbName, out errorMessage);
            var results = queryManager.ExecuteValidatedStatement(sqlStatement, userDbName, userName, password, userSessionId);

            // should return MEGAN, JENNIFER, JACKIE
            foreach (var row in results.Rows)
            {
                var span = new ReadOnlySpan<byte>(row[1].Value);
                var name = DbBinaryConvert.BinaryToString(span);
                Debug.WriteLine(name);
            }

            // --- ASSERT
            Assert.True(isStatementValid);
            Assert.InRange<int>(results.Rows.Count, 3, 3);

            var span0 = new ReadOnlySpan<byte>(results.Rows[0][1].Value);
            var meganName = DbBinaryConvert.BinaryToString(span0);

            Assert.Equal("Megan", meganName);

            var span1 = new ReadOnlySpan<byte>(results.Rows[1][1].Value);
            var jenniferName = DbBinaryConvert.BinaryToString(span1);

            Assert.Equal("Jennifer", jenniferName);

            var span2 = new ReadOnlySpan<byte>(results.Rows[2][1].Value);
            var jackieName = DbBinaryConvert.BinaryToString(span2);

            Assert.Equal("Jackie", jackieName);
        }

        [Fact]
        public void Test_Generate_Execute_Create_Db_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestMakeDb";
            Guid userSessionId = Guid.NewGuid();
            string createdDbName = "DbFoo";
            string sqlStatement = $"CREATE DATABASE {createdDbName}";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            // --- ACT
            string errorMessage = string.Empty;
            var isStatementValid = queryManager.IsStatementValid(sqlStatement, userDbName, out errorMessage);
            var results = queryManager.ExecuteValidatedStatement(sqlStatement, userDbName, userName, password, userSessionId);

            // --- ASSERT
            Assert.True(_dbManager.HasUserDatabase(createdDbName));
        }

        [Fact]
        public void Test_Generate_Execute_Drop_Db_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestDrop";
            Guid userSessionId = Guid.NewGuid();
            string createdDbName = "DropIt";
            string sqlCreateStatement = $"CREATE DATABASE {createdDbName}";
            string sqlDropStatement = $"DROP DATABASE {createdDbName}";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            // --- ACT
            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateStatement, userDbName, out errorCreateMessage);
            var createResult = queryManager.ExecuteValidatedStatement(sqlCreateStatement, userDbName, userName, password, userSessionId);
            var dbWasCreated = _dbManager.HasUserDatabase(createdDbName);

            string errorDropMessage = string.Empty;
            var isDropStatementValid = queryManager.IsStatementValid(sqlDropStatement, userDbName, out errorDropMessage);
            var dropResult = queryManager.ExecuteValidatedStatement(sqlDropStatement, userDbName, userName, password, userSessionId);
            var dbWasDropped = _dbManager.HasUserDatabase(createdDbName);

            // --- ASSERT
            Assert.True(dbWasCreated);
            Assert.False(dbWasDropped);
        }

        [Fact]
        public void Test_Generate_Execute_Create_Table_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestCreateT";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT IDENTITY(1,1),
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string sqlSelectFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName}";
            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT
            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            string errorSelectMessage = string.Empty;
            var isSelectStatementValid = queryManager.IsStatementValid(sqlSelectFromTable, userDbName, out errorSelectMessage);
            var selectTableResult = queryManager.ExecuteValidatedStatement(sqlSelectFromTable, userDbName, userName, password, userSessionId);

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(tableWasCreated);
            Assert.InRange(selectTableResult.Rows.Count, 0, 0);
            Assert.InRange(selectTableResult.Columns.Count(), 4, 4);
        }

        [Fact]
        public void Test_Generate_Execute_Insert_Table_Values_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestInsert";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";

            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string sqlInsertStatement = $@"
            INSERT INTO {createdTableName}
            (
                ID,
                EMPLOYEENAME,
                HIREDATE,
                TERMDATE
            )
            VALUES
            (
                1,
                'Brandon',
                '2021-09-13',
                '2099-01-01'
            ),
(               2,
                'Bobby',
                '2021-09-01',
                '2099-01-01'
            )
            ";

            string sqlSelectFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName}";

            string sqlSelectBrandonFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName} WHERE EMPLOYEENAME = 'Brandon'";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT
            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            string errorInsertMessage = string.Empty;
            var isInsertStatementValid = queryManager.IsStatementValid(sqlInsertStatement, userDbName, out errorInsertMessage);
            var insertTableResult = queryManager.ExecuteValidatedStatement(sqlInsertStatement, userDbName, userName, password, userSessionId);

            string errorSelectMessage = string.Empty;
            var isSelectStatementValid = queryManager.IsStatementValid(sqlSelectFromTable, userDbName, out errorSelectMessage);
            var selectTableResult = queryManager.ExecuteValidatedStatement(sqlSelectFromTable, userDbName, userName, password, userSessionId);

            string errorBrandonSelectMessage = string.Empty;
            var isSelectBranodnStatementValid = queryManager.IsStatementValid(sqlSelectBrandonFromTable, userDbName, out errorSelectMessage);
            var selectBrandonTableResult = queryManager.ExecuteValidatedStatement(sqlSelectBrandonFromTable, userDbName, userName, password, userSessionId);

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(isInsertStatementValid);
            Assert.True(tableWasCreated);
            Assert.InRange(selectTableResult.Rows.Count, 2, 2);
            Assert.InRange(selectTableResult.Columns.Count(), 4, 4);

            var brandonHireBinary = DbBinaryConvert.DateTimeToBinary("2021-09-13");
            var returnedBrandonHireDate = selectBrandonTableResult.Rows[0][2].Value;

            Assert.Equal(brandonHireBinary, returnedBrandonHireDate);
        }

        [Fact(Skip = "Insert operator not written")]
        public void Test_Generate_Execute_Insert_Table_Select_Query_Plan()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Test_Generate_Execute_Update_All_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestUpdateW";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            DateTime now = DateTime.Now;

            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string sqlInsertStatement = $@"
            INSERT INTO {createdTableName}
            (
                ID,
                EMPLOYEENAME,
                HIREDATE,
                TERMDATE
            )
            VALUES
            (
                1,
                'Brandon',
                '2021-09-13',
                '2099-01-01'
            ),
(               2,
                'Bobby',
                '2021-09-01',
                '2099-01-01'
            )
            ";

            string sqlSelectFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName}";

            string nowDate = now.ToShortDateString();

            string sqlUpdateStatement = $@"
            UPDATE {createdTableName}
            SET HIREDATE = '{nowDate}'
            ";

            string sqlSelectEmployees = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName}";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT
            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            string errorInsertMessage = string.Empty;
            var isInsertStatementValid = queryManager.IsStatementValid(sqlInsertStatement, userDbName, out errorInsertMessage);
            var insertTableResult = queryManager.ExecuteValidatedStatement(sqlInsertStatement, userDbName, userName, password, userSessionId);

            string errorSelectMessage = string.Empty;
            var isSelectStatementValid = queryManager.IsStatementValid(sqlSelectFromTable, userDbName, out errorSelectMessage);
            var selectTableResult = queryManager.ExecuteValidatedStatement(sqlSelectFromTable, userDbName, userName, password, userSessionId);

            string errorUpdateMessage = string.Empty;
            var isUpdateStatementValid = queryManager.IsStatementValid(sqlUpdateStatement, userDbName, out errorSelectMessage);
            var updateTableResult = queryManager.ExecuteValidatedStatement(sqlUpdateStatement, userDbName, userName, password, userSessionId);

            string errorSelectRevisedMessage = string.Empty;
            var isSelectRevisedStatementValid = queryManager.IsStatementValid(sqlSelectEmployees, userDbName, out errorSelectMessage);
            var selectRevisedTableResult = queryManager.ExecuteValidatedStatement(sqlSelectEmployees, userDbName, userName, password, userSessionId);

            var bobbyHireDate = selectRevisedTableResult.Rows[0][2].Value;
            var brandonHireDate = selectRevisedTableResult.Rows[1][2].Value;

            var bobbyRevisedHireDate = DbBinaryConvert.BinaryToDateTime(bobbyHireDate);
            var brandonRevisedHireDate = DbBinaryConvert.BinaryToDateTime(brandonHireDate);

            var expectedHireDate = now.Date;

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(isInsertStatementValid);
            Assert.True(tableWasCreated);
            Assert.InRange(selectTableResult.Rows.Count, 2, 2);
            Assert.InRange(selectTableResult.Columns.Count(), 4, 4);
            Assert.InRange(selectRevisedTableResult.Rows.Count, 2, 2);
            Assert.Equal(expectedHireDate, bobbyRevisedHireDate);
            Assert.Equal(expectedHireDate, brandonRevisedHireDate);
        }

        [Fact]
        public void Test_Generate_Execute_Update_Where_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestUpdateW";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            DateTime now = DateTime.Now;

            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string sqlInsertStatement = $@"
            INSERT INTO {createdTableName}
            (
                ID,
                EMPLOYEENAME,
                HIREDATE,
                TERMDATE
            )
            VALUES
            (
                1,
                'Brandon',
                '2021-09-13',
                '2099-01-01'
            ),
(               2,
                'Bobby',
                '2021-09-01',
                '2099-01-01'
            )
            ";

            string sqlSelectFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName}";

            string nowDate = now.ToShortDateString();

            string sqlUpdateStatement = $@"
            UPDATE {createdTableName}
            SET HIREDATE = '{nowDate}'
            WHERE EMPLOYEENAME = 'Bobby'
            ";

            string sqlSelectBobbyFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName} WHERE EMPLOYEENAME = 'Bobby'";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT
            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            string errorInsertMessage = string.Empty;
            var isInsertStatementValid = queryManager.IsStatementValid(sqlInsertStatement, userDbName, out errorInsertMessage);
            var insertTableResult = queryManager.ExecuteValidatedStatement(sqlInsertStatement, userDbName, userName, password, userSessionId);

            string errorSelectMessage = string.Empty;
            var isSelectStatementValid = queryManager.IsStatementValid(sqlSelectFromTable, userDbName, out errorSelectMessage);
            var selectTableResult = queryManager.ExecuteValidatedStatement(sqlSelectFromTable, userDbName, userName, password, userSessionId);

            string errorUpdateMessage = string.Empty;
            var isUpdateStatementValid = queryManager.IsStatementValid(sqlUpdateStatement, userDbName, out errorSelectMessage);
            var updateTableResult = queryManager.ExecuteValidatedStatement(sqlUpdateStatement, userDbName, userName, password, userSessionId);

            string errorSelectRevisedMessage = string.Empty;
            var isSelectRevisedStatementValid = queryManager.IsStatementValid(sqlSelectBobbyFromTable, userDbName, out errorSelectMessage);
            var selectRevisedTableResult = queryManager.ExecuteValidatedStatement(sqlSelectBobbyFromTable, userDbName, userName, password, userSessionId);

            var bobbyHireDate = selectRevisedTableResult.Rows[0][2].Value;

            var bobbyRevisedHireDate = DbBinaryConvert.BinaryToDateTime(bobbyHireDate);
            var expectedHireDate = now.Date;

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(isInsertStatementValid);
            Assert.True(tableWasCreated);
            Assert.InRange(selectTableResult.Rows.Count, 2, 2);
            Assert.InRange(selectTableResult.Columns.Count(), 4, 4);
            Assert.InRange(selectRevisedTableResult.Rows.Count, 1, 1);
            Assert.Equal(expectedHireDate, bobbyRevisedHireDate);
        }

        [Fact]
        public void Test_Generate_Execute_Delete_Where_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestUpdateW";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            DateTime now = DateTime.Now;

            DateTime brandonHireDate = DateTime.Parse("2021-09-13");

            var brandonHireDateString = brandonHireDate.Date.ToString();

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT

            // Create Table
            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            // Insert Employees Into Table
            string sqlInsertStatement = $@"
            INSERT INTO {createdTableName}
            (
                ID,
                EMPLOYEENAME,
                HIREDATE,
                TERMDATE
            )
            VALUES
            (
                1,
                'Brandon',
                '{brandonHireDate.Date.ToString()}',
                '2099-01-01'
            ),
(               2,
                'Bobby',
                '2021-09-01',
                '2099-01-01'
            )
            ";

            string errorInsertMessage = string.Empty;
            var isInsertStatementValid = queryManager.IsStatementValid(sqlInsertStatement, userDbName, out errorInsertMessage);
            var insertTableResult = queryManager.ExecuteValidatedStatement(sqlInsertStatement, userDbName, userName, password, userSessionId);

            // Select Employees From Table
            string sqlSelectEmployees = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName} ";

            string selectErrorMessage1 = string.Empty;
            var isSelectAllEmployeesValid1 = queryManager.IsStatementValid(sqlSelectEmployees, userDbName, out selectErrorMessage1);
            var selectEmployeesResult1 = queryManager.ExecuteValidatedStatement(sqlSelectEmployees, userDbName, userName, password, userSessionId);

            // Delete Employee Bobby From Table
            string sqlDeleteBobbyFromTable = $"DELETE FROM {createdTableName} WHERE EMPLOYEENAME = 'Bobby'";

            string errorDeleteMessage = string.Empty;
            var isDeleteStatementValid = queryManager.IsStatementValid(sqlDeleteBobbyFromTable, userDbName, out errorDeleteMessage);
            var deleteResult = queryManager.ExecuteValidatedStatement(sqlDeleteBobbyFromTable, userDbName, userName, password, userSessionId);

            // Select Employees Again from Table
            string selectErrorMessage2 = string.Empty;
            var isSelectAllEmployeesValid2 = queryManager.IsStatementValid(sqlSelectEmployees, userDbName, out selectErrorMessage2);
            var selectEmployeesResult2 = queryManager.ExecuteValidatedStatement(sqlSelectEmployees, userDbName, userName, password, userSessionId);

            var brandonReturnedHireDate = selectEmployeesResult2.Rows[0][2].Value;
            var brandonRevisedHireDate = DbBinaryConvert.BinaryToDateTime(brandonReturnedHireDate);

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(tableWasCreated);
            Assert.True(isInsertStatementValid);
            Assert.True(isSelectAllEmployeesValid1);
            Assert.InRange(selectEmployeesResult1.Rows.Count, 2, 2);
            Assert.True(isDeleteStatementValid);
            Assert.InRange(selectEmployeesResult2.Rows.Count, 1, 1);
            Assert.Equal(brandonHireDate, brandonRevisedHireDate);
        }

        [Fact]
        public void Test_Generate_Execute_Delete_All_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestUpdateW";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            DateTime now = DateTime.Now;

            DateTime brandonHireDate = DateTime.Parse("2021-09-13");

            var brandonHireDateString = brandonHireDate.Date.ToString();

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT

            // Create Table
            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            // Insert Employees Into Table
            string sqlInsertStatement = $@"
            INSERT INTO {createdTableName}
            (
                ID,
                EMPLOYEENAME,
                HIREDATE,
                TERMDATE
            )
            VALUES
            (
                1,
                'Brandon',
                '{brandonHireDate.Date.ToString()}',
                '2099-01-01'
            ),
(               2,
                'Bobby',
                '2021-09-01',
                '2099-01-01'
            )
            ";

            string errorInsertMessage = string.Empty;
            var isInsertStatementValid = queryManager.IsStatementValid(sqlInsertStatement, userDbName, out errorInsertMessage);
            var insertTableResult = queryManager.ExecuteValidatedStatement(sqlInsertStatement, userDbName, userName, password, userSessionId);

            // Select Employees From Table
            string sqlSelectEmployees = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName} ";

            string selectErrorMessage1 = string.Empty;
            var isSelectAllEmployeesValid1 = queryManager.IsStatementValid(sqlSelectEmployees, userDbName, out selectErrorMessage1);
            var selectEmployeesResult1 = queryManager.ExecuteValidatedStatement(sqlSelectEmployees, userDbName, userName, password, userSessionId);

            // Delete Employee Bobby From Table
            string sqlDeleteBobbyFromTable = $"DELETE FROM {createdTableName}";

            string errorDeleteMessage = string.Empty;
            var isDeleteStatementValid = queryManager.IsStatementValid(sqlDeleteBobbyFromTable, userDbName, out errorDeleteMessage);
            var deleteResult = queryManager.ExecuteValidatedStatement(sqlDeleteBobbyFromTable, userDbName, userName, password, userSessionId);

            // Select Employees Again from Table
            string selectErrorMessage2 = string.Empty;
            var isSelectAllEmployeesValid2 = queryManager.IsStatementValid(sqlSelectEmployees, userDbName, out selectErrorMessage2);
            var selectEmployeesResult2 = queryManager.ExecuteValidatedStatement(sqlSelectEmployees, userDbName, userName, password, userSessionId);

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(tableWasCreated);
            Assert.True(isInsertStatementValid);
            Assert.True(isSelectAllEmployeesValid1);
            Assert.InRange(selectEmployeesResult1.Rows.Count, 2, 2);
            Assert.True(isDeleteStatementValid);
            Assert.InRange(selectEmployeesResult2.Rows.Count, 0, 0);
        }

        [Fact]
        public void Test_Generate_Select_Select_Star_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestSelectStar";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            DateTime now = DateTime.Now;

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT

            // Create Table
            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            // Add Employees
            string sqlInsertStatement = $@"
            INSERT INTO {createdTableName}
            (
                ID,
                EMPLOYEENAME,
                HIREDATE,
                TERMDATE
            )
            VALUES
            (
                1,
                'Brandon',
                '2021-10-07',
                '2099-01-01'
            ),
(               2,
                'Bobby',
                '2021-09-01',
                '2099-01-01'
            )
            ";

            string errorInsertMessage = string.Empty;
            var isInsertStatementValid = queryManager.IsStatementValid(sqlInsertStatement, userDbName, out errorInsertMessage);
            var insertTableResult = queryManager.ExecuteValidatedStatement(sqlInsertStatement, userDbName, userName, password, userSessionId);

            // select everything back out
            string sqlSelectStar = $"SELECT * FROM {createdTableName}";

            string errorSelectStarMessage = string.Empty;
            var isSelectStarValid = queryManager.IsStatementValid(sqlSelectStar, userDbName, out errorSelectStarMessage);
            var selectStarResult = queryManager.ExecuteValidatedStatement(sqlSelectStar, userDbName, userName, password, userSessionId);

            // --- ASSERT

            // should be 4 columns
            Assert.Equal(4, selectStarResult.Columns.Count());
            // should be 2 rows
            Assert.Equal(2, selectStarResult.Rows.Count());
        }

        [Fact(Skip = "Drop table operator not written yet")]
        public void Test_Generate_Execute_Drop_Table_Query_Plan()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "System schema not implemented yet")]
        public void Test_Generate_Select_Databases_From_System_Schema_Query_Plan()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "System schema not implemented yet")]
        public void Test_Generate_Select_Tables_From_System_Schema_Query_Plan()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "System schema not implemented yet")]
        public void Test_Generate_Select_Table_Schema_From_System_Schema_Query_Plan()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Test_Generate_Create_Schema_Query_Plan()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestCreateSchema";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "EMPLOYEE";
            string createdSchemaName = "hr";
            DateTime now = DateTime.Now;

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);
            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT
            string sqlCreateSchemaStatement = $"CREATE SCHEMA {createdSchemaName}";
            string errorCreateSchema = string.Empty;
            var isCreateSchemaStatementValid = queryManager.IsStatementValid(sqlCreateSchemaStatement, userDbName, out errorCreateSchema);
            var createSchemaResult = queryManager.ExecuteValidatedStatement(sqlCreateSchemaStatement, userDbName, userName, password, userSessionId);


            // Create table in schemaschema
            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdSchemaName}.{createdTableName}
            (
                ID INT,
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            UserDatabase db = _dbManager.GetUserDatabase(userDbName);
            Assert.True(db.HasSchema(createdSchemaName));
            Assert.True(db.HasTable(createdTableName, createdSchemaName));
        }

        [Fact]
        public void Test_Generate_Get_Loaded_Databases()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestDb";
            Guid userSessionId = Guid.NewGuid();

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestLoadDbs");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string systemDbName = SystemDatabaseConstants100.Databases.DRUM_SYSTEM;

            string fileName = Path.Combine(storageFolder, userDbName + hostDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            fileName = Path.Combine(storageFolder, userDbName + logDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            fileName = Path.Combine(storageFolder, systemDbName + systemDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            fileName = Path.Combine(storageFolder, systemDbName + logDbExtension);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension);
            var cache = new CacheManager();
            var crypto = new CryptoManager();
            var xManager = new TransactionEntryManager();
            var dbManager = new DbManager(storage, cache, crypto, xManager);
            var auth = new AuthenticationManager(dbManager);

            dbManager.LoadSystemDatabases(cache, storage, crypto);

            dbManager.TryCreateNewHostDatabase(userDbName, out _);
            dbManager.TryCreateNewHostDatabase("TestDb2", out _);
            dbManager.TryCreateNewHostDatabase("TestDb3", out _);
            dbManager.TryCreateNewHostDatabase("TestDb4", out _);

            var db = dbManager.GetUserDatabase(userDbName);
            dbManager.CreateAdminLogin(userName, password, Guid.NewGuid());

            var queryManager = new QueryManager(dbManager, auth, xManager);

            // --- ACT
            string dbName = "drumSystem";
            string sqlSelectDatabasesStatement = $@"
            SELECT * FROM sys.Databases";

            string errorSelectDatabases = string.Empty;
            var isSelectDatabasesValid = queryManager.IsStatementValid(sqlSelectDatabasesStatement, dbName, out errorSelectDatabases);
            var selectDatabasesResult = queryManager.ExecuteValidatedStatement(sqlSelectDatabasesStatement, dbName, userName, password, userSessionId);

            foreach (var row in selectDatabasesResult.Rows)
            {
                string result = DbBinaryConvert.BinaryToString(row[0].Value);
                Debug.WriteLine(result);
            }

            Assert.InRange(selectDatabasesResult.Rows.Count(), 4, 4);
        }

        [Fact(Skip = "If Exists Operator Not Written")]
        public void Test_IfTableExistsOperator()
        {
            // -- ARRANGE
            string userName = "Tester";
            string password = "TestPass";
            string userDbName = "TestCreateT";
            Guid userSessionId = Guid.NewGuid();
            string createdTableName = "PEOPLE";
            string sqlCreateTableStatement = $@"
            CREATE TABLE {createdTableName}
            (
                ID INT IDENTITY(1,1),
                EMPLOYEENAME NVARCHAR(25) NOT NULL,
                HIREDATE DATETIME NOT NULL,
                TERMDATE DATETIME NULL
            );
            ";

            string sqlSelectFromTable = $"SELECT ID, EMPLOYEENAME, HIREDATE, TERMDATE FROM {createdTableName}";

            string sqlDropTableStatement = $@"
            DROP TABLE IF EXISTS{createdTableName}
            ";

            var queryManager = Arrange_Query_Manager(userDbName, userName, password, userSessionId);

            var dbExists = _dbManager.HasUserDatabase(userDbName);

            // --- ACT
            string errorCreateMessage = string.Empty;
            var isCreateStatementValid = queryManager.IsStatementValid(sqlCreateTableStatement, userDbName, out errorCreateMessage);
            var createTableResult = queryManager.ExecuteValidatedStatement(sqlCreateTableStatement, userDbName, userName, password, userSessionId);

            var db = _dbManager.GetUserDatabase(userDbName);
            var tableWasCreated = db.HasTable(createdTableName);

            string errorSelectMessage = string.Empty;
            var isSelectStatementValid = queryManager.IsStatementValid(sqlSelectFromTable, userDbName, out errorSelectMessage);
            var selectTableResult = queryManager.ExecuteValidatedStatement(sqlSelectFromTable, userDbName, userName, password, userSessionId);

            // -- ASSERT
            Assert.True(isCreateStatementValid);
            Assert.True(tableWasCreated);
            Assert.InRange(selectTableResult.Rows.Count, 0, 0);
            Assert.InRange(selectTableResult.Columns.Count(), 4, 4);

            string errorDropMessage = string.Empty;
            var isDropStatementValid = queryManager.IsStatementValid(sqlDropTableStatement, userDbName, out errorDropMessage);
            var dropTableResult = queryManager.ExecuteValidatedStatement(sqlDropTableStatement, userDbName, userName, password, userSessionId);

            var tableWasDropped = db.HasTable(createdTableName);

            Assert.False(tableWasDropped);
        }
    }
}
