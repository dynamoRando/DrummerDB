using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A row that temporarily holds data to be sent to a participant or returned from a participant, 
    /// but whose data is not saved to memory or disk
    /// </summary>
    internal class TempParticipantRow : RowValueGroup
    {
        private RowPreamble _preamble;

        public RowType RowType => RowType.TempParticipantRow;
        public Participant Participant { get; set; }
        public Guid ParticipantInternalId => Participant.InternalId;
        public bool IsRemoteDeleted { get; set; }
        public DateTime RemoteDeletedUTC { get; set; }

        public TempParticipantRow(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }

        public TempParticipantRow(RowPreamble preamble, Participant participant) : base(preamble)
        {
            _preamble = preamble;
            Participant = participant;
        }

        /// <summary>
        /// Converts this row to a host row (remotable data only, no values)
        /// </summary>
        /// <returns></returns>
        public HostRow ToHostRow()
        {
            var preamble = new RowPreamble(_preamble.Id, RowType.Remoteable);
            var row = new HostRow(preamble);
            var dt = new DateTime();
            dt = DateTime.MinValue;
            var rowHashData = GetRowHash();

            var remotableData = new RemotableFixedData(ParticipantInternalId, false, dt, RemoteType.Participant, (uint)rowHashData.Length);
            row.SetRemotableFixedData(remotableData);
            row.SetDataHash(rowHashData);

            return row;
        }
    }
}
