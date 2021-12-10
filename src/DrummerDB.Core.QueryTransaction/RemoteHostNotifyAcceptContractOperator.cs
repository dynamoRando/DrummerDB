﻿using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class RemoteHostNotifyAcceptContractOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private PartialDb _partDb;
        private Contract _contract;
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public RemoteHostNotifyAcceptContractOperator(PartialDb db, Contract contract)
        {
            _partDb = db;
            _contract = contract;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            _partDb.XactNotifyHostAcceptedContract(_contract, transaction, transactionMode);
            throw new NotImplementedException();
        }
    }
}
