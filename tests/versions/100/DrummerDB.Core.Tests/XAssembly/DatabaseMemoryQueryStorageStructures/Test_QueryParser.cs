using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.Storage;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_QueryParser
    {
        /// <summary>
        /// Tests <see cref="QueryParser"/> to ensure that it recognizes an invalid SQL statement
        /// </summary>
        [Fact]
        public void Test_Invalid_SQL_Syntax()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Storage
            * Drummersoft.DrummerDB.Core.Memory
            * Drummersoft.DrummerDB.Core.QueryTransaction
            */

            // ---  ARRANGE
            string sqlStatement = "THE QUICK BROWN FOX";



            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestInvalidSyntax");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestInvalidSQL";

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

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());

            var statementHandler = new StatementHandler(manager);
            var parser = new QueryParser(statementHandler);

            manager.XactCreateNewHostDatabase(dbName, out _);

            string errorMessage;

            // ---  ACT
            var result = parser.IsStatementValid(sqlStatement, manager, dbName, out errorMessage);
            Debug.WriteLine(errorMessage);

            // ---  ASSERT
            Assert.False(result);
        }

        /// <summary>
        /// Tests <see cref="QueryParser"/> to ensure that it recognizes the table name in the SQL statement doesn't exist in the database
        /// </summary>
        [Fact]
        public void Test_Invalid_TableName()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             * Drummersoft.DrummerDB.Core.Storage
             * Drummersoft.DrummerDB.Core.Memory
             * Drummersoft.DrummerDB.Core.QueryTransaction
             */

            // ---  ARRANGE
            string sqlStatement = "SELECT * FROM NOTABLE";

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestBadTableName");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestInvalidSQL";

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

            var tbName = "YESTABLE";
            var tbId = 1;
            var dbId = Guid.NewGuid();
            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("COLUMN1", new SQLInt(), 1);
            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());

            var statementHandler = new StatementHandler(manager);
            var parser = new QueryParser(statementHandler);

            manager.XactCreateNewHostDatabase(dbName, out _);

            string errorMessage;

            // ---  ACT
            var result = parser.IsStatementValid(sqlStatement, manager, dbName, out errorMessage);
            Debug.WriteLine(errorMessage);

            // ---  ASSERT
            Assert.False(result);
        }

        /// <summary>
        /// Tests <see cref="QueryParser"/> to ensure that it recognizes the table name in the database does exist
        /// </summary>
        [Fact]
        public void Test_Valid_TableName()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Storage
            * Drummersoft.DrummerDB.Core.Memory
            * Drummersoft.DrummerDB.Core.QueryTransaction
            */

            // ---  ARRANGE
            string sqlStatement = "SELECT COL FROM TESTTABLE";

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestValidTableName");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";

            var dbName = "TestInvalidSQL";

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

            var tbName = "TESTTABLE";
            var tbId = 1;
            var dbId = Guid.NewGuid();
            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("COL", new SQLInt(), 1);
            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());

            var statementHandler = new StatementHandler(manager);
            var parser = new QueryParser(statementHandler);

            manager.XactCreateNewHostDatabase(dbName, out _);
            var db = manager.GetUserDatabase(dbName);
            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);
            var createTableResult = db.AddTable(tableSchema, out _);

            string errorMessage;

            // ---  ACT
            var result = parser.IsStatementValid(sqlStatement, manager, dbName, out errorMessage);

            // ---  ASSERT
            Assert.True(result);
        }

        /// <summary>
        /// Tests <see cref="QueryParser"/> that it recognizes that the column in the SQL statement does not exist in the table in the database
        /// </summary>
        [Fact]
        public void Test_Invalid_ColumnName()
        {

            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             * Drummersoft.DrummerDB.Core.Storage
             * Drummersoft.DrummerDB.Core.Memory
             * Drummersoft.DrummerDB.Core.QueryTransaction
             */

            // ---  ARRANGE
            string sqlStatement = "SELECT COLFOO FROM TESTTABLE";

            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestInvalidColName");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";
            var dbName = "TestInvalidSQL";

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

            var tbName = "TESTTABLE";
            var tbId = 1;
            var dbId = Guid.NewGuid();
            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("COL", new SQLInt(), 1);
            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());


            var statementHandler = new StatementHandler(manager);
            var parser = new QueryParser(statementHandler);

            manager.XactCreateNewHostDatabase(dbName, out _);
            var db = manager.GetUserDatabase(dbName);
            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);
            var createTableResult = db.AddTable(tableSchema, out _);

            string errorMessage;

            // ---  ACT
            var result = parser.IsStatementValid(sqlStatement, manager, dbName, out errorMessage);
            Debug.WriteLine(errorMessage);

            // ---  ASSERT
            Assert.False(result);
        }

        [Fact(Skip ="If Exists Operator Not Written")]
        public void Test_Valid_IfTableExists()
        {
            // -- ARRANGE
            string storageFolder = Path.Combine(TestConstants.TEST_TEMP_FOLDER, "TestDropTableExist");
            string hostDbExtension = ".drum";
            string partDbExtension = ".drumpart";
            string logDbExtension = ".drumlog";
            string systemDbExtension = ".drumsys";
            var dbName = "TestDropTable";
            string contracts = "contracts";
            string contractFileExtension = ".drumContract";
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

            var tbName = "TESTTABLE";
            var tbId = 1;
            var dbId = Guid.NewGuid();
            var cols = new List<ColumnSchema>();
            var col1 = new ColumnSchema("COL", new SQLInt(), 1);
            cols.Add(col1);

            var storage = new StorageManager(storageFolder, hostDbExtension, partDbExtension, logDbExtension, systemDbExtension, contracts, contractFileExtension);
            var cache = new CacheManager();
            var mockCrypto = new MockCryptoManager();
            var xManager = new TransactionEntryManager();
            var manager = new DbManager(storage, cache, mockCrypto, xManager);
            manager.LoadSystemDatabases(cache, storage, mockCrypto, new HostInfo());

            var statementHandler = new StatementHandler(manager);
            var parser = new QueryParser(statementHandler);

            manager.XactCreateNewHostDatabase(dbName, out _);
            var db = manager.GetUserDatabase(dbName);
            var tableSchema = new TableSchema(tbId, tbName, dbId, cols);
            var createTableResult = db.AddTable(tableSchema, out _);

            string sqlDropTableStatement = $@"
            DROP TABLE IF EXISTS {tbName}
            ";

            // -- ACT
            string errorDropMessage = string.Empty;
            var isDropStatementValid = parser.IsStatementValid(sqlDropTableStatement, manager, out errorDropMessage);

            /// -- ASSERT
            Assert.True(isDropStatementValid);
        }
    }
}

