using Drummersoft.DrummerDB.Core.Databases.Abstract;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class QueryExecutor : IQueryExecutor
    {
        #region Private Fields
        private IAuthenticationManager _auth;
        private IDbManager _db;
        private LockManager _lockManager;
        private ITransactionManager _transactionManager;
        private ActivePlanCollection _activePlans;
        private LogService _log;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public QueryExecutor(IAuthenticationManager auth, IDbManager db, ITransactionEntryManager entryManager)
        {
            _auth = auth;
            _db = db;
            _lockManager = new LockManager(_db);
            _transactionManager = new TransactionManager(entryManager);
            _activePlans = new ActivePlanCollection();
        }

        public QueryExecutor(IAuthenticationManager auth, IDbManager db, ITransactionEntryManager entryManager, LogService log)
        {
            _auth = auth;
            _db = db;
            _lockManager = new LockManager(_db);
            _transactionManager = new TransactionManager(entryManager);
            _activePlans = new ActivePlanCollection();

            _log = log;
        }
        #endregion

        #region Public Methods
        public bool CancelPlan(Guid planId)
        {
            if (_activePlans.Contains(planId))
            {
                var plan = _activePlans.GetActivePlan(planId);
                plan.Cancel();
                _activePlans.Remove(plan);

                return true;
            }

            return false;
        }

        public async Task<Resultset> ExecutePlanAsync(QueryPlan plan, string un, string pw, Guid userSessionId)
        {
            var result = new Resultset();
            var messages = new List<string>();

            /*
             * First, we need to examine plan and ensure that the user has rights to 
             * take the plan actions against all objects in the plan
             * 
             * Second, we need to request locks on all objects in the plan, if possible
             * 
             * Third, we need to get a transaction id to pass to the various db transaction logs
             * 
             * If we are able to get the locks on all objects in the plan, then we will execute the plan
             * and get resultset (or result)
             * 
             * Note: UserSessionId is used to determine behaviors for open transactions
             * 
             * 
             */

            // note: Batch Transactions and Transaction Ids should be the same for the most part
            // unless the query plan generates a specific BEGIN TRAN/COMMIT statement, in which case there are sub transactions 
            // within the query, which we would account for.
            Guid transactionBatchId = _transactionManager.GetPendingBatchTransactionId();

            // check permission for entire plan
            if (UserCanRunPlan(plan, un, pw, out messages))
            {
                List<SQLLock> existingLocks = null;
                if (_lockManager.TryLockObjects(plan.LockObjectRequests, un, transactionBatchId, transactionBatchId, out existingLocks))
                {
                    TransactionRequest transaction = _transactionManager.EnqueueBatchTransaction(transactionBatchId, un, plan.PlanId);

                    var cancelSource = new CancellationTokenSource();
                    var activePlan = new ActivePlan(plan, cancelSource);
                    _activePlans.Add(activePlan);
                    result = await ExecuteAsync(plan, transaction);
                    _activePlans.Remove(activePlan);

                    _lockManager.TryReleaseLocks(transactionBatchId);
                    _transactionManager.DequeueBatchTransaction(transactionBatchId);
                }
                else
                {
                    result.ExecutionErrors.AddRange(GetLockListErrors(existingLocks));
                }
            }
            else
            {
                result.AuthenticationErrors.AddRange(messages);
            }

            return result;
        }
        #endregion

        #region Private Methods
        private async Task<Resultset> ExecuteAsync(QueryPlan plan, TransactionRequest transaction)
        {
            if (_log is not null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var result = await Task.Factory.StartNew(() => ExecutePlan(plan, transaction));
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds, transaction.TransactionBatchId, plan.SqlStatement);
                return result;
            }
            else
            {
                return await Task.Factory.StartNew(() => ExecutePlan(plan, transaction));
            }
        }

        private Resultset ExecutePlan(QueryPlan plan, TransactionRequest transaction)
        {
            var resultBuilder = new ResultsetBuilder(DetermineResultsetShape(plan), _db, _log);
            var addresses = new List<ValueAddress>();
            var resultSet = new Resultset();

            plan.Parts.OrderBy(p => p.Order);

            switch (plan.TransactionPlan.Behavior)
            {
                case TransactionBehavior.Normal:
                    addresses.AddRange(ExecuteNormalTransaction(plan, transaction, ref resultSet));
                    break;
                default:
                    throw new InvalidOperationException("Unknown transaction behavior");
            }

            return resultBuilder.Build(addresses, transaction, ref resultSet);
        }

        private ResultsetLayout DetermineResultsetShape(QueryPlan plan)
        {
            bool hasSelectStatement = false;
            int minPlanPart = 1;

            foreach (var part in plan.Parts)
            {
                if (part is SelectQueryPlanPart)
                {
                    hasSelectStatement = true;
                    if (part.Order <= minPlanPart)
                    {
                        minPlanPart = part.Order;
                    }
                }
            }

            if (hasSelectStatement)
            {
                foreach (var part in plan.Parts)
                {
                    if (part is SelectQueryPlanPart && part.Order == minPlanPart)
                    {
                        var select = part as SelectQueryPlanPart;
                        return select.Layout;
                    }
                }
            }

            return null;
        }

        private List<ValueAddress> ExecuteNormalTransaction(QueryPlan plan, TransactionRequest transaction, ref Resultset resultSet)
        {
            var result = new List<ValueAddress>();
            var messages = new List<string>();
            var errors = new List<string>();

            // begin transaction
            foreach (var part in plan.Parts)
            {
                foreach (var operation in part.Operations)
                {
                    if (operation is ISQLQueryable)
                    {
                        var op = operation as ISQLQueryable;
                        // don't need to add the results here, just attempt it
                        op.Execute(transaction, TransactionMode.Try);

                        //result.AddRange(op.Execute(transaction, TransactionMode.Try));
                    }
                    else if (operation is ISQLNonQueryable)
                    {
                        var op = operation as ISQLNonQueryable;
                        op.Execute(transaction, TransactionMode.Try, ref messages, ref errors);
                    }
                }
            }

            // complete transaction
            foreach (var part in plan.Parts)
            {
                foreach (var operation in part.Operations)
                {
                    if (operation is ISQLQueryable)
                    {
                        var op = operation as ISQLQueryable;
                        var results = op.Execute(transaction, TransactionMode.Commit);
                        result.AddRange(results.Distinct());
                    }
                    else if (operation is ISQLNonQueryable)
                    {
                        var op = operation as ISQLNonQueryable;
                        op.Execute(transaction, TransactionMode.Commit, ref messages, ref errors);
                    }
                }
            }

            resultSet.ExecutionErrors.AddRange(errors.Distinct());
            resultSet.NonQueryMessages.AddRange(messages.Distinct());

            return result;
        }

        private List<string> GetLockListErrors(List<SQLLock> heldLocks)
        {
            throw new NotImplementedException();
        }

        private bool UserCanRunPlan(QueryPlan plan, string un, string pw, out List<string> messages)
        {
            string message = string.Empty;
            messages = null;
            bool userCanRunPlan = true;

            foreach (var part in plan.Parts)
            {
                if (!userCanRunPlan)
                {
                    break;
                }

                foreach (var operation in part.Operations)
                {
                    if (UserHasRightsInOperation(operation, un, pw, out message))
                    {
                        continue;
                    }
                    else
                    {
                        userCanRunPlan = false;

                        if (messages is null)
                        {
                            messages = new List<string>();
                        }

                        messages.Add(message);
                        break;
                    }
                }
            }

            return userCanRunPlan;
        }

        private bool UserHasRightsInOperation(IQueryPlanPartOperator operation, string un, string pw, out string message)
        {
            message = string.Empty;
            UserDatabase db;

            switch (operation)
            {
                case TableReadOperator a:
                    var readOperation = operation as TableReadOperator;

                    db = _db.GetUserDatabase(readOperation.Address.DatabaseId);

                    if (db is null)
                    {
                        // then we are executing against the system database, which we need to ensure the user has full admin rights
                        return _auth.UserHasSystemPermission(un, SystemPermission.FullAccess);
                    }

                    var table = _db.GetTable(readOperation.Address);

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Select, table.Schema().ObjectId))
                    {
                        return true;
                    }
                    else
                    {
                        message = $"User: {un} does not have SELECT permission to table {table.Name}";
                    }
                    break;
                case CreateHostDbOperator b:
                    if (_auth.UserHasSystemPermission(un, SystemPermission.CreateDatabase))
                    {
                        return true;
                    }
                    break;
                case DropHostDbOperator c:
                    if (_auth.UserHasSystemPermission(un, SystemPermission.DropDatabase))
                    {
                        return true;
                    }
                    break;
                case CreateTableOperator d:

                    var ctOp = operation as CreateTableOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(ctOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Create_Table, db.Id))
                    {
                        return true;
                    }

                    break;
                case InsertTableOperator e:
                    var itOp = operation as InsertTableOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(itOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Insert, db.Id))
                    {
                        return true;
                    }
                    break;
                case UpdateOperator f:
                    var upOp = operation as UpdateOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(upOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Update, db.Id))
                    {
                        return true;
                    }
                    break;
                case DeleteOperator g:
                    var delOp = operation as DeleteOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(delOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Delete, db.Id))
                    {
                        return true;
                    }
                    break;
                case CreateSchemaOperator h:

                    var csOp = operation as CreateSchemaOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(csOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Create_Schema, db.Id))
                    {
                        return true;
                    }

                    break;
                case LogicalStoragePolicyOperator i:

                    var lspOp = operation as LogicalStoragePolicyOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(lspOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Set_Logical_Storage_Policy, db.Id))
                    {
                        return true;
                    }

                    break;
                case ReviewLogicalStoragePolicyOperator j:

                    var rlspOp = operation as ReviewLogicalStoragePolicyOperator;

                    if (_auth.UserHasSystemPermission(un, SystemPermission.FullAccess))
                    {
                        return true;
                    }

                    db = _db.GetUserDatabase(rlspOp.DatabaseName);
                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.FullAccess, db.Id))
                    {
                        return true;
                    }

                    if (_auth.UserHasDbPermission(un, pw, db.Name, DbPermission.Review_Logical_Storage_Policy, db.Id))
                    {
                        return true;
                    }

                    break;
                default:
                    throw new InvalidOperationException("Unknown operator type");
            }

            return false;
        }
        #endregion

    }
}
