using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_TableCache
    {
        /// <summary>
        /// Tests that adding a row is properly saved in cache.
        /// </summary>
        [Fact]
        public void Test_Table_Cache_AddRow()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Memory
            */

            // --- ARRANGE
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

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns);

            var mockStorage = new MockStorageManager(tableSchema);
            var xManager = new TransactionEntryManager();

            var cacheManager = new CacheManager();

            var table = new Table(tableSchema, cacheManager, mockStorage, xManager);
            mockStorage.SetAddress(table.Address);

            var row = table.GetNewLocalRow();
            int rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "222");
            row.SetValue("Age", "35");

            table.XactAddRow(row);

            var memoryRow = cacheManager.GetRow(rowId, table.Address);

            // --- ACT
            var setName = row.GetValueInString("Name");
            var savedName = memoryRow.GetValueInString("Name");

            // --- ASSERT
            Assert.Equal(setName, savedName);

        }

        /// <summary>
        /// Tests adding multiple rows to a table that should span over multiple <seealso cref="IPage"/>s in a<seealso cref="CacheManager"/>.
        /// </summary>
        [Fact]
        public void Test_Table_Multiple_Pages()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            * Drummersoft.DrummerDB.Core.Memory
            */

            // --- ARRANGE
            int tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            int currentRowInterval = 1;
            int maxInterval = 2000;

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1);
            columns.Add(columnId);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns);

            var mockStorage = new MockStorageManager(tableSchema);
            var xManager = new TransactionEntryManager();

            var cacheManager = new CacheManager();

            var table = new Table(tableSchema, cacheManager, mockStorage, xManager);
            mockStorage.SetAddress(table.Address);

            // --- ACT
            while (currentRowInterval <= maxInterval)
            {
                var row = table.GetNewLocalRow();

                int rowId = row.Id;
                Debug.WriteLine(rowId.ToString());
                Debug.WriteLine(currentRowInterval.ToString());
                Debug.WriteLine(maxInterval.ToString());

                row.SortBinaryOrder();
                row.SetValue("Id", currentRowInterval.ToString());
                table.XactAddRow(row);
                currentRowInterval++;
            }

            var maxRow = cacheManager.GetRow(maxInterval, table.Address);

            // --- ASSERT
            Assert.Equal(maxInterval, maxRow.Id);
        }

        /// <summary>
        /// Tests filling a Page up in Cache and then updating a row to force the update to be forwarded to a different page than
        /// where it started.
        /// </summary>
        [Fact]
        public void Test_Update_Row_Forward_To_New_Page()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             * Drummersoft.DrummerDB.Core.Memory
             */

            // --- ARRANGE
            int tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            int currentRowInterval = 1;
            int maxInterval = 500;

            var columns = new List<ColumnSchema>();

            var nvarchar = new SQLVarChar(4);
            var columnId = new ColumnSchema("Id", nvarchar, 1); ;
            columns.Add(columnId);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns);

            var mockStorage = new MockStorageManager(tableSchema);
            var mockXManager = new TransactionEntryManager();
            var cacheManager = new CacheManager();

            var table = new Table(tableSchema, cacheManager, mockStorage, mockXManager);
            mockStorage.SetAddress(table.Address);

            // --- ACT
            while (currentRowInterval <= maxInterval)
            {
                var row = table.GetNewLocalRow();
                row.SortBinaryOrder();
                row.SetValue("Id", currentRowInterval.ToString());
                table.XactAddRow(row);
                currentRowInterval++;
            }

            currentRowInterval = maxInterval / 2;
            var rowToUpdateId = currentRowInterval;

            currentRowInterval = 0;

            while (currentRowInterval <= maxInterval)
            {
                currentRowInterval++;
                var rowToUpdate = cacheManager.GetRow(rowToUpdateId, table.Address);
                rowToUpdate.SetValue("Id", currentRowInterval.ToString());

                Debug.WriteLine($"Setting {rowToUpdate.Id.ToString()} to value {currentRowInterval.ToString()}");

                cacheManager.UpdateRow(rowToUpdate, table.Address, tableSchema, out _);
            }

            var finalRow = cacheManager.GetRow(rowToUpdateId, table.Address);

            var expected = currentRowInterval.ToString();
            var actual = finalRow.GetValueInString("Id");

            // --- ASSERT
            Assert.Equal(expected, actual);
        }
    }
}
