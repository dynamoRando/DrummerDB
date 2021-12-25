using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using Drummersoft.DrummerDB.Core.Structures.SQLType.Interface;
using System;
using System.Diagnostics;

namespace Drummersoft.DrummerDB.Core.Structures
{
    internal class RowValue : IRowValue
    {
        #region Private Fields
        private byte[] _value;
        private bool _isNull = false;
        #endregion

        #region Public Properties
        public ColumnSchema Column { get; set; }

        /// <summary>
        /// Indicates how far the byte value starts in the row (minus the preamble)
        /// </summary>
        public int ParseValueLength { get; set; }
        #endregion

        #region Constructors
        public RowValue()
        {
            // default
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowValue"/> class and sets its values.
        /// </summary>
        /// <param name="column">The column of the value</param>
        /// <param name="value">The value in binary format</param>
        public RowValue(ColumnSchema column, ReadOnlySpan<byte> value)
        {
            SetColumn(column);
            SetValue(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowValue"/> class and sets its values.
        /// </summary>
        /// <param name="column">The column of the value.</param>
        /// <param name="value">The value in string format.</param>
        /// <param name="padIfNeeded">if set to <c>true</c> it will [pad if needed]. This is for fixed text length columns.</param>
        public RowValue(ColumnSchema column, string value, bool padIfNeeded = false)
        {
            SetColumn(column);
            SetValue(value, padIfNeeded);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowValue"/> class and sets its value to NULL.
        /// </summary>
        /// <param name="column">The column of the value.</param>
        /// <remarks>This constructor will fail if the column schema is not NULLABLE</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the column is not NULLABLE</exception>
        public RowValue(ColumnSchema column)
        {
            SetColumn(column);
            SetValueAsNull();
        }
        #endregion

        #region Public Methods
        public string DebugValue()
        {
            if (_value is not null)
            {
                return BitConverter.ToString(_value);
            }

            return string.Empty;
        }

        public void SetValueAsNull()
        {
            if (Column.IsNullable)
            {
                _isNull = true;
                _value = DbBinaryConvert.BooleanToBinary(_isNull);
            }
            else
            {
                throw new InvalidOperationException($"You may not set a value as NULL on Non-Nullable Column {Column.Name}");
            }
        }

        /// <summary>
        /// Reads the first byte of the value set and determines if it is set to TRUE/FALSE.
        /// </summary>
        /// <returns><c>TRUE</c> if the value has been marked as NULL, otherwise <c>FALSE</c></returns>
        public bool IsNull()
        {
            return _isNull;
        }

        /// <summary>
        /// Sets the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <exception cref="NullReferenceException">Thrown if the column passed in is NULL.</exception>
        public void SetColumn(ColumnSchema column)
        {
            if (column is not null)
            {
                Column = column;
            }
            else
            {
                throw new NullReferenceException();
            }

        }
        /// <summary>
        /// Returns the value represented as a string
        /// </summary>
        /// <returns>The value in string format</returns>
        /// <exception cref="InvalidOperationException">Thrown if the value is NULL. You should call <see cref="IsNull"/> first if the column is NULLABLE to see if the value is NULL.</exception>
        public string GetValueInString()
        {
            if (Column.IsNullable)
            {
                if (IsNull())
                {
                    throw new InvalidOperationException($"The value is NULL for column {Column.Name}");
                }
                else
                {
                    var span = new ReadOnlySpan<byte>(_value);
                    var data = span.Slice(Constants.SIZE_OF_BOOL, span.Length - Constants.SIZE_OF_BOOL).ToArray();
                    if (Column.IsFixedBinaryLength)
                    {
                        if (Column.DataType is SQLInt)
                        {
                            int value = DbBinaryConvert.BinaryToInt(data);
                            return value.ToString();
                        }

                        if (Column.DataType is SQLDateTime)
                        {
                            DateTime value = DbBinaryConvert.BinaryToDateTime(data);
                            return value.ToString();
                        }

                        if (Column.DataType is SQLDecimal)
                        {
                            decimal value = DbBinaryConvert.BinaryToDecimal(data);
                            return value.ToString();
                        }

                        if (Column.DataType is SQLBit)
                        {
                            bool value = DbBinaryConvert.BinaryToBoolean(data);
                            return value.ToString();
                        }
                    }
                    else
                    {
                        if (Column.DataType is SQLVarChar)
                        {
                            string value = DbBinaryConvert.BinaryToString(data);
                            return value.ToString();
                        }

                        if (Column.DataType is SQLChar)
                        {
                            string value = DbBinaryConvert.BinaryToString(data);
                            return value.ToString();
                        }
                    }
                }
            }
            else
            {
                if (Column.IsFixedBinaryLength)
                {
                    if (Column.DataType is SQLInt)
                    {
                        int value = DbBinaryConvert.BinaryToInt(_value);
                        return value.ToString();
                    }

                    if (Column.DataType is SQLDateTime)
                    {
                        DateTime value = DbBinaryConvert.BinaryToDateTime(_value);
                        return value.ToString();
                    }

                    if (Column.DataType is SQLDecimal)
                    {
                        decimal value = DbBinaryConvert.BinaryToDecimal(_value);
                        return value.ToString();
                    }

                    if (Column.DataType is SQLBit)
                    {
                        bool value = DbBinaryConvert.BinaryToBoolean(_value);
                        return value.ToString();
                    }
                }
                else
                {
                    if (Column.DataType is SQLVarChar)
                    {
                        string value = DbBinaryConvert.BinaryToString(_value);
                        return value.ToString();
                    }

                    if (Column.DataType is SQLChar)
                    {
                        string value = DbBinaryConvert.BinaryToString(_value);
                        return value.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Attempts to set the value based on the passed in byte array.
        /// </summary>
        /// <param name="span">The byte representation of the value. Include the leading INT length prefix if the column width is variable. Include leading BOOL prefix if the column is NULLABLE.</param>
        /// <remarks>If the column type is not fixed, this function assumes that the byte arrary argument includes the leading INT length prefix. If the column is NULLABLE, this function assumes that the first byte is 
        /// the leading BOOL prefix indicating if the value is NULL. For more information, see Row.md</remarks>
        public void SetValue(ReadOnlySpan<byte> span)
        {
            if (!Column.IsNullable)
            {
                if (Column.IsFixedBinaryLength)
                {
                    _value = span.ToArray();
                }
                else
                {
                    if (Column.DataType is SQLVarChar)
                    {
                        int length = DbBinaryConvert.BinaryToInt(span.Slice(0, Constants.SIZE_OF_INT));
                        string value = DbBinaryConvert.BinaryToString(span.Slice(Constants.SIZE_OF_INT, length));
                        SetValue(value);
                    }

                    if (Column.DataType is SQLChar)
                    {
                        int length = DbBinaryConvert.BinaryToInt(span.Slice(0, Constants.SIZE_OF_INT));
                        string value = DbBinaryConvert.BinaryToString(span.Slice(Constants.SIZE_OF_INT, length));
                        SetValue(value);
                    }

                    if (Column.DataType is SQLBinary || Column.DataType is SQLVarbinary)
                    {
                        int length = DbBinaryConvert.BinaryToInt(span.Slice(0, Constants.SIZE_OF_INT));
                        SetValue(span.Slice(Constants.SIZE_OF_INT, length));
                    }
                }
            }
            else // column is nullable
            {
                // the first byte is a BOOL, we need to check to see if it's been marked as NULL
                _isNull = DbBinaryConvert.BinaryToBoolean(span.Slice(0, Constants.SIZE_OF_BOOL));

                if (_isNull)
                {
                    return;
                }
                else
                {
                    if (Column.IsFixedBinaryLength)
                    {
                        _value = span.ToArray(); // set the value with the leading bool prefix
                    }
                    else // nullable non fixed binary type
                    {
                        if (Column.DataType is SQLVarChar)
                        {
                            int length = DbBinaryConvert.BinaryToInt(span.Slice(Constants.SIZE_OF_BOOL, Constants.SIZE_OF_INT));
                            string value = DbBinaryConvert.BinaryToString(span.Slice(Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL, length));
                            SetValue(value);
                        }

                        if (Column.DataType is SQLChar)
                        {
                            int length = DbBinaryConvert.BinaryToInt(span.Slice(Constants.SIZE_OF_BOOL, Constants.SIZE_OF_INT));
                            string value = DbBinaryConvert.BinaryToString(span.Slice(Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL, length));
                            SetValue(value);
                        }

                        if (Column.DataType is SQLBinary || Column.DataType is SQLVarbinary)
                        {
                            int length = DbBinaryConvert.BinaryToInt(span.Slice(Constants.SIZE_OF_BOOL, Constants.SIZE_OF_INT));
                            _value = span.Slice(Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL, length).ToArray();
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Sets the underlying value's byte array directly
        /// </summary>
        /// <param name="value">The byte array representing the value</param>
        /// <remarks>Useful for setting columns that are of <seealso cref="SQLBinary"/> or <seealso cref="SQLVarbinary"/> type. Use 
        /// instead of passing in a string representation of the value. </remarks>.
        public void SetValue(byte[] value)
        {
            _value = value;
            _isNull = false;
        }

        /// <summary>
        /// Attempts to set the value based on the passed in string representation. It will validate if the value is appropriate for the columns data type before setting value.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="padIfNeeded">Pads the value passed in if needed</param>
        /// <remarks>Do not call this function if you are trying to set NULL. Use <see cref="SetValueAsNull"/> instead.</remarks>
        public void SetValue(string value, bool padIfNeeded = false)
        {
            _isNull = false;

            switch (Column.DataType)
            {
                case SQLVarChar varchar:
                    if (IsValidLengthVarChar(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.StringToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            _value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, _value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                            string debugFinalValue = BitConverter.ToString(_value);
                            Debug.WriteLine(debugFinalValue);

                        }
                        else
                        {
                            _value = DbBinaryConvert.StringToBinary(value);
                        }

                    }
                    break;
                case SQLChar item:
                    if (padIfNeeded)
                    {
                        if (value.Length < Column.Length)
                        {
                            value = value.PadRight(Column.Length);
                        }
                    }

                    if (IsValidLengthChar(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.StringToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            _value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, _value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(_value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            _value = DbBinaryConvert.StringToBinary(value);
                        }

                    }
                    break;
                case SQLInt foo:
                    if (IsValidInt(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.IntToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            _value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, _value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(_value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            _value = DbBinaryConvert.IntToBinary(value);
                        }

                    }
                    break;
                case SQLDateTime a:
                    if (IsValidDateTime(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.DateTimeToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            _value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, _value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(_value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            _value = DbBinaryConvert.DateTimeToBinary(value);
                        }

                    }
                    break;
                case SQLBit b:
                    if (IsValidBoolean(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.BooleanToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            _value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, _value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(_value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            _value = DbBinaryConvert.BooleanToBinary(value);
                        }
                    }
                    break;
                case SQLDecimal c:
                    if (IsValidDecimal(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.DecimalToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            _value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, _value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(_value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            _value = DbBinaryConvert.DecimalToBinary(value);
                        }
                    }
                    break;
                case SQLVarbinary d:
                    if (Column.IsNullable)
                    {
                        byte[] convertedValue = DbBinaryConvert.StringToBinary(value);
                        byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                        string debugConvertedValue = BitConverter.ToString(convertedValue);
                        Debug.WriteLine(debugConvertedValue);

                        _value = new byte[convertedValue.Length + isNull.Length];
                        Array.Copy(isNull, 0, _value, 0, isNull.Length);
                        Array.Copy(convertedValue, 0, _value, isNull.Length, convertedValue.Length);

                        string debugValue = BitConverter.ToString(_value);
                        Debug.WriteLine(debugValue);
                    }
                    else
                    {
                        _value = DbBinaryConvert.StringToBinary(value);
                    }
                    break;
                default:
                    throw new UnknownSQLTypeException($"{Column.DataType.GetType().ToString()} is unknown");
            }
        }

        /// <summary>
        /// Returns the binary array size of the value
        /// </summary>
        /// <returns>the binary array size of the value</returns>
        public int BinarySize()
        {
            if (Column.IsFixedBinaryLength)
            {
                if (!Column.IsNullable)
                {
                    return (Column.DataType as IFixedSQLType).Size();
                }
                else
                {
                    return (Column.DataType as IFixedSQLType).Size() + Constants.SIZE_OF_BOOL;
                }
            }
            else
            {
                return _value.Length;
            }
        }

        /// <summary>
        /// Returns a ReadOnlySpan of the value
        /// </summary>
        /// <param name="includeSizeInBinaryIfNotFixed">Specify true if you want non-fixed values to have an int sized prefix of the length. Usually <c>TRUE</c> if you
        /// are trying to save to a <seealso cref="IBaseDataPage"/>'s data. Otherwise usually <c>FALSE</c> if you just want the value itself.</param>
        /// <returns>A ReadOnlySpan of the the value</returns>
        /// <remarks>The non-fixed length value types (char/varchar) will need a prefix of the size (also in binary)</remarks>
        public ReadOnlySpan<byte> GetValueInByteSpan(bool includeSizeInBinaryIfNotFixed = true, bool includeNullablePrefix = true)
        {

            if (!Column.IsNullable)
            {
                if (Column.IsFixedBinaryLength)
                {
                    switch (Column.DataType)
                    {
                        case SQLInt sqlInt:
                        case SQLBit bit:
                        case SQLDateTime dt:
                        case SQLDecimal dec:
                            return new ReadOnlySpan<byte>(_value);
                        default:
                            throw new InvalidOperationException("Unknown fixed binary length SQL type");
                    }
                }
                else
                {
                    if (Column.DataType is SQLVarChar || Column.DataType is SQLChar
                        || Column.DataType is SQLVarbinary || Column.DataType is SQLBinary)
                    {
                        if (includeSizeInBinaryIfNotFixed)
                        {
                            byte[] lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                            byte[] resultArray = new byte[lengthArray.Length + _value.Length];

                            Array.Copy(lengthArray, resultArray, lengthArray.Length);
                            Array.Copy(_value, 0, resultArray, lengthArray.Length, _value.Length);

                            return new ReadOnlySpan<byte>(resultArray);
                        }
                        else
                        {
                            return new ReadOnlySpan<byte>(_value);
                        }
                    }
                }
            }
            else // column is nullable
            {
                if (_isNull)
                {
                    return DbBinaryConvert.BooleanToBinary(true).AsSpan();
                }
                else
                {
                    var isNull = DbBinaryConvert.BooleanToBinary(_isNull);

                    if (Column.IsFixedBinaryLength)
                    {
                        if (includeNullablePrefix)
                        {
                            switch (Column.DataType)
                            {
                                case SQLInt sqlInt:
                                case SQLBit bit:
                                case SQLDateTime dt:
                                case SQLDecimal dec:
                                    var newArray = new byte[_value.Length + Constants.SIZE_OF_BOOL];
                                    Array.Copy(isNull, 0, newArray, 0, isNull.Length);
                                    Array.Copy(_value, 0, newArray, Constants.SIZE_OF_BOOL, _value.Length);

                                    return new ReadOnlySpan<byte>(newArray);
                                default:
                                    throw new InvalidOperationException("Unknown fixed binary length SQL type");
                            }
                        }
                        else
                        {
                            switch (Column.DataType)
                            {
                                case SQLInt sqlInt:
                                case SQLBit bit:
                                case SQLDateTime dt:
                                case SQLDecimal dec:
                                    return new ReadOnlySpan<byte>(_value);
                                default:
                                    throw new InvalidOperationException("Unknown fixed binary length SQL type");
                            }
                        }
                    }
                    else
                    {
                        if (Column.DataType is SQLVarChar || Column.DataType is SQLChar
                            || Column.DataType is SQLVarbinary || Column.DataType is SQLBinary)
                        {
                            if (includeSizeInBinaryIfNotFixed && includeNullablePrefix)
                            {
                                byte[] lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                                byte[] resultArray = new byte[lengthArray.Length + _value.Length + Constants.SIZE_OF_BOOL];

                                Array.Copy(isNull, 0, resultArray, 0, isNull.Length);
                                Array.Copy(lengthArray, 0, resultArray, Constants.SIZE_OF_BOOL, lengthArray.Length);
                                Array.Copy(_value, 0, resultArray, lengthArray.Length, _value.Length);

                                return new ReadOnlySpan<byte>(resultArray);
                            }
                            else if (includeSizeInBinaryIfNotFixed)
                            {
                                byte[] lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                                byte[] resultArray = new byte[lengthArray.Length + _value.Length];

                                Array.Copy(lengthArray, resultArray, lengthArray.Length);
                                Array.Copy(_value, 0, resultArray, lengthArray.Length, _value.Length);

                                return new ReadOnlySpan<byte>(resultArray);
                            }
                            else
                            {
                                return new ReadOnlySpan<byte>(_value);
                            }
                        }
                    }
                }

            }

            return null;
        }

        /// <summary>
        /// Returns the value in a binary format
        /// </summary>
        /// <param name="includeSizeInBinaryIfNotFixed">Specify true if you want non-fixed values to have an int sized prefix of the length</param>
        /// <returns>The value in binary format</returns>
        /// <remarks>Use this function when saving to a <seealso cref="IBaseDataPage"/>'s data. The non-fixed length value types (char/varchar) will need a prefix of the size (also in binary). If the column is NULLABLE, it will include the BOOL prefix.
        /// Usually <c>TRUE</c> if you are trying to save to a <seealso cref="IBaseDataPage"/>'s data. 
        /// Otherwise usually <c>FALSE</c> if you just want the value itself.</remarks>
        public byte[] GetValueInBinary(bool includeSizeInBinaryIfNotFixed = true, bool includeNullablePrefix = true)
        {
            if (!Column.IsNullable)
            {
                if (Column.IsFixedBinaryLength)
                {
                    switch (Column.DataType)
                    {
                        case SQLInt sqlInt:
                        case SQLBit bit:
                        case SQLDateTime dt:
                        case SQLDecimal dec:
                            return _value;
                        default:
                            throw new InvalidOperationException("Unknown fixed binary length SQL type");
                    }
                }
                else
                {
                    if (Column.DataType is SQLVarChar || Column.DataType is SQLChar
                        || Column.DataType is SQLVarbinary || Column.DataType is SQLBinary)
                    {
                        if (includeSizeInBinaryIfNotFixed)
                        {
                            byte[] lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                            byte[] resultArray = new byte[lengthArray.Length + _value.Length];

                            Array.Copy(lengthArray, resultArray, lengthArray.Length);
                            Array.Copy(_value, 0, resultArray, lengthArray.Length, _value.Length);

                            return resultArray;
                        }
                        else
                        {
                            return _value;
                        }
                    }
                }
            }
            else // column is nullable
            {
                if (_isNull)
                {
                    return DbBinaryConvert.BooleanToBinary(_isNull);
                }
                else
                {
                    if (Column.IsFixedBinaryLength)
                    {
                        if (includeNullablePrefix)
                        {
                            switch (Column.DataType)
                            {
                                case SQLInt sqlInt:
                                case SQLBit bit:
                                case SQLDateTime dt:
                                case SQLDecimal dec:

                                    string debugValue = BitConverter.ToString(_value);
                                    Debug.WriteLine(debugValue);

                                    return _value; // value should already include the nullable boolean prefix 
                                default:
                                    throw new InvalidOperationException("Unknown fixed binary length SQL type");
                            }
                        }
                        else
                        {
                            switch (Column.DataType)
                            {
                                case SQLInt sqlInt:
                                case SQLBit bit:
                                case SQLDateTime dt:
                                case SQLDecimal dec:
                                    var span = new ReadOnlySpan<byte>(_value);
                                    return span.Slice(Constants.SIZE_OF_BOOL, span.Length - Constants.SIZE_OF_BOOL).ToArray();
                                default:
                                    throw new InvalidOperationException("Unknown fixed binary length SQL type");
                            }
                        }
                    }
                    else // is variable binary length and nullable
                    {
                        if (Column.DataType is SQLVarChar || Column.DataType is SQLChar
                            || Column.DataType is SQLVarbinary || Column.DataType is SQLBinary)
                        {
                            if (includeSizeInBinaryIfNotFixed && includeNullablePrefix)
                            {
                                /*
                                 * _value here has the leading isNull prefix (1 byte) and then the actual value. We need to deconstruct and reconstruct
                                 * to have the following format: IsNull + Length Of Field + Field Data
                                 */

                                byte[] isNull = DbBinaryConvert.BooleanToBinary(_isNull);
                                byte[] lengthArray = DbBinaryConvert.IntToBinary(_value.Length - Constants.SIZE_OF_BOOL); // back out the leading isNull flag
                                byte[] resultArray = new byte[lengthArray.Length + _value.Length];

                                string debugIsNullArray = BitConverter.ToString(isNull);
                                Debug.WriteLine(debugIsNullArray);

                                string debugLengthArray = BitConverter.ToString(lengthArray);
                                Debug.WriteLine(debugLengthArray);

                                Array.Copy(isNull, 0, resultArray, 0, isNull.Length);
                                Array.Copy(lengthArray, 0, resultArray, Constants.SIZE_OF_BOOL, lengthArray.Length);
                                Array.Copy(_value, Constants.SIZE_OF_BOOL, resultArray, Constants.SIZE_OF_BOOL + lengthArray.Length, _value.Length - Constants.SIZE_OF_BOOL);

                                string debugArray = BitConverter.ToString(resultArray);
                                Debug.WriteLine(debugArray);

                                return resultArray;
                            }
                            else if (includeSizeInBinaryIfNotFixed)
                            {
                                byte[] lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                                byte[] resultArray = new byte[lengthArray.Length + _value.Length];
                                Array.Copy(lengthArray, resultArray, lengthArray.Length);
                                Array.Copy(_value, 0, resultArray, lengthArray.Length, _value.Length);

                                return resultArray;
                            }
                            else
                            {
                                return _value;
                            }
                        }
                    }
                }
            }


            return null;
        }

        #endregion

        #region Private Methods
        private bool IsValidDecimal(string value)
        {
            decimal i;
            if (decimal.TryParse(value, out i))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert {value} to datatype DECIMAL for {Column.Name}");
            }
        }
        private bool IsValidBoolean(string value)
        {
            bool i;
            if (bool.TryParse(value, out i))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert {value} to datatype BIT for {Column.Name}");
            }
        }
        private bool IsValidDateTime(string value)
        {
            DateTime i;
            if (DateTime.TryParse(value, out i))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert {value} to datatype DATETIME for {Column.Name}");
            }
        }
        private bool IsValidInt(string value)
        {
            int i;
            if (int.TryParse(value, out i))
            {
                return true;
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert {value} to datatype INT for {Column.Name}");
            }
        }
        private bool IsValidLengthVarChar(string value)
        {
            var dataType = Column.DataType as SQLVarChar;
            if (value.Length > dataType.MaxLength)
            {
                throw new InvalidOperationException($"The value length: {value.Length.ToString()} is greater than the column's specified length {dataType.MaxLength.ToString()} for {Column.Name}");
            }
            else
            {
                return true;
            }
        }

        private bool IsValidLengthChar(string value)
        {
            var dataType = Column.DataType as SQLChar;
            if (value.Length != dataType.Length)
            {
                throw new InvalidOperationException($"The value length: {value.Length.ToString()} is not equal than the column's specified length {dataType.Length.ToString()} for {Column.Name}");
            }
            else
            {
                return true;
            }
        }
        #endregion

    }
}
