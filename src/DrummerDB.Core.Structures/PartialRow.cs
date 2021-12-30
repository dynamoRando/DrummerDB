using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// A row that has <see cref="RowValue"/>s and has 
    /// <see cref="IRowRemotable"/> data about the host that refers to it
    /// </summary>
    internal class PartialRow : RowValueGroup, IRowRemotable
    {
        #region Private Fields
        private RowPreamble _preamble;
        private RemotableFixedData _remotableFixedData;
        #endregion

        #region Public Properties
        public override RowType Type => RowType.RemotableAndLocal;
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
        public uint DataHashLength => (uint)DataHash.Length;
        public byte[] DataHash => GetRowHash();
        public RemoteType RemoteType => RemoteType.Host;
        #endregion

        #region Constructors
        public PartialRow(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }
        #endregion

        #region Public Methods
        public new void SetValue(string columnName, string value) 
        {
            base.SetValue(columnName, value);
            SetSizes();
        }

        public new void SetValue(string columnName, byte[] value)
        {
            base.SetValue(columnName, value);
            SetSizes();
        }

        public new void SetRowData(ITableSchema schema, ReadOnlySpan<byte> span)
        {
            base.SetRowData(schema, span);
            SetSizes();
        }

        public new void SetValueAsNullForColumn(string columnName)
        {
            base.SetValueAsNullForColumn(columnName);
            SetSizes();
        }

        public void SetRemotableFixedData(ReadOnlySpan<byte> data)
        {
            _remotableFixedData = new RemotableFixedData(data);
            SetRemotableSize();
        }

        public void SetRemotableFixedData(RemotableFixedData data)
        {
            _remotableFixedData = data;
        }

        public override byte[] GetRowInPageBinaryFormat()
        {
            return GetRowInBinaryFormat();
        }

        public override byte[] GetRowInTransactionBinaryFormat()
        {
            return GetRowInBinaryFormat();
        }

        public bool HasRemotableDataSet()
        {
            return _remotableFixedData is not null;
        }

        #endregion

        #region Private Methods
        private byte[] GetRowInBinaryFormat()
        {
            var valueData = GetRowDataInBinary();
            _preamble.Type = Type;
            _preamble.RowValueSize = (uint)valueData.Length;

            _remotableFixedData.DataHashLength = (uint)DataHashLength;

            var arrays = new List<byte[]>(2);
            arrays.Add(_remotableFixedData.ToBinaryFormat());
            arrays.Add(DataHash);

            var bRemoteData = DbBinaryConvert.ArrayStitch(arrays);

            _preamble.RowRemotableSize = (uint)bRemoteData.Length;
            _preamble.RowTotalSize =
                _preamble.RowRemotableSize +
                _preamble.RowValueSize +
                RowConstants.Preamble.Length();

            var finalArrays = new List<byte[]>(3);
            finalArrays.Add(_preamble.ToBinaryFormat());
            finalArrays.Add(bRemoteData);
            finalArrays.Add(valueData);
            
            return DbBinaryConvert.ArrayStitch(finalArrays);
        }

        private byte[] GetRowDataInBinary()
        {
            SortBinaryOrder();
            List<byte[]> arrays = new List<byte[]>();

            foreach (var value in Values)
            {
                var bytes = value.GetValueInBinary();
                arrays.Add(bytes);
            }

            return DbBinaryConvert.ArrayStitch(arrays);
        }


        private void SetTotalSize()
        {
            _preamble.RowTotalSize = (uint)_preamble.ToBinaryFormat().Length + _preamble.RowRemotableSize + _preamble.RowValueSize;
        }

        private void SetRemotableSize()
        {
            _preamble.RowRemotableSize = 
                (uint)_remotableFixedData.ToBinaryFormat().Length +
                (uint)DataHash.Length
                ;
        }

        private void SetValueSize()
        {
            SortBinaryOrder();
            List<byte[]> arrays = new List<byte[]>();

            foreach (var value in Values)
            {
                if (value.IsDataSet())
                {
                    var bytes = value.GetValueInBinary();
                    arrays.Add(bytes);
                }
            }

            var totalArrays = DbBinaryConvert.ArrayStitch(arrays);
            _preamble.RowValueSize = (uint)totalArrays.Length;
        }

        private void SetSizes()
        {
            SetRemotableSize();
            SetValueSize();
            SetTotalSize();
        }
        #endregion
    }
}
