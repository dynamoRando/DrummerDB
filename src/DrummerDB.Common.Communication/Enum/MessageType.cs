using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Common.Communication.Enum
{
    public enum MessageType
    {
        Unknown,
        InsertRow,
        SaveContract,
        AcceptContract,
        GetRow,
        UpdateRow
    }
}
