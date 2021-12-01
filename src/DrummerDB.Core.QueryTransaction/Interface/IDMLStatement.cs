namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// A statement that is a DML statement operates on specific tables (SELECT, INSERT, UPDATE, DELETE)
    /// </summary>
    interface IDMLStatement
    {
        public string TableName { get; set; }
        public string TableAlias { get; set; }
    }
}
