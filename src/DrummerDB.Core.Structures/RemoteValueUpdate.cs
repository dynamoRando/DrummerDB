using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents a row value to update. This is analogus to Drummersoft.DrummerDB.Core.QueryTransaction.UpdateTableValue
    /// </summary>
    internal record struct RemoteValueUpdate
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
    }
}
