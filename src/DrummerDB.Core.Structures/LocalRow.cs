using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents a standard row with a preamble and data.
    /// No remotable attributes associated with it.
    /// </summary>
    internal class LocalRow : RowValueGroup
    {
        public override RowType Type => RowType.Local;
        public LocalRow(RowPreamble preamble) : base(preamble)
        {
        }
    }
}
