using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    enum TransactionSchemaOperation
    {
        CreateTable,
        CreateDatabase,
        AlterTable,
        AlterColumn,
        DropTable,
        DropDatabase,
        Unknown
    }
}
