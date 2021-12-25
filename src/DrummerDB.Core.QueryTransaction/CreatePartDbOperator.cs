using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class CreatePartDbOperator : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;
        private Contract _contract;
        public int Order { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public CreatePartDbOperator(IDbManager dbManager, Contract contract)
        {
            _db = dbManager;
            _contract = contract;
        }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            bool partDbCreated = false;
            if (!_db.HasUserDatabase(_contract.DatabaseName, DatabaseType.Partial))
            {
                if (_db is DbManager)
                {
                    var db = _db as DbManager;
                    Guid dbId;

                    partDbCreated = db.XactCreateNewPartialDatabase(_contract, transaction, transactionMode, out dbId);

                    if (partDbCreated)
                    {
                        messages.Add($"Database {_contract.DatabaseName} created with Id {dbId.ToString()}");
                        var partDb = _db.GetPartialDb(_contract.DatabaseName);

                        // need to create all user tables
                        foreach (var table in _contract.Tables)
                        {
                            var tableSchema = table as TableSchema;
                            tableSchema.ResetDbId(dbId);
                            partDb.AddTable(tableSchema, out _);
                        }
                    }
                }
            }
        }
    }
}
