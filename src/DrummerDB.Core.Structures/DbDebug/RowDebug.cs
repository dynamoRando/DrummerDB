using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Structures.DbDebug
{
    /// <summary>
    /// Used in debugging. Most of these functions are copy pasted from
    /// <seealso cref="Row"/>
    /// </summary>
    internal class RowDebug
    {
        #region Private Fields
        private byte[] _data;
        private byte[] _preamble;
        private byte[] _rowdata;
        private byte[] _rowsize;
        private byte[] _participant;
        private ITableSchema _schema;
        #endregion

        #region Public Properties
        public List<RowValueDebug> Values { get; set; }
        #endregion

        #region Constructors
        public RowDebug()
        {
            Values = new List<RowValueDebug>();
        }

        public RowDebug(ReadOnlySpan<byte> preamble, ReadOnlySpan<byte> rowData, ITableSchema schema)
        {
            var array = new byte[preamble.Length + rowData.Length];
            Array.Copy(preamble.ToArray(), 0, array, 0, preamble.Length);
            Array.Copy(rowData.ToArray(), 0, array, preamble.Length, rowData.Length);

            _preamble = preamble.ToArray();
            _rowdata = rowData.ToArray();

            _data = array;
            _schema = schema;

            Values = new List<RowValueDebug>();

            SetRowData();
        }
        #endregion

        #region Public Methods

        public void SetParticipant(ReadOnlySpan<byte> participant)
        {
            _participant = participant.ToArray();
        }

        public Guid ParticipantId()
        {
            return DbBinaryConvert.BinaryToGuid(_participant);
        }

        public string ParticipantIdDebug()
        {
            return BitConverter.ToString(_participant);
        }

        public void SetRowSize(ReadOnlySpan<byte> size)
        {
            _rowsize = size.ToArray();
        }

        public int RowSize()
        {
            return DbBinaryConvert.BinaryToInt(_rowsize);
        }

        public string RowSizeDebug()
        {
            return BitConverter.ToString(_rowsize);
        }

        public void SetSchema(TableSchema schema)
        {
            _schema = schema;
        }

        public void SetPreamble(ReadOnlySpan<byte> preamble)
        {
            _preamble = preamble.ToArray();
        }

        public void SetRowData(ReadOnlySpan<byte> rowData)
        {
            _rowdata = rowData.ToArray();

            if (_data is null && _preamble is not null)
            {
                var array = new byte[_preamble.Length + rowData.Length];
                Array.Copy(_preamble.ToArray(), 0, array, 0, _preamble.Length);
                Array.Copy(rowData.ToArray(), 0, array, _preamble.Length, rowData.Length);
                _data = array;
            }

            SetRowData();
        }

        public string RowPreambleDebug()
        {
            return BitConverter.ToString(_preamble);
        }

        public string RowDataDebug()
        {
            return BitConverter.ToString(_rowdata);
        }

        public int ForwardedPageId()
        {
            var preamble = new ReadOnlySpan<byte>(_preamble);
            return DbBinaryConvert.BinaryToInt(preamble.Slice(RowConstants.ForwardedPageIdOffset(), RowConstants.SIZE_OF_FORWARDED_PAGE_ID));
        }

        public string ForwardedPageIdDebug()
        {
            return DebugSlice(RowConstants.ForwardedPageIdOffset(), RowConstants.SIZE_OF_FORWARDED_PAGE_ID);
        }

        public int ForwardOffset()
        {
            var preamble = new ReadOnlySpan<byte>(_preamble);
            return DbBinaryConvert.BinaryToInt(preamble.Slice(RowConstants.ForwardOffset(), RowConstants.SIZE_OF_FORWARD_OFFSET));
        }

        public string ForwardOffsetDebug()
        {
            return DebugSlice(RowConstants.ForwardOffset(), RowConstants.SIZE_OF_FORWARD_OFFSET);
        }

        public bool IsForwarded()
        {
            var preamble = new ReadOnlySpan<byte>(_preamble);
            return DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.IsForwardedOffset(), RowConstants.SIZE_OF_IS_FORWARDED));
        }

        public string IsForwardedDebug()
        {
            return DebugSlice(RowConstants.IsForwardedOffset(), RowConstants.SIZE_OF_IS_FORWARDED);
        }

        public bool IsDeleted()
        {
            var preamble = new ReadOnlySpan<byte>(_preamble);
            return DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.IsDeletedOffset(), RowConstants.SIZE_OF_IS_DELETED));
        }

        public string IsDeletedDebug()
        {
            return DebugSlice(RowConstants.IsDeletedOffset(), RowConstants.SIZE_OF_IS_DELETED);
        }

        public int RowId()
        {
            var preamble = new ReadOnlySpan<byte>(_preamble);
            return DbBinaryConvert.BinaryToInt(preamble.Slice(RowConstants.RowIdOffset(), RowConstants.SIZE_OF_ROW_ID));
        }

        public bool IsLocal()
        {
            var preamble = new ReadOnlySpan<byte>(_preamble);
            return DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.IsLocalOffset(), RowConstants.SIZE_OF_IS_LOCAL));
        }

        public string IsLocalDebug()
        {
            return DebugSlice(RowConstants.IsLocalOffset(), RowConstants.SIZE_OF_IS_LOCAL);
        }

        public string RowIdDebug()
        {
            return DebugSlice(RowConstants.RowIdOffset(), RowConstants.SIZE_OF_ROW_ID);
        }

        public string DebugSlice(int index, int length)
        {
            var slice = new ReadOnlySpan<byte>(_data, index, length);
            return BitConverter.ToString(slice.ToArray());
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// This is a copy from <see cref="Row.SetRowData(ITableSchema, ReadOnlySpan{byte})"/>
        /// </summary>
        private void SetRowData()
        {
            var values = new List<RowValueDebug>();
            int runningTotal = 0;
            var span = new ReadOnlySpan<byte>(_rowdata);

            if (_schema is not null)
            {
                if (_schema is TableSchema)
                {
                    _schema = _schema as TableSchema;
                    _schema.SortBinaryOrder();
                    foreach (var column in _schema.Columns)
                    {
                        var rowValue = new RowValueDebug();
                        rowValue.Column = column;

                        if (!column.IsNullable)
                        {
                            if (column.IsFixedBinaryLength)
                            {
                                if (column.DataType is SQLInt)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                    runningTotal += Constants.SIZE_OF_INT;
                                    rowValue.ParseValueLength = runningTotal;
                                }

                                if (column.DataType is SQLDateTime)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DATETIME));
                                    runningTotal += Constants.SIZE_OF_DATETIME;
                                    rowValue.ParseValueLength = runningTotal;
                                }

                                if (column.DataType is SQLDecimal)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DECIMAL));
                                    runningTotal += Constants.SIZE_OF_DECIMAL;
                                    rowValue.ParseValueLength = runningTotal;
                                }

                                if (column.DataType is SQLBit)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_BOOL));
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    rowValue.ParseValueLength = runningTotal;
                                }
                            }
                            else
                            {
                                if (column.DataType is SQLVarChar)
                                {
                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                    runningTotal += Constants.SIZE_OF_INT;
                                    string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
                                    rowValue.SetValue(value);
                                    runningTotal += length;
                                    rowValue.ParseValueLength = runningTotal;
                                }

                                if (column.DataType is SQLChar)
                                {
                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                    runningTotal += Constants.SIZE_OF_INT;
                                    string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
                                    rowValue.SetValue(value);
                                    runningTotal += length;
                                    rowValue.ParseValueLength = runningTotal;
                                }

                                if (column.DataType is SQLBinary)
                                {
                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                    runningTotal += Constants.SIZE_OF_INT;
                                    rowValue.SetValue(span.Slice(runningTotal, length).ToArray());
                                    runningTotal += length;
                                    rowValue.ParseValueLength = runningTotal;
                                }

                                if (column.DataType is SQLVarbinary)
                                {
                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                    runningTotal += Constants.SIZE_OF_INT;
                                    rowValue.SetValue(span.Slice(runningTotal, length).ToArray());
                                    runningTotal += length;
                                    rowValue.ParseValueLength = runningTotal;
                                }
                            }
                        }
                        else // is nullable column
                        {
                            bool isNull = DbBinaryConvert.BinaryToBoolean(span.Slice(runningTotal, Constants.SIZE_OF_BOOL));

                            if (isNull)
                            {
                                rowValue.SetValueAsNull();
                                runningTotal += Constants.SIZE_OF_BOOL;
                                rowValue.ParseValueLength = runningTotal;
                            }
                            else
                            {
                                if (column.IsFixedBinaryLength)
                                {
                                    // for reading values back off the page, we need to set _value in the row value
                                    // to have the leading IsNull bit
                                    // so parse with the leading bit, then add back in the size of bit (bool)
                                    // to keep our running count correct

                                    if (column.DataType is SQLInt)
                                    {
                                        rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL));
                                        runningTotal += Constants.SIZE_OF_INT;
                                        runningTotal += Constants.SIZE_OF_BOOL;
                                        rowValue.ParseValueLength = runningTotal;
                                    }

                                    if (column.DataType is SQLDateTime)
                                    {
                                        rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DATETIME + Constants.SIZE_OF_BOOL));
                                        runningTotal += Constants.SIZE_OF_DATETIME;
                                        runningTotal += Constants.SIZE_OF_BOOL;
                                        rowValue.ParseValueLength = runningTotal;
                                    }

                                    if (column.DataType is SQLDecimal)
                                    {
                                        rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DECIMAL + Constants.SIZE_OF_BOOL));
                                        runningTotal += Constants.SIZE_OF_DECIMAL;
                                        runningTotal += Constants.SIZE_OF_BOOL;
                                        rowValue.ParseValueLength = runningTotal;
                                    }

                                    if (column.DataType is SQLBit)
                                    {
                                        rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_BOOL + Constants.SIZE_OF_BOOL));
                                        runningTotal += Constants.SIZE_OF_BOOL;
                                        runningTotal += Constants.SIZE_OF_BOOL;
                                        rowValue.ParseValueLength = runningTotal;
                                    }
                                }
                                else
                                {
                                    // this behavior differs from fixed values, which need the leading IsNull bit
                                    // set in _value 
                                    // varchar and char types just set the string value directly without parsing the isNull bit
                                    // whenever we retrieve the value (elsewhere) we append the leading isNull byte to the string value

                                    if (column.DataType is SQLVarChar)
                                    {
                                        // it's not null, but we need to account for it, so fast forward by the size of the "IsNull" value
                                        runningTotal += Constants.SIZE_OF_BOOL;

                                        int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                        runningTotal += Constants.SIZE_OF_INT;
                                        string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
                                        rowValue.SetValue(value);
                                        runningTotal += length;
                                        rowValue.ParseValueLength = runningTotal;
                                    }

                                    if (column.DataType is SQLChar)
                                    {
                                        // it's not null, but we need to account for it, so fast forward by the size of the "IsNull" value
                                        runningTotal += Constants.SIZE_OF_BOOL;

                                        int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                        runningTotal += Constants.SIZE_OF_INT;
                                        string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
                                        rowValue.SetValue(value);
                                        runningTotal += length;
                                        rowValue.ParseValueLength = runningTotal;
                                    }

                                    if (column.DataType is SQLBinary)
                                    {
                                        // for binary types, if we're not already NULL (we read this earlier) then we have the size prefix and the byte array
                                        // so go ahead and fast forward by the IsNull value
                                        runningTotal += Constants.SIZE_OF_BOOL;

                                        int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                        runningTotal += Constants.SIZE_OF_INT;
                                        rowValue.SetValue(span.Slice(runningTotal, length).ToArray());
                                        runningTotal += length;
                                        rowValue.ParseValueLength = runningTotal;
                                    }

                                    if (column.DataType is SQLVarbinary)
                                    {
                                        // for binary types, if we're not already NULL (we read this earlier) then we have the size prefix and the byte array
                                        // so go ahead and fast forward by the IsNull value
                                        runningTotal += Constants.SIZE_OF_BOOL;

                                        int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));
                                        runningTotal += Constants.SIZE_OF_INT;
                                        rowValue.SetValue(span.Slice(runningTotal, length).ToArray());
                                        runningTotal += length;
                                        rowValue.ParseValueLength = runningTotal;
                                    }
                                }
                            }
                        }

                        values.Add(rowValue);
                    }
                }
            }

            Values = values;

        }
        #endregion

    }
}
