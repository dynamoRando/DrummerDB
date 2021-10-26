using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.Unit.Structures
{
    public class Test_TableSchema
    {
        /// <summary>
        /// Tests the create table schema.
        /// </summary>
        [Fact]
        public void Test_Create_Table_Schema()
        {
            var i = new SQLInt();
            var columnId = new ColumnSchema("Id", i, 1);
            var nvarchar = new SQLVarChar(20);
            var columnName = new ColumnSchema("Name", nvarchar, 2);
            var columns = new List<ColumnSchema>();
            columns.Add(columnId);
            columns.Add(columnName);

            var tableName = "Employees";

            var table = new TableSchema(1, tableName, Guid.NewGuid(), columns);

            Assert.Equal(table.Name, tableName);
        }
    }
}
