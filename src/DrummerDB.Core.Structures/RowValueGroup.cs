using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures
{
    /// <summary>
    /// Represents a concrete row with a preamble and has values
    /// </summary>
    internal class RowValueGroup : Row, IRowValueGroup
    {
        #region Private Fields
        RowPreamble _preamble;
        #endregion

        #region Public Properties
        public IRowValue[] Values { get; set; }

        #endregion

        #region Constructors
        public RowValueGroup(RowPreamble preamble) : base(preamble)
        {
            _preamble = preamble;
        }
        #endregion

        #region Public Methods
        public byte[] GetRowHash()
        {
            // ideally this code should be in Drummersoft.DrummerDB.Core.Cryptogrpahy
            // but the dependencies wouldn't work (would result in a circular reference)
            // may later change the dependency layout, but for now leaving this here
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-6.0
            var sourceData = GetRowDataInBinary();

            bool debug = false;

            if (debug)
            {
                Debug.WriteLine("Source Data Before Hash:");
                DebugByteArray(sourceData);
            }
           
            var sha256Hash = SHA256.Create();
            var hash = sha256Hash.ComputeHash(sourceData);

            if (debug)
            {
                Debug.WriteLine("Hashed Value:");
                DebugByteArray(hash);
            }
       
            return hash;
        }

        public override void Delete()
        {
            _preamble.IsLogicallyDeleted = true;
        }

        public uint Size()
        {
            return (uint)GetRowInBinaryFormat().Length;
        }

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

        /// <summary>
        /// Returns the value for the specified column in binary format. Does not include the size prefix if variable binary length.
        /// </summary>
        /// <param name="columnName">The column to get the value for</param>
        /// <returns>The value of the specified column in binary format</returns>
        public byte[] GetValueInByte(string columnName)
        {
            byte[] result = null;
            var rowValue = GetRowValueWithColumnName(columnName);
            if (rowValue is not null)
            {
                result = rowValue.GetValueInBinary(false);
            }

            return result;
        }

        public ReadOnlySpan<byte> GetValueInByteSpan(string columnName)
        {
            return GetRowValueWithColumnName(columnName).GetValueInByteSpan();
        }

        /// <summary>
        /// Returns the value for the specified column in string format
        /// </summary>
        /// <param name="columnName">The column to get the value for</param>
        /// <returns>The value of the specified column in string format</returns>
        public string GetValueInString(string columnName)
        {
            string result = string.Empty;
            var rowValue = GetRowValueWithColumnName(columnName);
            if (rowValue is not null)
            {
                result = rowValue.GetValueInString();
            }

            return result;
        }

        public bool IsValueNull(string columnName)
        {
            var rowValue = GetRowValueWithColumnName(columnName);
            return rowValue.IsNull();
        }

        public void SetRowData(ITableSchema schema, ReadOnlySpan<byte> span)
        {
            var values = new List<RowValue>();
            uint runningTotal = 0;

            if (schema is TableSchema)
            {
                schema = schema as TableSchema;
                schema.SortBinaryOrder();
                foreach (var column in schema.Columns)
                {
                    var rowValue = new RowValue();
                    rowValue.Column = column;

                    uint parseLength = 0;

                    if (!column.IsNullable) // nullable == false
                    {
                        if (column.IsFixedBinaryLength)
                        {
                            if (column.DataType is SQLInt)
                            {
                                rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                runningTotal += Constants.SIZE_OF_INT;
                                parseLength += Constants.SIZE_OF_INT;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLDateTime)
                            {
                                rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_DATETIME));

                                runningTotal += Constants.SIZE_OF_DATETIME;
                                parseLength += Constants.SIZE_OF_DATETIME;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLDecimal)
                            {
                                rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_DECIMAL));

                                runningTotal += Constants.SIZE_OF_DECIMAL;
                                parseLength += Constants.SIZE_OF_DECIMAL;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLBit)
                            {
                                rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_BOOL));

                                runningTotal += Constants.SIZE_OF_BOOL;
                                parseLength += Constants.SIZE_OF_BOOL;

                                rowValue.ParseValueLength = parseLength;
                            }
                        }
                        else // nullable == false, fixedBinaryLength == false
                        {
                            if (column.DataType is SQLVarChar)
                            {
                                uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                if (column.Length > 0)
                                {
                                    if (length > column.Length)
                                    {
                                        throw new InvalidOperationException($"Invalid byteparse; {length.ToString()} is longer than the column max length " +
                                            $"of {column.Length.ToString()} for column {column.Name} in table {schema.Name}");
                                    }
                                }

                                runningTotal += Constants.SIZE_OF_INT;
                                parseLength += Constants.SIZE_OF_INT;

                                string value = DbBinaryConvert.BinaryToString(span.Slice((int)runningTotal, (int)length));
                                rowValue.SetValue(value);

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLChar)
                            {
                                uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                if (column.Length > 0)
                                {
                                    if (length > column.Length)
                                    {
                                        throw new InvalidOperationException($"Invalid byteparse; {length.ToString()} is longer than the required length " +
                                            $"of {column.Length.ToString()} for column {column.Name} in table {schema.Name}");
                                    }
                                }

                                runningTotal += Constants.SIZE_OF_INT;
                                parseLength += Constants.SIZE_OF_INT;

                                string value = DbBinaryConvert.BinaryToString(span.Slice((int)runningTotal, (int)length));
                                rowValue.SetValue(value);

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLBinary)
                            {
                                uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                if (column.Length > 0)
                                {
                                    if (length > column.Length)
                                    {
                                        throw new InvalidOperationException($"Invalid byteparse; {length.ToString()} is longer than the required length " +
                                            $"of {column.Length.ToString()} for column {column.Name} in table {schema.Name}");
                                    }
                                }

                                runningTotal += Constants.SIZE_OF_INT;
                                parseLength += Constants.SIZE_OF_INT;

                                rowValue.SetValue(span.Slice((int)runningTotal, (int)length).ToArray());

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLVarbinary)
                            {
                                uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                if (column.Length > 0)
                                {
                                    if (length > column.Length)
                                    {
                                        throw new InvalidOperationException($"Invalid byteparse; {length.ToString()} is longer than the max length " +
                                            $"of {column.Length.ToString()} for column {column.Name} in table {schema.Name}");
                                    }
                                }

                                runningTotal += Constants.SIZE_OF_INT;
                                parseLength += Constants.SIZE_OF_INT;

                                rowValue.SetValue(span.Slice((int)runningTotal, (int)length).ToArray());

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }
                        }
                    }
                    else // is nullable column
                    {
                        bool isNull = DbBinaryConvert.BinaryToBoolean(span.Slice((int)runningTotal, Constants.SIZE_OF_BOOL));

                        if (isNull)
                        {
                            rowValue.SetValueAsNull();

                            runningTotal += Constants.SIZE_OF_BOOL;
                            parseLength += Constants.SIZE_OF_BOOL;

                            rowValue.ParseValueLength = parseLength;
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
                                    rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLDateTime)
                                {
                                    rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_DATETIME + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_DATETIME;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_DATETIME;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLDecimal)
                                {
                                    rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_DECIMAL + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_DECIMAL;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_DECIMAL;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLBit)
                                {
                                    rowValue.SetValue(span.Slice((int)runningTotal, Constants.SIZE_OF_BOOL + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
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
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    string value = DbBinaryConvert.BinaryToString(span.Slice((int)runningTotal, (int)length));
                                    rowValue.SetValue(value);

                                    runningTotal += length;
                                    parseLength += length;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLChar)
                                {
                                    // it's not null, but we need to account for it, so fast forward by the size of the "IsNull" value
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    string value = DbBinaryConvert.BinaryToString(span.Slice((int)runningTotal, (int)length));
                                    rowValue.SetValue(value);

                                    runningTotal += length;
                                    parseLength += length;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLBinary)
                                {
                                    // for binary types, if we're not already NULL (we read this earlier) then we have the size prefix and the byte array
                                    // so go ahead and fast forward by the IsNull value
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    rowValue.SetValue(span.Slice((int)runningTotal, (int)length).ToArray());

                                    runningTotal += length;
                                    parseLength += length;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLVarbinary)
                                {
                                    // for binary types, if we're not already NULL (we read this earlier) then we have the size prefix and the byte array
                                    // so go ahead and fast forward by the IsNull value

                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    uint length = DbBinaryConvert.BinaryToUInt(span.Slice((int)runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    // troubleshoot issue with varbinary not having leadning IsNull value (which is false, but for parsing
                                    // purposes we need it included
                                    uint tempSliceIncludeLeadingNull = runningTotal - Constants.SIZE_OF_BOOL;
                                    uint tempLength = length + Constants.SIZE_OF_BOOL;

                                    //rowValue.SetValue(span.Slice(runningTotal, length).ToArray());
                                    rowValue.SetValue(span.Slice((int)tempSliceIncludeLeadingNull, (int)tempLength).ToArray());

                                    runningTotal += length;
                                    parseLength += length;

                                    rowValue.ParseValueLength = parseLength;
                                }
                            }
                        }
                    }

                    values.Add(rowValue);
                }

                Values = values.ToArray();
                SetSizes();
            }
        }

        /// <summary>
        /// Sets a value for a specific column in the row
        /// </summary>
        /// <param name="columnName">The column to set the value</param>
        /// <param name="value">The value</param>
        public void SetValue(string columnName, string value)
        {
            var rowValue = GetRowValueWithColumnName(columnName);
            if (rowValue is not null)
            {
                rowValue.SetValue(value, true);
            }

            SetSizes();
        }

        /// <summary>
        /// Sets a value for a specific column in the row
        /// </summary>
        /// <param name="columnName">The column to set the value</param>
        /// <param name="value">The value in byte array format</param>
        /// <remarks>WARNING: If setting the byte value directly, if column is NULLABLE, this will bypass setting the nullable not null prefix.
        /// Make sure if you want the binary array to include the leading NOT NULL bool value prefix, to include it.</remarks>
        public void SetValue(string columnName, byte[] value)
        {
            var rowValue = GetRowValueWithColumnName(columnName);
            if (rowValue is not null)
            {
                // warning check
                if (rowValue.Column.IsNullable)
                {
                    if (rowValue.Column.Length == value.Length)
                    {
                        throw new InvalidOperationException("You have forgotten to set the leading FALSE IsNull byte");
                    }
                }

                rowValue.SetValue(value);
            }

            SetSizes();
        }

        public void SetValueAsNullForColumn(string columnName)
        {
            var rowValue = GetRowValueWithColumnName(columnName);
            if (rowValue is not null)
            {
                rowValue.SetValueAsNull();
            }

            SetSizes();
        }

        public void SortBinaryOrder()
        {
            Values = Values.ToList().OrderBy(v => !v.Column.IsFixedBinaryLength).ThenBy(v => v.Column.Ordinal).ToArray();
        }

        public void SortOrdinalOrder()
        {
            Values = Values.ToList().OrderBy(v => v.Column.Ordinal).ToArray();
        }
        #endregion

        #region Private Methods
        private byte[] GetRowInBinaryFormat()
        {
            byte[] data = GetRowDataInBinary();
            _preamble.Type = Type;
            _preamble.RowRemotableSize = 0;
            _preamble.RowValueSize = (uint)data.Length;

            _preamble.RowTotalSize =
                (uint)data.Length +
                (uint)RowConstants.Preamble.Length();

            byte[] preamble = _preamble.ToBinaryFormat();

            var result = new byte[RowConstants.Preamble.Length() + data.Length];

            Array.Copy(preamble, result, preamble.Length);
            Array.Copy(data, 0, result, RowConstants.Preamble.Length(), data.Length);

            return result;
        }

        private byte[] GetRowDataInBinary()
        {
            SortBinaryOrder();
            List<byte[]> arrays = new List<byte[]>();

            foreach (var value in Values)
            {
                DebugValue(value as RowValue);
                var bytes = value.GetValueInBinary();
                arrays.Add(bytes);
            }

            return DbBinaryConvert.ArrayStitch(arrays);
        }

        private IRowValue GetRowValueWithColumnName(string columnName)
        {
            foreach (var value in Values)
            {
                if (string.Equals(columnName, value.Column.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null;
        }

        private void SetTotalSize()
        {
            _preamble.RowTotalSize = 
                (uint)_preamble.ToBinaryFormat().Length + 
                _preamble.RowRemotableSize + 
                _preamble.RowValueSize;
        }

        private void SetRemotableSize()
        {
            _preamble.RowRemotableSize = 0;
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

        [Conditional("DEBUG")]
        private void DebugValue(RowValue value)
        {
            bool debug = false;
            if (debug)
            {
                Debug.WriteLine("RowValue:");
                if (!value.IsNull())
                {
                    Debug.WriteLine(value.GetValueInString());
                }
                else
                {
                    Debug.WriteLine("NULL");
                }
            }
            
        }

        private void DebugByteArray(byte[] array)
        {
            bool debug = false;
            if (debug)
            {
                Debug.WriteLine("DebugByteArray:");
                string callingMethod = new StackFrame(1, true).GetMethod().Name;
                Debug.WriteLine($"Called by {callingMethod}");
                Debug.WriteLine($"Bytes: {BitConverter.ToString(array)}");
            }
        }
        #endregion
    }
}
