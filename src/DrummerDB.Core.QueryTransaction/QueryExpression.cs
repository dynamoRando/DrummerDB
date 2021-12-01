using Drummersoft.DrummerDB.Common;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Represents an expression defined in a query, like a Scalar from a Group By, etc.
    /// </summary>
    internal class QueryExpression
    {
        public int Id { get; set; }
        public byte[]? Value { get; set; }
        public SQLColumnType ColumnType { get; set; }
        public bool IsNullable { get; set; }

        public QueryExpression()
        {
            Id = 0;
            Value = new byte[1];
            ColumnType = SQLColumnType.Unknown;
            IsNullable = false;
        }
    }
}
