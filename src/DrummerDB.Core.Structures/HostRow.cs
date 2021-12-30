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
    /// A row that is stored in a host database that does not have any
    /// <see cref="RowValue"/>s and only <see cref="IRowRemotable"/> information.
    /// </summary>
    internal class HostRow : Row, IRowRemotable
    {
        #region Private Fields
        private RowPreamble _preamble;
        private RemotableFixedData _remotableFixedData;
        private byte[] _dataHash;
        #endregion

        #region Public Properties
        public override RowType Type => RowType.Remoteable;

        public Guid RemoteId
        {
            get
            {
                return _remotableFixedData.RemoteId;
            }
            set
            {
                _remotableFixedData.RemoteId = value;
            }
        }

        public bool IsRemoteDeleted
        {
            get
            {
                return _remotableFixedData.IsRemoteDeleted;
            }
            set
            {
                _remotableFixedData.IsRemoteDeleted = value;
            }
        }

        public DateTime RemoteDeletionUTC
        {
            get
            {
                return _remotableFixedData.RemoteDeletionUTC;
            }

            set
            {
                _remotableFixedData.RemoteDeletionUTC = value;
            }
        }
        public uint DataHashLength => (uint)_dataHash.Length;
        public byte[] DataHash => _dataHash;
        public RemoteType RemoteType => RemoteType.Participant;
        #endregion

        #region Constructors
        public HostRow(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }
        #endregion

        #region Public Methods
        public bool HasRemotableDataSet()
        {
            return _remotableFixedData is not null;
        }

        public bool HasDataHashSet()
        {
            return _dataHash is not null;
        }

        public override void Delete()
        {
            _preamble.IsLogicallyDeleted = true;
        }

        public void SetRemotableFixedData(ReadOnlySpan<byte> data)
        {
            _remotableFixedData = new RemotableFixedData(data);
        }

        public void SetRemotableFixedData(RemotableFixedData data)
        {
            _remotableFixedData = data;
        }

        public void SetDataHash(byte[] dataHash)
        {
            _dataHash = dataHash;
            _remotableFixedData.DataHashLength = (uint)dataHash.Length;
        }
        #endregion

        #region Private Methods
        public override void ForwardRow(uint newOffset, uint pageId)
        {
            _preamble.IsForwarded = true;
            _preamble.ForwardOffset = newOffset;
            _preamble.ForwardedPageId = pageId;
        }

        public override byte[] GetRowInPageBinaryFormat()
        {
            return GetRowInBinaryFormat();
        }

        public override byte[] GetRowInTransactionBinaryFormat()
        {
            return GetRowInBinaryFormat();
        }

        private byte[] GetRowInBinaryFormat()
        {
            _preamble.Type = Type;
            _preamble.RowValueSize = 0;

            _remotableFixedData.DataHashLength = (uint)_dataHash.Length;

            var arrays = new List<byte[]>(2);
            arrays.Add(_remotableFixedData.ToBinaryFormat());
            arrays.Add(DataHash);

            var bRemoteData = DbBinaryConvert.ArrayStitch(arrays);

            _preamble.RowRemotableSize = (uint)bRemoteData.Length;
            _preamble.RowTotalSize = _preamble.RowRemotableSize + (uint)RowConstants.Preamble.Length();

            var finalArrays = new List<byte[]>(2);
            finalArrays.Add(_preamble.ToBinaryFormat());
            finalArrays.Add(bRemoteData);

            return DbBinaryConvert.ArrayStitch(finalArrays);

        }
        #endregion
    }
}
