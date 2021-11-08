using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class UpdateStatement : IStatement, IWhereClause, IColumnList, IDMLStatement,

        // antlr handles
        IContextFullTableName
    {
        private WhereClause _whereClause;
        public string FullText { get; set; }
        public bool IsValidated { get; set; }
        public string TableName { get; set; }
        public string TableAlias { get; set; }
        public StatementType Type => StatementType.DML;
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
        /// <summary>
        /// The list of columns we will be setting values on, i.e. FOO = 'BAR', BAZ = 'BLAH' => FOO, BAZ
        /// </summary>
        public List<StatementColumn> Columns { get; set; }
        public List<IUpdateColumnSource> Values { get; set; }
        public IUpdateColumnSource CurrentValue { get; set; }
        public StatementColumn CurrentColumn { get; set; }

        public UpdateStatement()
        {
            Columns = new List<StatementColumn>();
            Values = new List<IUpdateColumnSource>();
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
