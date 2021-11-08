namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// A statement that has the ability to specify a WHERE clause
    /// </summary>
    interface IWhereClause
    {
        WhereClause? WhereClause { get; set; }
        int GetMaxWhereClauseId();
        bool HasWhereClause { get; }
    }
}
