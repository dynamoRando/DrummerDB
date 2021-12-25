using Drummersoft.DrummerDB.Core.Databases;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.QueryTransaction
{
    // updates a table from the specified sources
    // we need to intake sources and targets
    // a source may be another table (as in a JOIN statement in an UPDATE statement)
    // or a source may be a value that the user passed in, i.e. FOO = 'BAR'
    // and at target are the rows specified in a WHERE clause
    // from a TableReadOperator (which returns a List<ValueAddres>)
    // or a target may be the entire table
    internal class UpdateOperator : ISQLNonQueryable, IQueryPlanPartOperator
    {
        #region Private Fields
        private IDbManager _db;
        private List<IUpdateColumnSource> _sources;
        #endregion

        #region Public Properties
        public readonly TreeAddress Address;
        public IQueryPlanPartOperator PreviousOperation { get; set; }
        public IQueryPlanPartOperator NextOperation { get; set; }
        public string DatabaseName { get; set; }
        #endregion

        #region Constructors
        public UpdateOperator(IDbManager db, TreeAddress address, List<IUpdateColumnSource> sources)
        {
            _db = db;
            Address = address;
            _sources = sources;
        }
        #endregion

        #region Public Methods
        public void Execute(TransactionRequest transaction, TransactionMode transactionMode, ref List<string> messages, ref List<string> errorMessages)
        {
            Table table = _db.GetTable(Address);
            bool rowsUpdated = true;

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
                                foreach (var source in _sources)
                                {
                                    if (source is UpdateTableValue)
                                    {
                                        var updateValue = source as UpdateTableValue;

                                        if (table.HasColumn(updateValue.Column.ColumnName))
                                        {
                                            row.SetValue(updateValue.Column.ColumnName, updateValue.Value);
                                            if (!table.XactUpdateRow(row, transaction, transactionMode))
                                            {
                                                rowsUpdated = false;
                                            }
                                        }
                                        else
                                        {
                                            errorMessages.Add(
                                                $"Tried to update column {updateValue.Column.ColumnName} which is not in table {table.Name}");
                                        }
                                    }
                                }
                            }

                            if (rowsUpdated)
                            {
                                messages.Add($"{targets.Item2.Count.ToString()} rows updated in table {table.Name}");
                            }
                        }
                    }
                }
            }
            else
            {
                // update all rows in table
                // this still works because in StatementPlanEvaluator.cs in EvaluateQueryPlanForUpdate() we set the TableReadOperator to the entire table.
            }
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
