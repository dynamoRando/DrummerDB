using Drummersoft.DrummerDB.Core.Databases.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IContextSelectListElement
    {
        void HandleEnterSelectListElement(ContextWrapper context);
        void HandleExitSelectListElement(ContextWrapper context);
        bool TryValidateSelectListElement(ContextWrapper context, IDatabase database, out List<string> errors);
        bool TryValidateSelectListElement(IDatabase database, out List<string> errors);
    }
}
