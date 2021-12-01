namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class StatementColumn
    {
        public string ColumnName { get; set; }
        public int ColumnIndex { get; set; }
        public string TableAlias { get; set; }
        public string TableName { get; set; }

        /// <summary>
        /// Sets the column info 
        /// </summary>
        /// <param name="id">The id (index) of the column</param>
        /// <param name="name">The name of the column</param>
        /// <remarks>Usually called from setting up an INSERT staement</remarks>
        public StatementColumn(int id, string name)
        {
            ColumnName = name;
            ColumnIndex = id;
        }

        public StatementColumn(int id, string name, string tableAlias) : this(id, name)
        {
            TableAlias = tableAlias;
        }

        /// <summary>
        /// Sets the column info
        /// </summary>
        /// <param name="id">The id (index) of the column</param>
        /// <remarks>Usually called from an UPDATE statement. 
        /// Do not forget to set the ColumnName. Due to the parse tree,
        /// the column name is not identified until elsewere in the tree.</remarks>
        public StatementColumn(int id)
        {
            ColumnIndex = id;
        }
    }
}
