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
        public virtual RowType Type => _preamble.Type;
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


        #endregion

        #region Public Methods
        /// <summary>
        /// Determines if the Row can be cast as a <see cref="RowValueGroup"/>
        /// </summary>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool IsValueGroup()
        {
            return this is RowValueGroup;
        }

        /// <summary>
        /// Cassts the row to a <see cref="RowValueGroup"/>
        /// </summary>
        /// <returns>The row as a RowValueGroup if successfu, otherwise <c>NULL</c></returns>
        public RowValueGroup AsValueGroup()
        {
            return this as RowValueGroup;
        }

        /// <summary>
        /// Casts the row to a <see cref="LocalRow"/>
        /// </summary>
        /// <returns>The row as a LocalRow if successful, otherwise <c>NULL</c></returns>
        /// <remarks>If you need the values, it is safer to cast to <see cref="RowValueGroup"/> based on the 
        /// inheritance chain</remarks>
        public LocalRow AsLocal()
        {
            return this as LocalRow;
        }

        /// <summary>
        /// Determines if the Row can be cast as a <see cref="LocalRow"/>
        /// </summary>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool IsLocal()
        {
            return this is LocalRow;
        }

        /// <summary>
        /// Casts the row to a <see cref="PartialRow"/>
        /// </summary>
        /// <returns>The row as a LocalRow if successful, otherwise <c>NULL</c></returns>
        public PartialRow AsPartial()
        {
            return this as PartialRow;
        }

        /// <summary>
        /// Determines if the Row can be cast as a <see cref="PartialRow"/>
        /// </summary>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool IsPartial()
        {
            return this is PartialRow;
        }

        /// <summary>
        /// Casts the row to a <see cref="HostRow"/>
        /// </summary>
        /// <returns>The row as a HostRow if successful, otherwise <c>NULL</c></returns>
        public HostRow AsHost()
        {
            return this as HostRow;
        }

        /// <summary>
        /// Determines if the Row can be cast as a <see cref="HostRow"/>
        /// </summary>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool IsHost()
        {
            return this is HostRow;
        }

        /// <summary>
        /// Determines if the Row can be cast as a <see cref="TempParticipantRow"/>
        /// </summary>
        /// <returns><c>TRUE</c> if successful, otherwise <c>FALSE</c></returns>
        public bool IsTempForParticipant()
        {
            return this is TempParticipantRow;
        }

        /// <summary>
        /// Casts the row to a <see cref="TempParticipantRow"/>
        /// </summary>
        /// <returns>The row as a TempParticipantRow if successful, otherwise <c>NULL</c></returns>
        public TempParticipantRow AsTempForParticipant()
        {
            return this as TempParticipantRow;
        }

        /// <summary>
        /// Determines if the Row has remote data associated with it
        /// </summary>
        /// <returns><c>TRUE</c> if the row has remote data, otherwise <c>FALSE</c></returns>
        public bool IsRemotable()
        {
            var rowType = _preamble.Type;

            if (
                rowType == RowType.RemotableAndLocal || 
                rowType == RowType.Remoteable ||
                rowType == RowType.TempParticipantRow
                )
            {
                return true;
            }

            return false;
        }

        public bool HasLocalData()
        {
            RowType type = _preamble.Type;
            switch (type)
            {
                case RowType.Local:
                    return true;
                case RowType.RemotableAndLocal:
                    return true;
                case RowType.Remoteable:
                    return false;
                case RowType.TempParticipantRow:
                    return true;
                case RowType.Unknown:
                default:
                    throw new InvalidOperationException("Unknown row type");
            }
        }

        public abstract byte[] GetRowInPageBinaryFormat();
        public abstract byte[] GetRowInTransactionBinaryFormat();
        public abstract void ForwardRow(uint newOffset, uint pageId);
        public abstract void Delete();

        /// <summary>
        /// Determines if the row should have local values based on the type of row
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool HasLocalData(RowType type)
        {
            switch (type)
            {
                case RowType.Local:
                    return true;
                case RowType.RemotableAndLocal:
                    return true;
                case RowType.Remoteable:
                    return false;
                case RowType.TempParticipantRow:
                    return true;
                case RowType.Unknown:
                default:
                    throw new InvalidOperationException("Unknown row type");
            }
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
