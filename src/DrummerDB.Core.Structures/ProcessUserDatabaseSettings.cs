using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Settings class for determining database specific settings (Log Select Statements, etc.)
    /// </summary>
    internal class ProcessUserDatabaseSettings
    {
        public bool LogSelectStatementsForHost { get; set; }
    }
}
