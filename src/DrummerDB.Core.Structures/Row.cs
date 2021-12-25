using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.Version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class Row : IRow
    {
        private Participant _participant;

        /*
         * Row Byte Array Layout:
         * RowId IsLocal IsDeleted IsForwarded ForwardOffset ForwardedPageId {{SizeOfRow | ParticipantId} | RowData}
         * RowId IsLocal IsDeleted IsForwarded ForwardOffset ForwardedPageId - preamble (used in inital load of the Row)
         * 
         * if IsLocal == true, then need to request the rest of the byte array
         * 
         * if IsLocal == false, then need to request the rest of the byte array, i.e. the size of the ParticipantId
         * 
         * SizeOfRow is the size of the rest of the row in bytes minus the preamble.  It includes the int32 byte size value itself.
         * For a remote row, this is just the size of the ParticipantId (a guid)
         * For a local row, this is the total size of all the data
         * 
         * If IsLocal == true, format is as follows -
         * [data_col1] [data_col2] [data_colX] - fixed size columns first
         * [SizeOfVar] [varData] [SizeOfVar] [varData] - variable size columns
         * [ -1 preamble] - signals the end of row data (a preamble whose RowId == -1 and IsLocal == true)
         */

        #region Public Properties
        public int Id { get; set; }
        public bool IsLocal { get; set; }
        public Guid? ParticipantId { get; set; }
        public IRowValue[] Values { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsForwarded { get; set; }
        public int ForwardOffset { get; set; }
        public int ForwardedPageId { get; set; }
        public Participant Participant => _participant;
        public byte[] Hash { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a new instance of a row for this table
        /// </summary>
        /// <param name="id">The row id</param>
        /// <param name="isLocal">If the row is local to the host database</param>
        /// <param name="participantId">The participant who owns this row, if applicable. This defaults to null.</param>
        public Row(int id, bool isLocal, Guid? participantId = null)
        {
            Id = id;
            IsLocal = isLocal;
            IsDeleted = false;
            IsForwarded = false;
            ForwardOffset = 0;
            ForwardedPageId = 0;
        }

        public Row(int id, bool isLocal, Participant participant)
        {
            Id = id;
            IsLocal = isLocal;
            IsDeleted = false;
            IsForwarded = false;
            ForwardOffset = 0;
            ForwardedPageId = 0;
            _participant = participant;
            ParticipantId = participant.Id;
        }

        /// <summary>
        /// Constructs an instance of a Row based off of the preamble byte array
        /// </summary>
        /// <param name="preamble">The preamble byte array</param>
        public Row(ReadOnlySpan<byte> preamble)
        {
            checked
            {
                Id = DbBinaryConvert.BinaryToInt(preamble.Slice(RowConstants.RowIdOffset(), RowConstants.SIZE_OF_ROW_ID));
                IsLocal = DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.IsLocalOffset(), RowConstants.SIZE_OF_IS_LOCAL));
                IsDeleted = DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.IsDeletedOffset(), RowConstants.SIZE_OF_IS_DELETED));
                IsForwarded = DbBinaryConvert.BinaryToBoolean(preamble.Slice(RowConstants.IsForwardedOffset(), RowConstants.SIZE_OF_IS_FORWARDED));
                ForwardOffset = DbBinaryConvert.BinaryToInt(preamble.Slice(RowConstants.ForwardOffset(), RowConstants.SIZE_OF_FORWARD_OFFSET));
                ForwardedPageId = DbBinaryConvert.BinaryToInt(preamble.Slice(RowConstants.ForwardedPageIdOffset(), RowConstants.SIZE_OF_FORWARDED_PAGE_ID));
            }
        }
        #endregion

        #region Public Methods
        public byte[] GetRowInTransactionBinaryFormat()
        {
            return GetRowInPageBinaryFormat();
        }

        /// <summary>
        /// Helper method to get the size of the row based on the supplied byte array. The returned row size includes the preamble + data + the size measurement of the row (INT) itself.
        /// </summary>
        /// <param name="data">A readonly span of data representing the size of the row</param>
        /// <returns>The size of the row</returns>
        public static int GetRowSizeFromBinary(ReadOnlySpan<byte> data)
        {
            return DbBinaryConvert.BinaryToInt(data);
        }

        /// <summary>
        /// Attempts to populate a row's data based on the supplied byte span. 
        /// </summary>
        /// <param name="schema">The schema of the table the row belongs to</param>
        /// <param name="span">The byte array span (usually from the Page's data)</param>
        /// <remarks>This function assumes in the Span an <seealso cref="int"/> prefix indicating the length of the value if it is
        /// a variable binary length type (<seealso cref="SQLChar"/>, <seealso cref="SQLVarChar"/>,
        /// <seealso cref="SQLBinary"/>,<seealso cref="SQLVarbinary"/>, etc.)</remarks>
        public void SetRowData(ITableSchema schema, ReadOnlySpan<byte> span)
        {
            var values = new List<RowValue>();
            int runningTotal = 0;

            if (schema is TableSchema)
            {
                schema = schema as TableSchema;
                schema.SortBinaryOrder();
                foreach (var column in schema.Columns)
                {
                    var rowValue = new RowValue();
                    rowValue.Column = column;

                    int parseLength = 0;

                    if (!column.IsNullable) // nullable == false
                    {
                        if (column.IsFixedBinaryLength)
                        {
                            if (column.DataType is SQLInt)
                            {
                                rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_INT));

                                runningTotal += Constants.SIZE_OF_INT;
                                parseLength += Constants.SIZE_OF_INT;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLDateTime)
                            {
                                rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DATETIME));

                                runningTotal += Constants.SIZE_OF_DATETIME;
                                parseLength += Constants.SIZE_OF_DATETIME;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLDecimal)
                            {
                                rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DECIMAL));

                                runningTotal += Constants.SIZE_OF_DECIMAL;
                                parseLength += Constants.SIZE_OF_DECIMAL;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLBit)
                            {
                                rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_BOOL));

                                runningTotal += Constants.SIZE_OF_BOOL;
                                parseLength += Constants.SIZE_OF_BOOL;

                                rowValue.ParseValueLength = parseLength;
                            }
                        }
                        else // nullable == false, fixedBinaryLength == false
                        {
                            if (column.DataType is SQLVarChar)
                            {
                                int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

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

                                string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
                                rowValue.SetValue(value);

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLChar)
                            {
                                int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

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

                                string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
                                rowValue.SetValue(value);

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLBinary)
                            {
                                int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

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

                                rowValue.SetValue(span.Slice(runningTotal, length).ToArray());

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
                            }

                            if (column.DataType is SQLVarbinary)
                            {
                                int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

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

                                rowValue.SetValue(span.Slice(runningTotal, length).ToArray());

                                runningTotal += length;
                                parseLength += length;

                                rowValue.ParseValueLength = parseLength;
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
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLDateTime)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DATETIME + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_DATETIME;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_DATETIME;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLDecimal)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_DECIMAL + Constants.SIZE_OF_BOOL));

                                    runningTotal += Constants.SIZE_OF_DECIMAL;
                                    runningTotal += Constants.SIZE_OF_BOOL;
                                    parseLength += Constants.SIZE_OF_DECIMAL;
                                    parseLength += Constants.SIZE_OF_BOOL;

                                    rowValue.ParseValueLength = parseLength;
                                }

                                if (column.DataType is SQLBit)
                                {
                                    rowValue.SetValue(span.Slice(runningTotal, Constants.SIZE_OF_BOOL + Constants.SIZE_OF_BOOL));

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

                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
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

                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    string value = DbBinaryConvert.BinaryToString(span.Slice(runningTotal, length));
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

                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    rowValue.SetValue(span.Slice(runningTotal, length).ToArray());

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

                                    int length = DbBinaryConvert.BinaryToInt(span.Slice(runningTotal, Constants.SIZE_OF_INT));

                                    runningTotal += Constants.SIZE_OF_INT;
                                    parseLength += Constants.SIZE_OF_INT;

                                    // troubleshoot issue with varbinary not having leadning IsNull value (which is false, but for parsing
                                    // purposes we need it included
                                    int tempSliceIncludeLeadingNull = runningTotal - Constants.SIZE_OF_BOOL;
                                    int tempLength = length + Constants.SIZE_OF_BOOL;

                                    //rowValue.SetValue(span.Slice(runningTotal, length).ToArray());
                                    rowValue.SetValue(span.Slice(tempSliceIncludeLeadingNull, tempLength).ToArray());

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
                GetAndSetRowHash();
            }
        }

        /// <summary>
        /// Returns the size of the row calculated from it's binary values. The size of a row includes the row preamble + the size of the data (including for variable columns the length prefix (INT)) + the size of the row itself (INT)
        /// </summary>
        /// <returns>The complete size of the row in bytes</returns>
        public int Size()
        {
            return GetRowInPageBinaryFormat().Length;
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

        public ReadOnlySpan<byte> GetValueInByteSpan(string columnName)
        {
            return GetRowValueWithColumnName(columnName).GetValueInByteSpan();
        }

        public bool IsValueNull(string columnName)
        {
            var rowValue = GetRowValueWithColumnName(columnName);
            return rowValue.IsNull();
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
        }

        public void SetValueAsNullForColumn(string columnName)
        {
            var rowValue = GetRowValueWithColumnName(columnName);
            if (rowValue is not null)
            {
                rowValue.SetValueAsNull();
            }
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
        }

        /// <summary>
        /// Returns the entire row in binary format, ordered by binary save format (usually to save data to a Page or for sending over the wire to a Participant)
        /// </summary>
        /// <returns>A binary representation of the row</returns>
        /// <remarks>A local row consists of: preamble + sizeOfRow + rowData (ordered by fixed binary columns first, then by variable binary columns, each with an INT size prefix before the actual data for variable size columns.)</remarks>
        public byte[] GetRowInPageBinaryFormat()
        {
            if (IsLocal)
            {
                return GetRowInBinaryFormat();
            }
            else
            {
                // need the participant id and the row hash
                GetAndSetRowHash();

                byte[] preamble = GetPreambleInBinary();
                byte[] bRowHashLength = DbBinaryConvert.IntToBinary(Hash.Length);
                byte[] bParticipantId = DbBinaryConvert.GuidToBinary(Participant.Id);

                var arrays = new List<byte[]>(3);
                arrays.Add(bParticipantId);
                arrays.Add(bRowHashLength);
                arrays.Add(Hash);

                byte[] bRowData = DbBinaryConvert.ArrayStitch(arrays);
                int sizeOfRow = preamble.Length + bRowData.Length + RowConstants.SIZE_OF_ROW_SIZE;
                byte[] bSizeOfRow = DbBinaryConvert.IntToBinary(sizeOfRow);

                var result = new byte[preamble.Length + bRowData.Length + RowConstants.SIZE_OF_ROW_SIZE];

                // format needs to be 
                // participant id
                // length of data hash (int - 4 bytes)
                // data hash

                Array.Copy(preamble, result, preamble.Length);
                Array.Copy(bSizeOfRow, 0, result, RowConstants.SizeOfRowOffset(), bSizeOfRow.Length);
                Array.Copy(bRowData, 0, result, RowConstants.RowDataOffset(), bRowData.Length);

                return result;
            }
        }

        /// <summary>
        /// Sorts the row's value by the column Ordinal value
        /// </summary>
        public void SortOrdinalOrder()
        {
            this.Values = Values.ToList().OrderBy(v => v.Column.Ordinal).ToArray();
        }

        /// <summary>
        /// Sorts the row's values by the binary value - Fixed Length Columns First, then by Ordinal order
        /// </summary>
        public void SortBinaryOrder()
        {
            this.Values = Values.ToList().OrderBy(v => !v.Column.IsFixedBinaryLength).ThenBy(v => v.Column.Ordinal).ToArray();
        }

        /// <summary>
        /// Sets the row to forwarded and records the new offset on the specified page where the row can be found. 
        /// </summary>
        /// <param name="newOffset">The byte offset where the row can now be found</param>
        /// <param name="pageId">The page id where the row was forwarded to</param>
        public void ForwardRow(int newOffset, int pageId)
        {
            IsForwarded = true;
            ForwardOffset = newOffset;
            ForwardedPageId = pageId;
        }
        #endregion

        #region Private Methods
        private byte[] GetRowInBinaryFormat()
        {
            // TODO: need to change this in the future to handle remote rows
            byte[] preamble = GetPreambleInBinary();

            // TODO: in the future, for remote rows, this should only return the particiapnt id, and not the actual values 
            byte[] data = GetRowDataInBinary();
            int sizeOfRow = preamble.Length + data.Length + RowConstants.SIZE_OF_ROW_SIZE;

            byte[] sizeOfRowArray = DbBinaryConvert.IntToBinary(sizeOfRow);

            var result = new byte[preamble.Length + sizeOfRowArray.Length + data.Length];

            Array.Copy(preamble, result, preamble.Length);
            Array.Copy(sizeOfRowArray, 0, result, RowConstants.SizeOfRowOffset(), sizeOfRowArray.Length);
            Array.Copy(data, 0, result, RowConstants.RowDataOffset(), data.Length);

            return result;
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
        private byte[] GetPreambleInBinary()
        {
            var arrays = new List<byte[]>(6);

            arrays.Add(DbBinaryConvert.IntToBinary(Id));
            arrays.Add(DbBinaryConvert.BooleanToBinary(IsLocal.ToString()));
            arrays.Add(DbBinaryConvert.BooleanToBinary(IsDeleted.ToString()));
            arrays.Add(DbBinaryConvert.BooleanToBinary(IsForwarded.ToString()));
            arrays.Add(DbBinaryConvert.IntToBinary(ForwardOffset));
            arrays.Add(DbBinaryConvert.IntToBinary(ForwardedPageId));

            return DbBinaryConvert.ArrayStitch(arrays);
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
        private byte[] GetAndSetRowHash()
        {
            // ideally this code should be in Drummersoft.DrummerDB.Core.Cryptogrpahy
            // but the dependencies wouldn't work (would result in a circular reference)
            // may later change the dependency layout, but for now leaving this here
            // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=net-6.0
            var sourceData = GetRowInBinaryFormat();
            var sha256Hash = SHA256.Create();
            Hash = sha256Hash.ComputeHash(sourceData);
            return Hash;
        }
        #endregion
    }
}
