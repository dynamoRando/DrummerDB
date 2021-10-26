using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IContextColumnDefinition
    {
        void HandleEnterColumnDefinition(ContextWrapper context);
        void HandleExitColumnDefinition(ContextWrapper context);   
    }
}
