using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Memory;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.XAssembly
{
    public class Test_TableRowPage
    {
        /// <summary>
        /// Creates a UserDataPage and a Table/Row.
        /// 
        /// Tests the internals of a Page.
        /// 
        /// Assert:
        /// Checks to see if the rowcount on the page is incremented by 1 when a row is added.
        /// </summary>
        [Fact]
        public void Test_Add_Row_To_Page()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var page = new UserDataPage100(new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID)), tableSchema);

            // --- ACT
            page.AddRow(row);

            // --- ASSERT
            Assert.Equal(1, (int)page.TotalRows());
        }

        /// <summary>
        /// Creates a Page and a Table/Row. Checks to ensure that the returned row size matches the calculated row size value.
        /// 
        /// Tests the internals of a Page
        /// 
        /// Assert: 
        /// The total bytes used on the Page should match the size of the row that was added
        /// </summary>
        [Fact]
        public void Test_Page_Row_Add_Bytes()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);

            // --- ACT
            page.AddRow(row);
            uint totalBytes = page.TotalBytesUsed();
            uint rowSize = row.Size();

            // --- ASSERT
            Assert.Equal(rowSize, totalBytes);
        }


        /// <summary>
        /// Tests adding multiple rows to a page from a table to ensure that the binary array layout is working.
        /// 
        /// Tests the internals of a Page.
        /// 
        /// Assert:
        /// The value of the third row added should match the expected value.
        /// </summary>
        [Fact]
        public void Test_Add_Multiple_Rows_Get_Row()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);

            // --- ACT
            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);

            // --- ACT
            page.AddRow(row2);

            uint rowSize = row.Size();
            uint row2Size = row2.Size();

            uint totalBytes = page.TotalBytesUsed();


            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);

            // --- ACT
            page.AddRow(row3);

            var returnedrow = page.GetRow(row3Id) as LocalRow;

            // --- ASSERT
            Assert.Equal(row3NickName, returnedrow.GetValueInString("NickName"));
        }

        /// <summary>
        /// Tests a Page, Row, and Table. Checks to see that after adding multiple rows to a Page, that the value in the middle of the page is correct.
        /// Bassically ensures that the byte array layout of the page is correct and I/O for it is working properly. 
        ///
        /// Assert:
        /// The value of the middle row on the page should match what was set up.
        /// </summary>
        [Fact]
        public void Test_Add_Multiple_Rows_Get_Row_Middle()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);

            // --- ACT
            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);

            // --- ACT
            page.AddRow(row2);

            uint rowSize = row.Size();
            uint row2Size = row2.Size();

            uint totalBytes = page.TotalBytesUsed();


            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);

            // --- ACT
            page.AddRow(row3);

            var returnedrow = page.GetRow(row2Id) as LocalRow;

            // --- ASSERT
            Assert.Equal(row2NickName, returnedrow.GetValueInString("NickName"));
        }


        /// <summary>
        /// Tests the <seealso cref="UserDataPage100.IsFull(int)"/> method to see if bounds checking on the page is working.
        /// 
        /// Tries to keep adding rows until the page returns that it is full.
        /// 
        /// Assert:
        /// The total number of rows that were sent in should match the total number of rows on the page.
        /// </summary>
        /// <remarks>For more information on how Pages work, see Page.md</remarks> 
        [Fact]
        public void Test_Page_Bounds()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            uint currentRowInterval = 0;

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;
            columns.Add(columnId);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            row.SortBinaryOrder();

            row.SetValue("Id", currentRowInterval.ToString());

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);

            // --- ACT
            while (!page.IsFull(row.Size()))
            {
                page.AddRow(row);
                currentRowInterval += 1;
                row = table.GetNewLocalRow();
                row.SortBinaryOrder();
                row.SetValue("Id", currentRowInterval.ToString());
            }

            // --- ASSERT
            Assert.Equal(currentRowInterval, page.TotalRows());
        }


        /// <summary>
        /// Tests the <seealso cref="IRow.IsDeleted"/> flag is working when retriving data from a <seealso cref="UserDataPage100"/>.
        /// 
        /// Tries to validate the binary layout on the page by adding multiple rows, then deleting the middle row.
        /// 
        /// Assert:
        /// The row that was deleted from the page should report deleted.
        /// </summary>
        /// <remarks>For more information on how Pages work, see Page.md</remarks>
        [Fact]
        public void Test_Page_Delete_Row_Middle()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);
            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);

            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);

            page.AddRow(row2);

            uint rowSize = row.Size();
            uint row2Size = row2.Size();

            uint totalBytes = page.TotalBytesUsed();

            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);

            page.AddRow(row3);

            // --- ACT
            // delete row 2
            page.DeleteRow(row2Id);

            var deletedRow = page.GetRow(row2Id) as LocalRow;

            // --- ASSERT
            Assert.True(deletedRow.IsLogicallyDeleted);
        }

        /// <summary>
        /// Tests a <seealso cref="UserDataPage100"/> to see if when adding a <seealso cref="IRow"/> of the same byte length will update the row in place (and saves the correct information.)
        /// 
        /// Assert:
        /// The value that was updated should be persisted when reading the row back from the table
        /// </summary>
        /// <remarks>For more information on how Pages work, see Page.md</remarks>
        [Fact]
        public void Test_Update_Row_Same_Length()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);
            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);

            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);

            page.AddRow(row2);

            uint rowSize = row.Size();
            uint row2Size = row2.Size();

            uint totalBytes = page.TotalBytesUsed();

            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);

            page.AddRow(row3);

            var nameFlip = "ydnaR";

            row.SetValue("Name", nameFlip);

            // --- ACT
            page.TryUpdateRowData(row, out _);
            var updatedRow = page.GetRow(rowId) as LocalRow;

            // --- ASSERT
            Assert.Equal(nameFlip, updatedRow.GetValueInString("Name"));
        }


        /// <summary>
        /// Tests updating a <see cref="IRow"/> with different variable length values will forward the row correctly via the <seealso cref="UserDataPage100.TryUpdateRowData(IRow, out int)"/> method. 
        /// It adds a row, then updates it, then retrieves it to check if the values are the updated ones.
        /// </summary>
        /// <remarks>For more information on how Pages work, see Page.md</remarks>
        [Fact]
        public void Test_Update_Row_Different_Length()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "35");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);
            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);

            page.AddRow(row2);

            uint rowSize = row.Size();
            uint row2Size = row2.Size();

            uint totalBytes = page.TotalBytesUsed();

            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);

            page.AddRow(row3);

            var newName = "ABCDEFGHIJ";

            // --- ACT
            row.SetValue("Name", newName);
            page.TryUpdateRowData(row, out _);
            var updatedRow = page.GetRow(rowId) as LocalRow;

            // --- ASSERT
            Assert.Equal(newName, updatedRow.GetValueInString("Name"));
        }



        /// <summary>
        /// Tests updating multiple rows with multiple variable length values and ensures they are saved correctly.
        /// </summary>
        /// <remarks>For more information on how Pages work, see Page.md</remarks>
        [Fact]
        public void Test_Update_Row_Multiple_Variable_Length()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            LocalRow row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "-1");
            row.SetValue("NickName", "Randster");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);
            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);

            page.AddRow(row2);

            uint totalBytes = page.TotalBytesUsed();

            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);

            page.AddRow(row3);
            //---------------------------------

            var newName = "ABCDEFGHIJ";

            // --- ACT
            row.SetValue("Name", newName); // from Randy to ABCDEFGHIJ
            // forward 1
            page.TryUpdateRowData(row, out _);

            row = page.GetRow(rowId) as LocalRow;

            var newName2 = "12345678901234567890";

            // --- ACT
            row.SetValue("Name", newName2); // from ABCDEFGHIJ to 12345678901234567890
            // forward 2
            page.TryUpdateRowData(row, out _);

            row = page.GetRow(rowId) as LocalRow;

            // --- ACT
            var newName3 = "foobazbar";
            row.SetValue("Name", newName3); // from 12345678901234567890 to ABCDEFGHIJ
            // forward 3
            page.TryUpdateRowData(row, out _);

            var updatedRow = page.GetRow(rowId) as LocalRow;

            // --- ASSERT
            Assert.Equal(newName3, updatedRow.GetValueInString("Name"));
        }


        /// <summary>
        /// Tests the <seealso cref="IRow.SortBinaryOrder"/> function to ensure that data is ordered by binary format, not ordinal format.
        /// </summary>
        [Fact]
        public void Test_Test_Row_Sort()
        {
            /*
            * ASSEMBLIES:
            * Drummersoft.DrummerDB.Core.Tests.Mocks
            * 
            * Drummersoft.DrummerDB.Core.Structures
            * Drummersoft.DrummerDB.Core.Databases
            */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            var sqlIntAge = new SQLInt();
            var columnNameAge = new ColumnSchema("Age", sqlIntAge, 3);

            var nicName = new ColumnSchema("NickName", nvarchar, 4);

            columns.Add(columnId);
            columns.Add(columnName);
            columns.Add(columnNameAge);
            columns.Add(nicName);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();

            // --- ACT
            row.SortBinaryOrder();

            // --- ASSERT
            Assert.True(row.Values.First().Column.IsFixedBinaryLength);
        }


        /// <summary>
        /// Tests a Nullable NVarchar column and sets the value to NULL, and tests a Nullable INT column but sets the value. Validates after initial save of row and multiple updates to the row that the values are both NULL and NOT NULL.
        /// </summary>
        [Fact]
        public void Test_Row_Null_Column_Varchar_Int()
        {
            /*
           * ASSEMBLIES:
           * Drummersoft.DrummerDB.Core.Tests.Mocks
           * 
           * Drummersoft.DrummerDB.Core.Structures
           * Drummersoft.DrummerDB.Core.Databases
           */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1); ;

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

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            row.SetValue("Name", "Randy");
            row.SetValue("Id", "999");
            row.SetValue("Age", "-1");
            row.SetValueAsNullForColumn("NickName");
            row.SetValue("Rank", "741");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);
            page.AddRow(row);

            var row2 = table.GetNewLocalRow();
            uint row2Id = row2.Id;
            row2.SortBinaryOrder();

            string row2NickName = "Way";

            row2.SetValue("Name", "Megan");
            row2.SetValue("Id", "888");
            row2.SetValue("Age", "36");
            row2.SetValue("NickName", row2NickName);
            row2.SetValueAsNullForColumn("Rank");

            page.AddRow(row2);

            uint totalBytes = page.TotalBytesUsed();

            var row3 = table.GetNewLocalRow();
            uint row3Id = row3.Id;
            row3.SortBinaryOrder();

            string row3NickName = "Dad";

            row3.SetValue("Name", "Charles");
            row3.SetValue("Id", "777");
            row3.SetValue("Age", "75");
            row3.SetValue("NickName", row3NickName);
            row3.SetValueAsNullForColumn("Rank");

            page.AddRow(row3);
            //---------------------------------

            var newName = "ABCDEFGHIJ";

            // --- ACT
            row.SetValue("Name", newName); // from Randy to ABCDEFGHIJ
            // forward 1
            page.TryUpdateRowData(row, out _);

            row = page.GetRow(rowId) as LocalRow;

            var newName2 = "12345678901234567890";

            // --- ACT
            row.SetValue("Name", newName2); // from ABCDEFGHIJ to 12345678901234567890
            // forward 2
            page.TryUpdateRowData(row, out _);

            row = page.GetRow(rowId) as LocalRow;

            // --- ACT
            var newName3 = "foobazbar";
            row.SetValue("Name", newName3); // from 12345678901234567890 to ABCDEFGHIJ
            // forward 3
            page.TryUpdateRowData(row, out _);

            var updatedRow = page.GetRow(rowId) as LocalRow;

            // --- ACT
            var returnedRow = page.GetRow(rowId) as LocalRow;
            bool nickNamevalueIsNull = returnedRow.IsValueNull("NickName");
            bool rankValueIsNotNull = returnedRow.IsValueNull("Rank");

            // --- ASSERT
            Assert.Equal(newName3, updatedRow.GetValueInString("Name"));
            Assert.True(nickNamevalueIsNull);
            Assert.False(rankValueIsNotNull);
        }

        /// <summary>
        /// Same test as <seealso cref="Test_Row_Null_Column_Varchar_Int"/> but validates that the Nullable INT column still has it's value.
        /// </summary>
        [Fact]
        public void Test_Row_Null_Column_Varchar_Int_Value()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 1);

            var nicName = new ColumnSchema("NickName", nvarchar, 2, true);

            var sqlIntRank = new SQLInt();
            var columnRank = new ColumnSchema("Rank", sqlIntRank, 3, true);

            columns.Add(columnName);
            columns.Add(nicName);
            columns.Add(columnRank);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            uint rowId = row.Id;
            row.SortBinaryOrder();

            string rank = "1";

            row.SetValue("Name", "Randy");
            row.SetValueAsNullForColumn("NickName");
            row.SetValue("Rank", rank);

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);
            page.AddRow(row);

            var newName = "ABCDEFGHIJ";

            // --- ACT
            row.SetValue("Name", newName); // from Randy to ABCDEFGHIJ
            // forward 1
            page.TryUpdateRowData(row, out _);

            row = page.GetRow(rowId) as LocalRow;

            var newName2 = "12345678901234567890";

            // --- ACT
            row.SetValue("Name", newName2); // from ABCDEFGHIJ to 12345678901234567890
            // forward 2
            page.TryUpdateRowData(row, out _);

            row = page.GetRow(rowId) as LocalRow;

            // --- ACT
            var newName3 = "foobazbar";
            row.SetValue("Name", newName3); // from 12345678901234567890 to ABCDEFGHIJ
            // forward 3
            page.TryUpdateRowData(row, out _);

            var updatedRow = page.GetRow(rowId) as LocalRow;

            // --- ACT
            var returnedRow = page.GetRow(rowId) as LocalRow;
            bool nickNameValueIsNull = returnedRow.IsValueNull("NickName");
            bool rankValueIsNotNull = returnedRow.IsValueNull("Rank");
            var rankValue = returnedRow.GetValueInString("Rank");

            // --- ASSERT
            Assert.Equal(newName3, updatedRow.GetValueInString("Name"));
            Assert.True(nickNameValueIsNull);
            Assert.False(rankValueIsNotNull);
            Assert.Equal(rank, rankValue);
        }

        /// <summary>
        /// Tests setting NULL on all different type of data type columns and checks to ensure that values are as expected.
        /// </summary>
        [Fact]
        public void Test_Row_Null_Column_All_DataTypes()
        {
            /*
             * ASSEMBLIES:
             * Drummersoft.DrummerDB.Core.Tests.Mocks
             * 
             * Drummersoft.DrummerDB.Core.Structures
             * Drummersoft.DrummerDB.Core.Databases
             */

            // --- ARRANGE
            uint tableId = 1;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 1, true);

            var charColumnType = new SQLChar(1);
            var charColumn = new ColumnSchema("Sex", charColumnType, 2, true);

            var sqlIntRank = new SQLInt();
            var columnRank = new ColumnSchema("Rank", sqlIntRank, 3, true);

            var charBoolType = new SQLBit();
            var columnIsEmployed = new ColumnSchema("IsEmployed", charBoolType, 4, true);

            var dtType = new SQLDateTime();
            var columnBirthday = new ColumnSchema("Birthday", dtType, 5, true);

            var decimalType = new SQLDecimal();
            var columnSalary = new ColumnSchema("Salary", decimalType, 6, true);

            columns.Add(columnName);
            columns.Add(charColumn);
            columns.Add(columnRank);
            columns.Add(columnIsEmployed);
            columns.Add(columnBirthday);
            columns.Add(columnSalary);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var mockCache = new MockCacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, mockCache, mockRemoteDataManager, mockStorage, xManager);

            var rowAllNull = table.GetNewLocalRow();
            uint rowIdAllNull = rowAllNull.Id;
            rowAllNull.SortBinaryOrder();

            rowAllNull.SetValueAsNullForColumn("Name");
            rowAllNull.SetValueAsNullForColumn("Sex");
            rowAllNull.SetValueAsNullForColumn("Rank");
            rowAllNull.SetValueAsNullForColumn("IsEmployed");
            rowAllNull.SetValueAsNullForColumn("Birthday");
            rowAllNull.SetValueAsNullForColumn("Salary");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);
            uint rowAllNullOffset = page.AddRow(rowAllNull);

            var rowNotNull = table.GetNewLocalRow();
            uint rowIdNotNull = rowNotNull.Id;
            rowNotNull.SortBinaryOrder();

            /*
             Used in troubleshooting byte array layout. For more information, see Row.md -
             
             Values as stored in RowValue.cs's _value field:
             Name: Randy - "52-61-6E-64-79" / "00-52-61-6E-64-79" (with isNull flag)
             Sex: M - "4D" / "00-4D" (with isNull flag)
             Rank: 0 - "00-00-00-00" / "00-00-00-00-00" (with isNull flag)
             IsEmployed: true - "01" / "00-01"
             Birthday: 01-01-1990 - "00-40-4D-3B-41-EC-B5-08" / "00-00-40-4D-3B-41-EC-B5-08"
             Salary: 10.00 - "00-00-00-00-00-00-24-40" / "00-00-00-00-00-00-00-24-40"

             Name (Saved) As: IsNull + Length (5, "Randy") + Data:
             "00-05-00-00-00-52-61-6E-64-79"
             
             Data: 41 bytes
             Total Length: (Preamble + Int + Data Length) 60 bytes

             Value as retrieved from the Page:
             "00-00-00-00-00-00-01-00-00-40-4D-3B-41-EC-B5-08-00-00-00-00-00-00-00-24-40-00-05-00-00-00-52-61-6E-64-79-00-01-00-00-00-4D"
             [00-00-00-00-00] (5) [00-01] (7) [00-00-40-4D-3B-41-EC-B5-08] (16) [00-00-00-00-00-00-00-24-40] (25) [00-05-00-00-00-52-61-6E-64-79] (35) [00-01-00-00-00-4D] (41)"

             Rank
             IsEmployed
             Birthday
             Salary
             Name
             Sex
             */

            rowNotNull.SetValue("Name", "Randy");
            rowNotNull.SetValue("Sex", "M");
            rowNotNull.SetValue("Rank", "0");
            rowNotNull.SetValue("IsEmployed", "true");
            rowNotNull.SetValue("Birthday", "01-01-1990");
            rowNotNull.SetValue("Salary", "10.00");

            uint rowNotNullOffset = page.AddRow(rowNotNull);

            var rowMixNull = table.GetNewLocalRow();
            uint rowIdMixNull = rowMixNull.Id;
            rowMixNull.SortBinaryOrder();

            rowMixNull.SetValue("Name", "John");
            rowMixNull.SetValueAsNullForColumn("Sex");
            rowMixNull.SetValueAsNullForColumn("Rank");
            rowMixNull.SetValueAsNullForColumn("Birthday");
            rowMixNull.SetValue("IsEmployed", "false");
            rowMixNull.SetValueAsNullForColumn("Salary");

            uint rowMixNullOffset = page.AddRow(rowMixNull);

            // --- ACT
            var returnedNullRow = page.GetRow(rowIdAllNull) as LocalRow;
            var returnedNotNullRow = page.GetRow(rowIdNotNull) as LocalRow;
            var returnedMixRow = page.GetRow(rowIdMixNull) as LocalRow;

            // --- ASSERT
            Assert.Equal("Randy", returnedNotNullRow.GetValueInString("Name"));
            Assert.Equal("M", returnedNotNullRow.GetValueInString("Sex"));
            Assert.Equal("0", returnedNotNullRow.GetValueInString("Rank"));
            Assert.Equal("True", returnedNotNullRow.GetValueInString("IsEmployed"));

            var expectedDate = DateTime.Parse("1/1/1990");
            var returnedDate = DateTime.Parse(returnedNotNullRow.GetValueInString("Birthday"));

            Assert.Equal(expectedDate, returnedDate);
            Assert.Equal("10", returnedNotNullRow.GetValueInString("Salary"));

            Assert.True(returnedNullRow.IsValueNull("Name"));
            Assert.True(returnedNullRow.IsValueNull("Sex"));
            Assert.True(returnedNullRow.IsValueNull("Rank"));
            Assert.True(returnedNullRow.IsValueNull("IsEmployed"));
            Assert.True(returnedNullRow.IsValueNull("Birthday"));
            Assert.True(returnedNullRow.IsValueNull("Salary"));

            Assert.False(returnedMixRow.IsValueNull("Name"));
            Assert.Equal("John", returnedMixRow.GetValueInString("Name"));
            Assert.True(returnedMixRow.IsValueNull("Sex"));
            Assert.True(returnedMixRow.IsValueNull("Rank"));
            Assert.True(returnedMixRow.IsValueNull("Birthday"));
            Assert.True(returnedMixRow.IsValueNull("Salary"));
            Assert.False(returnedMixRow.IsValueNull("IsEmployed"));
            Assert.Equal("False", returnedMixRow.GetValueInString("IsEmployed"));

        }

        [Fact]
        public void Test_Add_Row_Get_Value_At_Address()
        {
            uint tableId = 990;
            Guid dbId = Guid.NewGuid();
            string tableName = "Test";

            var columns = new List<ColumnSchema>();

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 1, true);

            var charColumnType = new SQLChar(1);
            var charColumn = new ColumnSchema("Sex", charColumnType, 2, true);

            var sqlIntRank = new SQLInt();
            var columnRank = new ColumnSchema("Rank", sqlIntRank, 3, true);

            columns.Add(columnName);
            columns.Add(charColumn);
            columns.Add(columnRank);

            var tableSchema = new TableSchema(tableId, tableName, dbId, columns, string.Empty);

            var cache = new CacheManager();
            var mockRemoteDataManager = new MockRemoteDataManager();
            var mockStorage = new MockStorageManager();
            var xManager = new TransactionEntryManager();

            var table = new Table(tableSchema, cache, mockRemoteDataManager, mockStorage, xManager);

            var row = table.GetNewLocalRow();
            row.SetValue("Name", "Randy");
            row.SetValue("Sex", "M");
            row.SetValue("Rank", "7");

            var address = new PageAddress(table.Address.DatabaseId, table.Address.TableId, 1, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var page = cache.UserDataGetPage(address);

            // --- ACT
            page.AddRow(row);

            var rowAddresses = page.GetRowIdsOnPage();
            RowAddress rowAddress;

            foreach (var add in rowAddresses)
            {
                if (add.RowId == row.Id)
                {
                    rowAddress = add;
                }
            }

            // no longer applicable as the table loads the cache up if needed
            //cache.UserDataAddIntitalData(page, table.Address);

            var returnedRow = page.GetRow(row.Id);

            var columnSchema = table.GetColumn("Name");

            ValueAddress nameValueAddress;

            var values = cache.GetValues(table.Address, "Name", table.Schema());

            nameValueAddress = values.First();

            var valueName = page.GetValueAtAddress(nameValueAddress, columnSchema);
            var returnedValue = valueName.GetValueInString();


            Assert.Equal("Randy", returnedValue);
        }
    }
}
