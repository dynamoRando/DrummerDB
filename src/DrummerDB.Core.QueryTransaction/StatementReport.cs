using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Used to report any errors found while evaluating a SQL statement
    /// </summary>
    internal struct StatementReport
    {
        public List<string> Errors;
        public bool IsValid { get; set; }
        public string OriginalStatement { get; set; }
    }
}
