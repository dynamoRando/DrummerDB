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
    class SelectStatement :
        IStatement,
        IWhereClause,
        IColumnList,
        IDMLStatement,

        // antlr handlers
        IContextTableName,
        IContextTableAlias,
        IContextSelectListElement
    {
        private WhereClause _whereClause;
        private List<string> _statementColumns;
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
        public List<StatementColumn> Columns { get; set; }

        public SelectStatement()
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

        #region Antlr Wrappers

        public void HandleEnterTableNameOrCreateTable(ContextWrapper context)
        {
            if (context.Debug.Contains('.'))
            {
                var tableParts = context.Debug.Split('.');
                string tableAlias = tableParts[0].Trim();
                string tableName = tableParts[1].Trim();

                TableAlias = tableAlias;
                TableName = tableName;

            }
            else
            {
                TableName = context.Debug;
            }
        }

        public void HandleEnterTableAlias(ContextWrapper context)
        {
            TableAlias = context.FullText;
        }

        public void HandleEnterSelectListElement(ContextWrapper context)
        {
            string columnName = context.Debug;
            string tableAlias = string.Empty;

            // if the column has been aliased
            if (columnName.Contains('.'))
            {
                var colParts = columnName.Split('.');
                tableAlias = colParts[0].Trim();
                columnName = colParts[1].Trim();
            }

            StatementColumn col;
            var id = GetMaxColumnId() + 1;

            if (string.IsNullOrEmpty(tableAlias))
            {
                col = new StatementColumn(id, columnName);
            }
            else
            {
                col = new StatementColumn(id, columnName, tableAlias);
            }

            Columns.Add(col);
        }

        public void HandleExitSelectListElement(ContextWrapper context)
        {
            throw new NotImplementedException();
        }

        public bool TryValidateEnterTableNameOrCreateTable(ContextWrapper context, IDatabase database, out List<string> errors)
        {
            if (context.Debug.Contains('.'))
            {
                var tableParts = context.Debug.Split('.');
                string tableAlias = tableParts[0].Trim();
                string tableName = tableParts[1].Trim();

                TableAlias = tableAlias;
                TableName = tableName;

            }
            else
            {
                TableName = context.Debug;
            }

            IsValidated = database.HasTable(TableName);

            if (!IsValidated)
            {
                errors = new List<string>();
                errors.Add($"{TableName} does not exist in database {database.Name}");
            }
            else
            {
                errors = new List<string>();
            }

            return IsValidated;
        }

        public bool TryValidateColumnList(ContextWrapper context, IDatabase database, out List<string> errors)
        {
            throw new NotImplementedException();
        }

        public bool TryValidateSelectListElement(ContextWrapper context, IDatabase database, out List<string> errors)
        {
            var table = database.GetTable(TableName);
            string columnList = context.Debug;
            string[] columns = null;

            if (_statementColumns is null)
            {
                _statementColumns = new List<string>();
            }

            if (!string.Equals(columnList, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase))
            {
                columns = columnList.Split(",");
                _statementColumns.AddRange(columns);
            }
            else
            {
                _statementColumns.Add(columnList);
            }

            errors = new List<string>();

            if (table is not null)
            {
                if (!string.Equals(columnList, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase))
                {

                    foreach (var column in columns)
                    {
                        var tableColumns = table.Schema().Columns.Select(c => c.Name).ToList();
                        if (!tableColumns.Any(tc => string.Equals(tc, column, StringComparison.OrdinalIgnoreCase)))
                        {
                            errors.Add($"Column {column} does not exist in {TableName}");
                            IsValidated = false;
                            return IsValidated;
                        }
                    }
                }
            }
            else
            {
                // throw new InvalidOperationException($"{TableName} not found in {database.Name}");
                // table name not set, will need to validate after it is set
            }

            IsValidated = true;
            return IsValidated;
        }

        public bool TryValidateSelectListElement(IDatabase database, out List<string> errors)
        {
            errors = new List<string>();

            if (_statementColumns is not null)
            {
                if (_statementColumns.Count > 0)
                {
                    foreach (var column in _statementColumns)
                    {
                        if (!string.Equals(column, SQLGeneralKeywords.WILDCARD, StringComparison.OrdinalIgnoreCase))
                        {
                            var table = database.GetTable(TableName);
                            if (table is not null)
                            {
                                if (!table.HasColumn(column))
                                {
                                    errors.Add($"{column} not found in table {TableName}");
                                    IsValidated = false;
                                    return IsValidated;
                                }
                            }
                            else
                            {
                                throw new InvalidOperationException($"{TableName} not found in {database.Name}");
                            }
                        }
                    }

                    if (errors.Count == 0)
                    {
                        IsValidated = true;
                        return IsValidated;
                    }

                }
            }

            return false;
        }

        #endregion

    }
}
