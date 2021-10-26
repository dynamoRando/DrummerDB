using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    interface IStatement
    {
        StatementType Type { get; }
        bool IsValidated { get; set; }
    }
}
