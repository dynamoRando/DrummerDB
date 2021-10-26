using Drummersoft.DrummerDB.Core.Databases.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IContextTableName
    {
        void HandleEnterTableNameOrCreateTable(ContextWrapper context);
        bool TryValidateEnterTableNameOrCreateTable(ContextWrapper context, IDatabase database, out List<string> errors);
    }
}
