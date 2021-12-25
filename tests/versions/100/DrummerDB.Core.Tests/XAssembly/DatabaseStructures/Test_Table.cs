using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_Table
    {
        /// <summary>
        /// Tests the create table.
        /// </summary>
        [Fact]
        public void Test_Create_Table()
        {
            int tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockStorage, xManager);

            Assert.Equal(tableName, table.Name);

        }
    }
}
