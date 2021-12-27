using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class LocalRow : Row, IRowValueCollection
    {
        public LocalRow(RowPreamble preamble) : base(preamble)
        {
        }

        public IRowValue[] Values { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void ForwardRow(int newOffset, int pageId)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetRowInPageBinaryFormat()
        {
            throw new NotImplementedException();
        }

        public override byte[] GetRowInTransactionBinaryFormat()
        {
            throw new NotImplementedException();
        }

        public byte[] GetValueInByte(string columnName)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<byte> GetValueInByteSpan(string columnName)
        {
            throw new NotImplementedException();
        }

        public string GetValueInString(string columnName)
        {
            throw new NotImplementedException();
        }

        public bool IsValueNull(string columnName)
        {
            throw new NotImplementedException();
        }

        public void SetRowData(ITableSchema schema, ReadOnlySpan<byte> span)
        {
            throw new NotImplementedException();
        }

        public void SetValue(string columnName, string value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(string columnName, byte[] value)
        {
            throw new NotImplementedException();
        }

        public void SetValueAsNullForColumn(string columnName)
        {
            throw new NotImplementedException();
        }

        public void SortBinaryOrder()
        {
            throw new NotImplementedException();
        }

        public void SortOrdinalOrder()
        {
            throw new NotImplementedException();
        }
    }
}
