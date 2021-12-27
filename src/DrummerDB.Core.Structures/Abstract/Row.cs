using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    internal abstract class Row : IRow
    {
        #region Private Fields
        private RowPreamble _preamble;
        #endregion

        #region Public Properties
        public uint Id => _preamble.Id;
        public RowType Type => _preamble.Type;
        public bool IsLogicallyDeleted => _preamble.IsLogicallyDeleted;
        public bool IsForwarded => _preamble.IsForwarded;
        public uint ForwardOffset => _preamble.ForwardOffset;
        public uint ForwardedPageId => _preamble.ForwardedPageId;
        public uint TotalSize => _preamble.RowTotalSize;
        public uint RemoteSize => _preamble.RowRemotableSize;
        public uint ValueSize => _preamble.RowValueSize;
        #endregion

        #region Constructors
        public Row(RowPreamble preamble)
        {
            _preamble = preamble;
        }

        public abstract byte[] GetRowInPageBinaryFormat();
        public abstract byte[] GetRowInTransactionBinaryFormat();
        public abstract void ForwardRow(int newOffset, int pageId);
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
