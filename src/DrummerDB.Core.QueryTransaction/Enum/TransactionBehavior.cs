namespace Drummersoft.DrummerDB.Core.QueryTransaction.Enum
{
    // note: this enum should be seperate from the mode that the query plan is executed in, i.e try, rollback, or commit. that still
    // will be done. 
    internal enum TransactionBehavior
    {
        Unknown,
        // the sql statement contained no begin/commit transaction; therefore the query executor will try the query plan and if there are no errors
        // immediately after will commit the transaction
        Normal,
        // the sql statement contained explicit transaction statements, which will drive how the query executor will execute the plan
        Explicit,
        // the sql statement contained an open transaction statement; we will have the query executor execute the plan openly until there is another
        // commit or rollback command executed from the same user and session id again to query executor (or the query timeout expires)
        Open
    }
}
