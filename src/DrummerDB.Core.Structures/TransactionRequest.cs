using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Holds part of a transaction information to be saved to disk
    /// by the appropriate object when an action is taken
    /// </summary>
    struct TransactionRequest
    {
        public Guid TransactionBatchId;
        public string UserName;
        public Guid PlanId;
        public DateTime TransactionStart;

        public static TransactionRequest GetEmpty()
        {
            var result = new TransactionRequest();
            return result;
        }
    }
}
