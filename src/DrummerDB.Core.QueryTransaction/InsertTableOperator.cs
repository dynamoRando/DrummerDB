using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class InsertTableOperator : ISQLNonQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        private List<Row> _tryRows;
        #endregion

        #region Public Properties
        public int Order { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string TableSchemaName { get; set; }
        public List<InsertRow> Rows { get; set; }
        public List<StatementColumn> Columns { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        #endregion

        #region Constructors
        public InsertTableOperator(IDbManager db)
        {
            _db = db;
            Rows = new List<InsertRow>();
            _tryRows = new List<Row>();
            Columns = new List<StatementColumn>();
        }
        #endregion

        #region Public Methods
        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            bool rowsAdded = true;

            var db = _db.GetDatabase(DatabaseName);


            if (string.IsNullOrEmpty(TableSchemaName))
            {
                if (db.HasTable(TableName))
                {
                    var table = db.GetTable(TableName);

                    if (transactionMode == TransactionMode.Try || transactionMode == TransactionMode.None)
                    {
                        foreach (var insertRow in Rows)
                        {
                            var row = table.GetNewLocalRow();
                            foreach (var insertValue in insertRow.Values)
                            {
                                if (insertValue.IsNull)
                                {
                                    row.SetValueAsNullForColumn(insertValue.ColumnName);
                                }
                                else
                                {
                                    row.SetValue(insertValue.ColumnName, insertValue.Value);
                                }

                            }

                            if (!table.XactAddRow(row, transaction, transactionMode))
                            {
                                rowsAdded = false;
                            }
                            else
                            {
                                _tryRows.Add(row);
                            }
                        }
                    }
                    else if (transactionMode == TransactionMode.Commit && _tryRows.Count > 0)
                    {
                        foreach (var row in _tryRows)
                        {
                            if (!table.XactAddRow(row, transaction, transactionMode))
                            {
                                rowsAdded = false;
                            }
                        }
                    }

                    if (rowsAdded)
                    {
                        messages.Add($"{Rows.Count.ToString()} rows were added to table {TableName}");
                    }
                    else
                    {
                        errorMessages.Add("Rows were not added");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid Query Plan: Database {DatabaseName} does not have table {TableName}");
                }
            }
            else
            {
                if (db.HasTable(TableName, TableSchemaName))
                {
                    var table = db.GetTable(TableName);

                    if (transactionMode == TransactionMode.Try || transactionMode == TransactionMode.None)
                    {
                        foreach (var insertRow in Rows)
                        {
                            var row = table.GetNewLocalRow();
                            foreach (var insertValue in insertRow.Values)
                            {
                                if (insertValue.IsNull)
                                {
                                    row.SetValueAsNullForColumn(insertValue.ColumnName);
                                }
                                else
                                {
                                    row.SetValue(insertValue.ColumnName, insertValue.Value);
                                }
                            }

                            if (!table.XactAddRow(row, transaction, transactionMode))
                            {
                                rowsAdded = false;
                            }
                            else
                            {
                                _tryRows.Add(row);
                            }
                        }
                    }
                    else if (transactionMode == TransactionMode.Commit && _tryRows.Count > 0)
                    {
                        foreach (var row in _tryRows)
                        {
                            if (!table.XactAddRow(row, transaction, transactionMode))
                            {
                                rowsAdded = false;
                            }
                        }
                    }

                    if (rowsAdded)
                    {
                        messages.Add($"{Rows.Count.ToString()} rows were added to table {TableName}");
                    }
                    else
                    {
                        errorMessages.Add("Rows were not added");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Invalid Query Plan: Database {DatabaseName} does not have table {TableSchemaName}.{TableName}");
                }
            }
        }


        #endregion

        #region Private Methods
        #endregion


    }
}
