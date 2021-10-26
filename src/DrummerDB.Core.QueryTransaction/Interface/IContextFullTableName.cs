using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IContextFullTableName
    {
        void HandleEnterFullTableName(ContextWrapper context);
    }
}
