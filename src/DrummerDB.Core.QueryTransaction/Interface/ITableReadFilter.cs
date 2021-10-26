﻿using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction.Interface
{
    internal interface ITableReadFilter
    {
        int Order { get; set; }
        public List<RowAddress> GetRows(IDbManager db, TransactionRequest transaction, TransactionMode transactionMode);
    }
}
