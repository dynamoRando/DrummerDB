using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class DeleteStatement : IStatement, IWhereClause, IColumnList, IDMLStatement,

        // antlr handles
        IContextFullTableName
    {
        private WhereClause _whereClause;
        public bool IsValidated { get; set; }
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public StatementType Type => StatementType.DML;
        public string FullText { get; set; }

        public WhereClause? WhereClause
        {
            get
            {
                if (_whereClause is null)
                {
                    return null;
                }
                else
                {
                    return _whereClause;
                }
            }

            set
            {
                _whereClause = value;
            }
        }
        public bool HasWhereClause => WhereClause is null ? false : true;
        public List<StatementColumn> Columns { get; set; }

        public DeleteStatement()
        {
            Columns = new List<StatementColumn>();
            TableName = string.Empty;
        }

        public int GetMaxWhereClauseId()
        {
            if (HasWhereClause)
            {
                return WhereClause!.GetMaxWhereClauseId();
            }

            return 0;
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

        #region Antlr Handles
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
