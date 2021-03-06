using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class DeleteOperator : ISQLNonQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        private bool _isPreviousRemoteDeleteSuccess;
        #endregion

        #region Public Properties
        public readonly TreeAddress Address;
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string DatabaseName { get; set; }
        #endregion

        #region Constructors
        public DeleteOperator(IDbManager db, TreeAddress address)
        {
            _db = db;
            Address = address;
        }
        #endregion

        #region Public Methods
        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            Table table = _db.GetTable(Address);
            HostDb db = _db.GetHostDatabase(Address.DatabaseId);
            bool isSuccessful = false;
            string errorMessage = string.Empty;

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
                                    var row = table.GetRow(rowAddress);
                                    if (table.XactDeleteRow(row, transaction, transactionMode))
                                    {
                                        messages.Add("DELETE completed successfully");
                                    }
                                }
                                else
                                {
                                    if (rowAddress.HasDataLocally)
                                    {
                                        // need to handle deleting locally
                                        // then notifying the host of the deletion
                                        // and then letting the host decide what it wants to do with the deletion action

                                        var row = table.GetRow(rowAddress);
                                        if (table.XactDeleteRow(row, transaction, transactionMode))
                                        {
                                            messages.Add("DELETE locally completed successfully");

                                            var sysDb = _db.GetSystemDatabase();
                                            var shouldNotifyHost = sysDb.ShouldNotifyHostOfDataChanges(DatabaseName, table.Name);
                                            var hostId = table.GetRemotableRow(row.Id).RemoteId;
                                            var hostInfo = sysDb.GetCooperatingHost(hostId);

                                            if (shouldNotifyHost)
                                            {
                                                if ((transactionMode == TransactionMode.Commit || transactionMode == TransactionMode.None) && _isPreviousRemoteDeleteSuccess)
                                                {
                                                    messages.Add($"Host {hostInfo} has been notified of deletion");
                                                }
                                                else
                                                {
                                                    var partDb = _db.GetPartialDb(DatabaseName);
                                                    var hostIsNotified = partDb.NotifyHostOfRowDeletion(row.Id, table.Name, hostInfo, partDb.Id, table.Address.TableId);

                                                    if (hostIsNotified)
                                                    {
                                                        messages.Add($"Host {hostInfo} has been notified of deletion");

                                                        if (transactionMode == TransactionMode.Try || transactionMode == TransactionMode.None)
                                                        {
                                                            if (hostIsNotified)
                                                            {
                                                                _isPreviousRemoteDeleteSuccess = true;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (transactionMode == TransactionMode.Try || transactionMode == TransactionMode.None)
                                                        {
                                                            _isPreviousRemoteDeleteSuccess = false;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // need to delete the remote row first
                                        var participant = db.GetParticipant(rowAddress.RemotableId, true);
                                        isSuccessful = db.XactRequestParticipantRemoveRow
                                            (
                                            participant,
                                            table.Name,
                                            table.Address.TableId,
                                            db.Name,
                                            db.Id,
                                            rowAddress.RowId,
                                            transaction,
                                            transactionMode,
                                            out errorMessage
                                            );

                                        // if the remote row delete is successful, then delete the local reference
                                        if (isSuccessful)
                                        {
                                            var row = table.GetRow(rowAddress);
                                            if (table.XactDeleteRow(row, transaction, transactionMode))
                                            {
                                                messages.Add("DELETE completed successfully");
                                            }
                                        }

                                        if (isSuccessful)
                                        {
                                            messages.Add($"REMOTE DELETE completed successfully at {participant.Alias}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // delete all rows in the table
                // this still works because in StatementPlanEvaluator.cs in EvalutateQueryPlanForDelete() we set the target rows to be every row in the table
            }
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
