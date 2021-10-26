using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
