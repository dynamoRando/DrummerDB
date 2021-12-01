using Drummersoft.DrummerDB.Core.Databases.Interface;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    /// <summary>
    /// A statement that has a list of columns to affect (SELECT, INSERT, UPDATE)
    /// </summary>
    interface IColumnList
    {
        public List<StatementColumn> Columns { get; set; }
        public int GetMaxColumnId();
        public bool TryValidateColumnList(ContextWrapper context, IDatabase database, out List<string> errors);
    }
}
