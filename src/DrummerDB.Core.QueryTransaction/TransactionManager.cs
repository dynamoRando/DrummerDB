using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using Drummersoft.DrummerDB.Core.Structures.Interface;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    /// <summary>
    /// Tracks all <see cref="TransactionRequest"/>s.
    /// </summary>
    internal class TransactionManager : ITransactionManager
    {
        #region Private Fields
        private TransactionRequestCollection _transactions;
        // will be used to report details out of in flight transactions
        private ITransactionEntryManager _entryManager;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public TransactionManager(ITransactionEntryManager entryManager)
        {
            _transactions = new TransactionRequestCollection();
            _entryManager = entryManager;
        }
        #endregion

        #region Public Methods
        public Guid GetPendingBatchTransactionId()
        {
            return Guid.NewGuid();
        }

        public TransactionRequest EnqueueBatchTransaction(Guid batchTransactionId, string userName, Guid planId)
        {
            var request = new TransactionRequest { TransactionBatchId = batchTransactionId, PlanId = planId, UserName = userName, TransactionStart = DateTime.UtcNow };

            if (!_transactions.Contains(request))
            {
                _transactions.Add(request);
                return request;
            }
            else
            {
                throw new InvalidOperationException($"There is already an open tranasction batch id: {request.TransactionBatchId.ToString()}");
            }
        }

        public bool DequeueBatchTransaction(Guid batchTransactionId)
        {
            var request = new TransactionRequest { TransactionBatchId = batchTransactionId, PlanId = Guid.Empty, UserName = string.Empty, TransactionStart = DateTime.UtcNow };

            if (_transactions.Contains(request))
            {
                _transactions.Remove(request);
                return true;
            }

            return false;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
