using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class InsertRow
    {
        public string TableName { get; set; }
        public List<InsertValue> Values { get; set; }
        public int Id { get; set; }

        public InsertRow(int id)
        {
            Values = new List<InsertValue>();
            Id = id;
        }

        /// <summary>
        /// Returns the row value with the specified columnname, or NULL
        /// </summary>
        /// <param name="columnName">The value with the column</param>
        /// <returns>The row value with the specified column name, or NULL</returns>
        public InsertValue GetColumn(string columnName)
        {
            foreach (var value in Values)
            {
                if (string.Equals(value.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }
            return null;
        }

        public int GetMaxValueId()
        {
            int max = 0;
            foreach (var value in Values)
            {
                if (value.Id > max)
                {
                    max = value.Id;
                }
            }
            return max;
        }
    }
}
