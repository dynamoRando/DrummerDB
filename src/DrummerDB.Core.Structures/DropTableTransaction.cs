using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class DropTableTransaction : TransactionActionSchema
    {
        private TableSchema _schema;
        private ITable _table;
        private List<IPage> _pages;

        public TableSchema Schema => _schema;
        public ITable Table => _table;
        public List<IPage> Pages => _pages;

        public DropTableTransaction(TableSchema schema, ITable table, List<IPage> pages)
        {
            _schema = schema;
            _table = table;
            _pages = pages;
        }

        public override TransactionSchemaOperation Operation => TransactionSchemaOperation.DropTable;

        public override byte[] GetDataInTransactionBinaryFormat()
        {
            throw new NotImplementedException();
        }
    }
}
