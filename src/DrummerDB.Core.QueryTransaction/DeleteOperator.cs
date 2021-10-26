using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    internal class DeleteOperator : ISQLNonQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        #endregion

        #region Public Properties
        public readonly TreeAddress Address;
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string DatabaseName { get; set; }
        #endregion

        #region Constructors
        public DeleteOperator(IDbManager db, TreeAddress address)
        {
            _db = db;
            Address = address;
        }
        #endregion

        #region Public Methods
        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            Table table = _db.GetTable(Address);
            
            // if we have a WHERE clause that we need to specify
            if (PreviousOperation is not null)
            {
                if (PreviousOperation is TableReadOperator)
                {
                    var readOp = PreviousOperation as TableReadOperator;
                    var filter = readOp.Result;
                    var targets = filter.Rows();

                    int tableCount = targets.Item1.Count();

                    // if we only are updating 1 table
                    if (tableCount == 1)
                    {
                        // make sure the target table is the same one we're updating
                        var targetTable = targets.Item1.First();
                        if (targetTable == Address)
                        {
                            foreach (var rowAddress in targets.Item2)
                            {
                                var row = table.GetRow(rowAddress);
                                if (table.TryDeleteRow(row, transaction, transactionMode))
                                {
                                    messages.Add("DELETE completed successfully");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // delete all rows in the table
                // this still works because in StatementPlanEvaluator.cs in EvalutateQueryPlanForDelete() we set the target rows to be every row in the table
            }
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
