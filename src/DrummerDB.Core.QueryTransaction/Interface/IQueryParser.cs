using Drummersoft.DrummerDB.Core.Databases.Interface;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// Validates the syntax of a SQL query, including if the objects exist
    /// </summary>
    internal interface IQueryParser
    {
        bool IsStatementValid(string statement, IDbManager dbManager, out string errorMessage);
        bool IsStatementValid(string statement, string dbName, IDbManager dbManager, out string errorMessage);
    }
}
