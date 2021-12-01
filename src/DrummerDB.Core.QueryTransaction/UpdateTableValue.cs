using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Represents a constant value in an UPDATE statement,
    /// i.e. this holds the value the column will be updated with,
    /// such as NAME = 'FOO, where NAME is the StatementColumn object
    /// and VALUE is the string representation of the value we will update it with
    /// </summary>
    internal class UpdateTableValue : IUpdateColumnSource
    {
        public StatementColumn Column { get; set; }
        public string Value { get; set; }
    }
}
