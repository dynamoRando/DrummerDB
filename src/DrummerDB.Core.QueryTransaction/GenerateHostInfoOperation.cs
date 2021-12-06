using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System;
using static Drummersoft.DrummerDB.Core.Databases.Version.SystemDatabaseConstants100;
using Drummersoft.DrummerDB.Core.Cryptography;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class GenerateHostInfoOperation : IQueryPlanPartOperator, ISQLNonQueryable
    {
        private IDbManager _db;
        public string HostName { get; set; }
        public int Order { get; set; }
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }

        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            var sysDb = _db.GetSystemDatabase();
            var hostInfoTable = sysDb.GetTable(Tables.HostInfo.TABLE_NAME);

            if (hostInfoTable.RowCount() > 0)
            {
                throw new InvalidOperationException("There is already host information set");
            }

            var token = CryptoManager.GenerateToken();
            var id = Guid.NewGuid();

            var row = hostInfoTable.GetNewLocalRow();
            row.SetValue(Tables.HostInfo.Columns.HostGUID, id.ToString());
            row.SetValue(Tables.HostInfo.Columns.HostName, HostName);
            row.SetValue(Tables.HostInfo.Columns.Token, token);

            if (!hostInfoTable.TryAddRow(row, transaction, transactionMode))
            {
                errorMessages.Add($"Unable to add host information with name {HostName}");
            }
            else
            {
                messages.Add($"Succesfully added host information {HostName} with generated id {id.ToString()}";
            }
        }

        public GenerateHostInfoOperation(string hostName, IDbManager dbManager)
        {
            HostName = hostName;
            _db = dbManager;
        }
    }
}
