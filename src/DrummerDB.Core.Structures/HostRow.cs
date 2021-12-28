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
        private byte[] _dataHash;
        #endregion

        #region Public Properties
        public override RowType Type => RowType.Remoteable;
        public Guid RemoteId { get; set; }
        public bool IsRemoteDeleted { get; set; }
        public DateTime RemoteDeletionUTC { get; set; }
        public uint DataHashLength => (uint)_dataHash.Length;
        public byte[] DataHash => _dataHash;
        #endregion

        #region Constructors
        public HostRow(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }

        public HostRow(RowPreamble preamble, byte[] dataHash) : base(preamble)
        {
            _preamble = preamble;
            _dataHash = dataHash;
        }
        #endregion

        #region Public Methods
        public void SetDataHash(byte[] dataHash)
        {
            _dataHash = dataHash;
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

            var arrays = new List<byte[]>(5);
            var bRemoteId = DbBinaryConvert.GuidToBinary(RemoteId);
            arrays.Add(bRemoteId);

            var bIsRemoteDeleted = DbBinaryConvert.BooleanToBinary(IsRemoteDeleted);
            arrays.Add(bIsRemoteDeleted);

            var bRemoteDeletedUTC = DbBinaryConvert.DateTimeToBinary(RemoteDeletionUTC.ToString());
            arrays.Add(bRemoteDeletedUTC);

            var bDataHashLength = DbBinaryConvert.UIntToBinary(DataHashLength); 
            arrays.Add(bDataHashLength);

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
