using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.Unit.Structures
{
    public class Test_UserDataPage
    {
        /// <summary>
        /// Creates a UserDataPage and assigns an address to it.
        /// 
        /// Tests that the address being set for a paged is saved properly.
        /// 
        /// Assert:
        /// Checks that the returned address matches the saved address.
        /// </summary>
        [Fact]
        public void Test_Create_User_Page()
        {
            // --- ARRANGE
            int tableId = 1;
            Guid dbId = Guid.NewGuid();
            int pageId = 999;

            var address = new PageAddress(dbId, tableId, pageId, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var columns = new List<ColumnSchema>();

            var sqlInt = new SQLInt();

            var columnId = new ColumnSchema("Id", sqlInt, 1);

            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            columns.Add(columnId);
            columns.Add(columnName);

            var tableSchema = new TableSchema(tableId, "Test", dbId, columns, string.Empty);

            // ---  ACT
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);


            // ---  ASSERT
            Assert.Equal(page.PageId(), pageId);
            Assert.Equal(page.DbId(), dbId);
            Assert.Equal(page.TableId(), tableId);
        }

        [Fact]
        public void Test_Delete_Page()
        {
            // --- ARRANGE
            int tableId = 777;
            Guid dbId = Guid.NewGuid();
            int pageId = 999;

            var address = new PageAddress(dbId, tableId, pageId, Guid.Parse(Constants.DBO_SCHEMA_GUID));
            var columns = new List<ColumnSchema>();
            var sqlInt = new SQLInt();
            var columnId = new ColumnSchema("Id", sqlInt, 1);
            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);

            columns.Add(columnId);
            columns.Add(columnName);

            var tableSchema = new TableSchema(tableId, "Test", dbId, columns, string.Empty);

            // ---  ACT
            var page = UserDataPageFactory.GetUserDataPage100(address, tableSchema);
            page.Delete();

            // --- ASSERT
            Assert.True(page.IsDeleted());
            
            // ---  ACT
            page.UnDelete();

            // --- ASSERT
            Assert.False(page.IsDeleted());
        }
    }
}
