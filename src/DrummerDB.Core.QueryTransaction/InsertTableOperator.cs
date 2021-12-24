using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System;
using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class InsertTableOperator : ISQLNonQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        private List<Row> _tryRows;
        private ICoopActionPlanOption[] _options;
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

            if (_options is null)
            {
                _options = new ICoopActionPlanOption[0];
            }
        }

        public InsertTableOperator(IDbManager db, ICoopActionPlanOption[] options) : this(db)
        {
            _options = options;
        }
        #endregion

        #region Public Methods
        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            bool rowsAdded = true;

            // default host type
            var db = _db.GetDatabase(DatabaseName, DatabaseType.Host);

            if (_options.Length > 0)
            {
                foreach (var option in _options)
                {
                    if (option is CoopActionOptionParticipant)
                    {
                        var participantOption = (CoopActionOptionParticipant)option;
                        ExecuteForRemoteInsert(participantOption, transaction, transactionMode, ref messages, ref errorMessages);
                        return;
                    }
                }
            }

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
        private void ExecuteForRemoteInsert(CoopActionOptionParticipant participantOption, TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            bool rowsAdded = true;
            HostDb db = null;
            Table table = null;

            if (transactionMode == TransactionMode.Try || transactionMode == TransactionMode.None)
            {
                Participant participant = new Participant();
                db = _db.GetHostDatabase(DatabaseName);

                if (db.HasParticipantAlias(participantOption.ParticipantAlias))
                {
                    participant = db.GetParticipant(participantOption.ParticipantAlias);
                }

                if (string.IsNullOrEmpty(TableSchemaName))
                {
                    if (db.HasTable(TableName))
                    {
                        table = db.GetTable(TableName);
                        foreach (var insertRow in Rows)
                        {
                            var row = table.GetNewRemoteRow(participant);
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
                }
            }
            else if (transactionMode == TransactionMode.Commit && _tryRows.Count > 0)
            {
                table = db.GetTable(TableName);
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
        #endregion


    }
}
