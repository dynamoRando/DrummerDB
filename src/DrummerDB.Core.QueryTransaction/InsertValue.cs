namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class InsertValue
    {
        private bool _isNull = false;
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public string TableName { get; set; }
        public int Id { get; set; }

        public bool IsNull => _isNull;

        /// <summary>
        /// Creates a new insert value with the specified values
        /// </summary>
        /// <param name="id">The Id (Ordinal) of the insert value</param>
        /// <param name="columnName">The name of the column this value will be inserted into</param>
        /// <param name="value">The value to insert</param>
        public InsertValue(int id, string columnName, string value)
        {
            Id = id;
            ColumnName = columnName;
            Value = value;
            _isNull = false;
        }

        /// <summary>
        /// Creates a new insert value with NULL as the value to set
        /// </summary>
        /// <param name="id">The Id (Ordinal) of the insert value</param>
        /// <param name="columnName">The column we will set NULL as the value for</param>
        public InsertValue(int id, string columnName)
        {
            Id = id;
            ColumnName = columnName;
            _isNull = true;
        }
    }
}
