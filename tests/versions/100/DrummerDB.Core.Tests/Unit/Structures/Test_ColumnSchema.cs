using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Xunit;

namespace Drummersoft.DrummerDB.Core.Tests.Unit.Structures
{
    public class Test_ColumnSchema
    {
        // moved to breakout

        /// <summary>
        /// Tests creating a new <seealso cref="ColumnSchema"/> with a <seealso cref="SQLBit"/> data type.
        /// </summary>
        [Fact]
        public void Test_ColumnSchema_Create()
        {
            var colName = "Name";
            var datatype = new SQLBit();
            var column = new ColumnSchema(colName, datatype, 1);

            Assert.Equal(column.Name, colName);
        }
    }
}
