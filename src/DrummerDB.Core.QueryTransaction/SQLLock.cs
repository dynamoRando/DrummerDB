using Drummersoft.DrummerDB.Core.QueryTransaction.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class SQLLock
    {
        #region Private Fields
        #endregion

        #region Public Properties
        public readonly Guid Id;
        public readonly DateTime LockStartTimeUTC;
        public readonly string UserName;
        public readonly Guid TransactionBatchId;
        public readonly Guid TransactionId;
        public readonly LockType LockType;
        public SQLAddress LockAddress;
        public readonly string ObjectName;
        public readonly Guid ObjectId;
        public readonly ObjectType ObjectType;
        #endregion

        #region Constructors
        public SQLLock(string userName, Guid transactionBatchId, Guid transactionId,
            LockType lockType, SQLAddress lockAddress, string objectName, Guid objectId,
            ObjectType objectType)
        {
            Id = Guid.NewGuid();
            LockStartTimeUTC = DateTime.UtcNow;

            UserName = userName;
            TransactionBatchId = transactionBatchId;
            TransactionId = transactionId;
            LockType = lockType;
            LockAddress = lockAddress;
            ObjectName = objectName;
            ObjectId = objectId;
            ObjectType = objectType;
        }

        public SQLLock(Guid id, DateTime lockStartTimeUTC, string userName, Guid transactionBatchId, Guid transactionId,
         LockType lockType, SQLAddress lockAddress, string objectName, Guid objectId,
         ObjectType objectType)
        {
            Id = id;
            LockStartTimeUTC = lockStartTimeUTC;
            UserName = userName;
            TransactionBatchId = transactionBatchId;
            TransactionId = transactionId;
            LockType = lockType;
            LockAddress = lockAddress;
            ObjectName = objectName;
            ObjectId = objectId;
            ObjectType = objectType;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
