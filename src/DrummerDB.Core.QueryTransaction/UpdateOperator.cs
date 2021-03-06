using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System.Linq;
using System;
using Drummersoft.DrummerDB.Common;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    // updates a table from the specified sources
    // we need to intake sources and targets
    // a source may be another table (as in a JOIN statement in an UPDATE statement)
    // or a source may be a value that the user passed in, i.e. FOO = 'BAR'
    // and at target are the rows specified in a WHERE clause
    // from a TableReadOperator (which returns a List<ValueAddres>)
    // or a target may be the entire table
    internal class UpdateOperator : ISQLNonQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        private List<IUpdateColumnSource> _sources;
        private bool _alreadySuccessfullyUpdated = false;
        #endregion

        #region Public Properties
        public readonly TreeAddress Address;
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string DatabaseName { get; set; }
        #endregion

        #region Constructors
        public UpdateOperator(IDbManager db, TreeAddress address, List<IUpdateColumnSource> sources)
        {
            _db = db;
            Address = address;
            _sources = sources;
        }
        #endregion

        #region Public Methods
        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            Table table = _db.GetTable(Address);

            bool rowsUpdated = true;
            bool hostNotified = false;

            // if we have a WHERE clause that we need to specify
            if (PreviousOperation is not null)
            {
                if (PreviousOperation is TableReadOperator)
                {
                    var readOp = PreviousOperation as TableReadOperator;
                    var filter = readOp.Result;
                    var targets = filter.Rows();

                    int tableCount = targets.Item1.Count();

                    // if we only are updating 1 table
                    if (tableCount == 1)
                    {
                        // make sure the target table is the same one we're updating
                        var targetTable = targets.Item1.First();
                        if (targetTable == Address)
                        {
                            foreach (var rowAddress in targets.Item2)
                            {
                                if (rowAddress.RemotableId == Guid.Empty)
                                {
                                    // this is a local row update
                                    rowsUpdated = UpdateLocalRow(transaction, transactionMode, errorMessages, table, rowsUpdated, rowAddress);
                                }
                                else // this is a remote update (host row or partial row)
                                {
                                    // this is a partial database row update
                                    if (rowAddress.HasDataLocally)
                                    {
                                        // we need to update the local data, 
                                        // then notify the upstream host of a data hash change 
                                        // if we are configured to do so

                                        // first, update the row in the local database
                                        rowsUpdated = UpdateLocalRow(transaction, transactionMode, errorMessages, table, rowsUpdated, rowAddress);

                                        if (rowsUpdated)
                                        {
                                            byte[] rowDataHash = null;

                                            // next need to determine if we are configured to notify the host of upstream changes
                                            var sysDb = _db.GetSystemDatabase();
                                            var shouldNotifyHost = sysDb.ShouldNotifyHostOfDataChanges(DatabaseName, table.Name);
                                            if (shouldNotifyHost)
                                            {
                                                // need to write function to notify host of data hash change
                                                rowDataHash = table.GetDataHashFromRow(rowAddress.RowId);
                                                Guid hostId = table.GetRemotableRow(rowAddress.RowId).RemoteId;

                                                var db = _db.GetDatabase(DatabaseName, DatabaseType.Partial) as PartialDb;
                                                var hostInfo = sysDb.GetCooperatingHost(hostId);

                                                if (!_alreadySuccessfullyUpdated && (transactionMode == TransactionMode.Try || transactionMode == TransactionMode.None))
                                                {
                                                    hostNotified = db.NotifyHostOfRowDataHashChange(
                                                 rowAddress.RowId,
                                                 table.Name,
                                                 rowDataHash,
                                                 hostInfo,
                                                 db.Id,
                                                 table.Address.TableId);
                                                }

                                                if (hostNotified)
                                                {
                                                    if (transactionMode == TransactionMode.None || transactionMode == TransactionMode.Try)
                                                    {
                                                        _alreadySuccessfullyUpdated = true;
                                                        messages.Add($"{targets.Item2.Count.ToString()} rows updated in table {table.Name} " +
                                                       $"and host {hostInfo} notified of data change");
                                                    }
                                                }
                                                else
                                                {
                                                    if (!_alreadySuccessfullyUpdated)
                                                    {
                                                        errorMessages.Add("Rows were updated but unable to notify host of data change");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            errorMessages.Add($"Unable to update row");
                                        }
                                    }
                                    else
                                    {
                                        // this is a pure remote update
                                        HostDb hostDb = _db.GetHostDatabase(Address.DatabaseId);
                                        var participant = hostDb.GetParticipant(rowAddress.RemotableId, true);
                                        foreach (var source in _sources)
                                        {
                                            if (source is UpdateTableValue)
                                            {
                                                var updateValue = source as UpdateTableValue;
                                                var remoteUpdateValue = new RemoteValueUpdate();
                                                remoteUpdateValue.ColumnName = updateValue.Column.ColumnName;
                                                remoteUpdateValue.Value = updateValue.Value;
                                                string errorMessage = string.Empty;

                                                byte[] newDataHash = new byte[0];
                                                var existingRow = table.GetHostRow(rowAddress);

                                                var result =
                                                    hostDb.XactRequestParticipantUpdateRow
                                                    (
                                                        participant,
                                                        table.Name,
                                                        table.Address.TableId,
                                                        hostDb.Name,
                                                        hostDb.Id,
                                                        rowAddress.RowId,
                                                        remoteUpdateValue,
                                                        transaction,
                                                        transactionMode,
                                                        existingRow.DataHash,
                                                        out newDataHash,
                                                        out errorMessage
                                                    );

                                                if (result)
                                                {
                                                    table.UpdateDataHashForHostRow(existingRow, newDataHash);
                                                }

                                                rowsUpdated = result;
                                            }
                                        }
                                    }

                                }
                            }

                            if (rowsUpdated)
                            {
                                messages.Add($"{targets.Item2.Count.ToString()} rows updated in table {table.Name}");
                            }
                        }
                    }
                }
            }
            else
            {
                // update all rows in table
                // this still works because in StatementPlanEvaluator.cs in EvaluateQueryPlanForUpdate() we set the TableReadOperator to the entire table.
            }
        }

        private bool UpdateLocalRow(TransactionRequest transaction, TransactionMode transactionMode, List<string> errorMessages, Table table, bool rowsUpdated, RowAddress rowAddress)
        {
            var row = table.GetRow(rowAddress);
            foreach (var source in _sources)
            {
                if (source is UpdateTableValue)
                {
                    var updateValue = source as UpdateTableValue;

                    if (table.HasColumn(updateValue.Column.ColumnName))
                    {
                        if (row.IsValueGroup())
                        {
                            row.AsValueGroup().SetValue(updateValue.Column.ColumnName, updateValue.Value);
                            if (!table.XactUpdateRow(row, transaction, transactionMode))
                            {
                                rowsUpdated = false;
                            }
                        }
                    }
                    else
                    {
                        errorMessages.Add(
                            $"Tried to update column {updateValue.Column.ColumnName} which is not in table {table.Name}");
                    }
                }
            }

            return rowsUpdated;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
