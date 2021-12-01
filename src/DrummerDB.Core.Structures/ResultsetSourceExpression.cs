using Drummersoft.DrummerDB.Core.Structures.Interface;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents a computed expression in a resultset
    /// </summary>
    /// <remarks>You use this resultset source object when identifying the source as the result of an aggregate function (SUM, AVG), etc.</remarks>
    internal class ResultsetSourceExpression : IResultsetSource
    {
        /// <summary>
        /// The id of an expression computed elsewhere in the query plan
        /// </summary>
        public int ExpressionId { get; set; }
        public int Order { get; set; }
    }
}
