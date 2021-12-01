using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    class InsertStatement : IStatement, IColumnList, IDMLStatement,

        // antlr handles
        IContextFullTableName
    {
        public StatementType Type => StatementType.DML;
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public bool IsValidated { get; set; }
        public List<StatementColumn> Columns { get; set; }
        public List<InsertRow> Rows { get; set; }
        public string FullText { get; set; }
        public InsertRow PendingRow { get; set; }

        public InsertStatement()
        {
            Columns = new List<StatementColumn>();
            TableName = string.Empty;
            Rows = new List<InsertRow>();
            FullText = string.Empty;
        }

        public int GetMaxColumnId()
        {
            int max = 0;
            foreach (var column in Columns)
            {
                if (column.ColumnIndex > max)
                {
                    max = column.ColumnIndex;
                }
            }
            return max;
        }

        public StatementColumn GetColumn(int id)
        {
            foreach (var column in Columns)
            {
                if (column.ColumnIndex == id)
                {
                    return column;
                }
            }
            return null;
        }

        public int GetMaxRowId()
        {
            int max = 0;
            foreach (var row in Rows)
            {
                if (row.Id > max)
                {
                    max = row.Id;
                }
            }
            return max;
        }

        public InsertRow GetRow(int rowId)
        {
            foreach (var row in Rows)
            {
                if (row.Id == rowId)
                {
                    return row;
                }
            }
            return null;
        }

        #region Antlr Handlers
        public void HandleEnterFullTableName(ContextWrapper context)
        {
            TableName = context.FullText.Trim();
        }

        public bool TryValidateColumnList(ContextWrapper context, IDatabase database, out List<string> errors)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
