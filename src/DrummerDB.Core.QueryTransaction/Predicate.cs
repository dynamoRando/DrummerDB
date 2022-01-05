using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures.Enum;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Represents part of a WHERE clause, i.e if a WHERE clause has FOO = A OR BAR = 2, this would be one of the parts (FOO = A)
    /// </summary>
    class Predicate : IPredicate
    {
        private uint _id;
        private Interval _interval;

        public string FullText { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public int EvaluationLevel { get; set; }
        public uint Id => _id;
        public Interval Interval => _interval;

        public Predicate(uint id)
        {
            if (string.IsNullOrEmpty(FullText))
            {
                FullText = string.Empty;
            }

            TableName = string.Empty;
            ColumnName = string.Empty;
            Operator = string.Empty;
            Value = string.Empty;
            _id = id;
        }

        public Predicate(uint id, string fulltext, string tableName) : this(id)
        {
            FullText = fulltext;
            TableName = tableName;

            ParseText();
        }

        public void SetInterval(Interval interval)
        {
            _interval = interval;
        }

        public ValueComparisonOperator GetValueOperator()
        {
            if (Operator == ">")
            {
                return ValueComparisonOperator.GreaterThan;
            }

            if (Operator == "<")
            {
                return ValueComparisonOperator.LessThan;
            }

            if (Operator == ">=")
            {
                return ValueComparisonOperator.GreaterThanOrEqualTo;
            }

            if (Operator == "<")
            {
                return ValueComparisonOperator.LessThanOrEqualTo;
            }

            if (Operator == "=")
            {
                return ValueComparisonOperator.Equals;
            }

            return ValueComparisonOperator.Unknown;
        }

        private void ParseText()
        {
            var parts = FullText.Split(' ');

            if (parts.Length == 3)
            {
                ColumnName = parts[0];
                Operator = parts[1];
                Value = parts[2].Replace("'", "");
            }
        }
    }
}
