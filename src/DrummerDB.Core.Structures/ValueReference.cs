using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal struct ValueReference
    {
        public int TableId;
        public string TableName;
        public ColumnSchemaStruct Column;
        public ValueAddress Address;
    }
}
