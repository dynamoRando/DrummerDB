using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents a table/column in a resultset. 
    /// </summary>
    /// <remarks>This is used in identifying the origin of a SELECT table/column when building the  results of a query plan.</remarks>
    internal class ResultsetSourceTable : IResultsetSource
    {
        public TreeAddress Table { get; set; }
        public int ColumnId { get; set; }
        public int Order { get; set; }
    }
}
