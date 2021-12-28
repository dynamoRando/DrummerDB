using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class TempParticipantRow : RowValueGroup
    {
        private RowPreamble _preamble;

        public RowType RowType => RowType.TempParticipantRow;
        public Participant Participant { get; set; }
        public Guid ParticipantId => Participant.Id;

        public TempParticipantRow(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }
    }
}
