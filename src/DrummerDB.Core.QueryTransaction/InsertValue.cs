using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class InsertValue
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public string TableName { get; set; }
        public int Id { get; set; }

        public InsertValue(int id, string columnName, string value)
        {
            Id = id;
            ColumnName = columnName;
            Value = value;
        }
    }
}
