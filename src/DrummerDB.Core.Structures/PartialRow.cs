﻿using Drummersoft.DrummerDB.Common;
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
    internal class PartialRow : RowValueCollection, IRowRemotable
    {
        #region Private Fields
        private RowPreamble _preamble;
        #endregion

        #region Public Properties
        public override RowType Type => RowType.RemotableAndLocal;
        public Guid RemoteId { get; set; }
        public bool IsRemoteDeleted { get; set; }
        public DateTime RemoteDeletionUTC { get; set; }
        public uint DataHashLength => (uint)DataHash.Length;
        public byte[] DataHash => GetRowHash();

        #endregion

        #region Constructors
        public PartialRow(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }
        #endregion

        #region Public Methods
        public override byte[] GetRowInPageBinaryFormat()
        {
            return GetRowInBinaryFormat();
        }

        public override byte[] GetRowInTransactionBinaryFormat()
        {
            return GetRowInBinaryFormat();
        }
        #endregion

        #region Private Methods


        private byte[] GetRowInBinaryFormat()
        {
            var valueData = GetRowDataInBinary();
            _preamble.Type = Type;
            _preamble.RowValueSize = (uint)valueData.Length;

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
            _preamble.RowTotalSize = 
                _preamble.RowRemotableSize + 
                _preamble.RowValueSize + 
                (uint)RowConstants.Preamble.Length();

            var finalArrays = new List<byte[]>(2);
            finalArrays.Add(_preamble.ToBinaryFormat());
            finalArrays.Add(valueData);
            finalArrays.Add(bRemoteData);

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

        /// <summary>
        /// Computes the row hash data from RowValues, sets the propery of the Hash, and returns the it to the caller
        /// </summary>
        /// <returns>A hash of the row data's values</returns>
        private byte[] GetRowHash()
        {
            // ideally this code should be in Drummersoft.DrummerDB.Core.Cryptogrpahy
            // but the dependencies wouldn't work (would result in a circular reference)
            // may later change the dependency layout, but for now leaving this here
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-6.0
            var sourceData = GetRowDataInBinary();
            var sha256Hash = SHA256.Create();
            return sha256Hash.ComputeHash(sourceData);
        }
        #endregion
    }
}
