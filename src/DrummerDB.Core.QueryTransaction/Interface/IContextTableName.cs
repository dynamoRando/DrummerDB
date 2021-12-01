using Drummersoft.DrummerDB.Core.Databases.Interface;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface IContextTableName
    {
        void HandleEnterTableNameOrCreateTable(ContextWrapper context);
        bool TryValidateEnterTableNameOrCreateTable(ContextWrapper context, IDatabase database, out List<string> errors);
    }
}
