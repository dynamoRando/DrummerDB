using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class InsertRowToPartialDbAction : IDatabaseServiceAction
    {
        private Guid _id;
        private Row _row;
        private PartialDb _db;
        private Table _table;

        public Guid Id => _id;

        public InsertRowToPartialDbAction(Row row, PartialDb database, Table table)
        {
            _id = Guid.NewGuid();
            _row = row;
            _db = database;
            _table = table;
        }

        public bool Execute(TransactionRequest transaction, TransactionMode transactionMode, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (_table.XactAddRow(_row, transaction, transactionMode))
            {
                return true;
            }
            else
            {
                errorMessage = $"Unable to insert row into database {_db.Name} into table {_table.Name}";
                return false;
            }

        }
    }
}
