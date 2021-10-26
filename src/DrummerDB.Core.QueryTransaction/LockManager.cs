using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class LockManager
    {
        #region Private Fields
        private IDbManager _db;
        private SQLLockCollection _lockCollection;
        private object _lock = new object();
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public LockManager(IDbManager db)
        {
            _db = db;
            _lockCollection = new SQLLockCollection();
        }
        #endregion

        #region Public Methods
        public bool TryLockObjects(LockObjectRequestCollection requests, string userName,
            Guid transactionBatchId,
            Guid transactionId,
            out List<SQLLock> heldLocks)
        {
            heldLocks = null;
            bool requestsCanBeLocked = true;

            lock (_lock)
            {
                foreach (var request in requests)
                {
                    if (request.ObjectType == ObjectType.Row)
                    {
                        // note: there needs to be some intelligence added here later
                        // for example, if we are requesting a lock on a row
                        // and the table that the row is a part of is going an exclusive lock
                        // we should return false here

                        var tableAddress = new TreeAddress(request.Address.DatabaseId, request.Address.TableId, request.Address.SchemaId);
                        var table = _db.GetTable(tableAddress);

                        var tableId = table.Schema().ObjectId;
                        var tableName = table.Name;

                        if (_lockCollection.Contains(tableName, tableId))
                        {
                            var tableLocktype = _lockCollection.GetLockTypeForObject(tableName, tableId);
                            if (tableLocktype == LockType.Exclusive)
                            {
                                requestsCanBeLocked = false;
                                heldLocks.Add(_lockCollection.GetCopyOfLock(tableName, tableId));
                                break;
                            }
                        }
                    }

                    if (_lockCollection.Contains(request.ObjectName, request.ObjectId))
                    {
                        requestsCanBeLocked = false;
                        // return the lock that's currently being held
                        heldLocks.Add(_lockCollection.GetCopyOfLock(request.ObjectName, request.ObjectId));
                        break;
                    }
                }

                // need to lock everything
                if (requestsCanBeLocked)
                {
                    foreach (var request in requests)
                    {
                        _lockCollection.Add
                            (
                            new SQLLock
                            (
                                userName,
                                transactionBatchId,
                                transactionId,
                                request.LockType,
                                request.Address,
                                request.ObjectName,
                                request.ObjectId,
                                request.ObjectType
                            )
                            );
                    }
                }
            }

            return requestsCanBeLocked;
        }
        public bool TryLockObjects(List<LockObjectRequest> requests,
            string userName,
            Guid transactionBatchId,
            Guid transactionId,
            out List<SQLLock> heldLocks)
        {
            heldLocks = null;
            bool requestsCanBeLocked = true;

            lock (_lock)
            {
                foreach (var request in requests)
                {
                    if (request.ObjectType == ObjectType.Row)
                    {
                        // note: there needs to be some intelligence added here later
                        // for example, if we are requesting a lock on a row
                        // and the table that the row is a part of is going an exclusive lock
                        // we should return false here

                        var tableAddress = new TreeAddress(request.Address.DatabaseId, request.Address.TableId, request.Address.SchemaId);
                        var table = _db.GetTable(tableAddress);

                        var tableId = table.Schema().ObjectId;
                        var tableName = table.Name;

                        if (_lockCollection.Contains(tableName, tableId))
                        {
                            var tableLocktype = _lockCollection.GetLockTypeForObject(tableName, tableId);
                            if (tableLocktype == LockType.Exclusive)
                            {
                                requestsCanBeLocked = false;
                                heldLocks.Add(_lockCollection.GetCopyOfLock(tableName, tableId));
                                break;
                            }
                        }
                    }

                    if (_lockCollection.Contains(request.ObjectName, request.ObjectId))
                    {
                        requestsCanBeLocked = false;
                        // return the lock that's currently being held
                        heldLocks.Add(_lockCollection.GetCopyOfLock(request.ObjectName, request.ObjectId));
                        break;
                    }
                }

                // need to lock everything
                if (requestsCanBeLocked)
                {
                    foreach (var request in requests)
                    {
                        _lockCollection.Add
                            (
                            new SQLLock
                            (
                                userName,
                                transactionBatchId,
                                transactionId,
                                request.LockType,
                                request.Address,
                                request.ObjectName,
                                request.ObjectId,
                                request.ObjectType
                            )
                            );
                    }
                }
            }

            return requestsCanBeLocked;
        }

        public bool TryReleaseLocks(Guid transactionBatchId)
        {
            bool result = false;

            lock (_lock)
            {
                result = _lockCollection.RemoveLocksForTransactionBatch(transactionBatchId);
            }

            return result;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
