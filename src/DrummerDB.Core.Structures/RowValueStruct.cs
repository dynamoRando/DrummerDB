using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using System;

namespace Drummersoft.DrummerDB.Core.Structures
{
    // experimental, not used anywhere
    internal readonly ref struct RowValueStruct
    {
        #region Private Fields
        private readonly byte[] _value;
        #endregion

        #region Public Properties
        public readonly ColumnSchemaStruct Column { get; }
        #endregion

        #region Constructors
        public RowValueStruct(ColumnSchemaStruct column, string value, bool padIfNeeded = false)
        {
            Column = column;

            _value = null;

            switch (Column.DataType)
            {
                case SQLColumnType.Varchar:
                    if (IsValidLengthVarChar(value))
                    {
                        _value = DbBinaryConvert.StringToBinary(value);
                    }
                    break;
                case SQLColumnType.Char:
                    if (padIfNeeded)
                    {
                        if (value.Length < Column.Length)
                        {
                            value = value.PadRight(Column.Length);
                        }
                    }

                    if (IsValidLengthChar(value))
                    {
                        _value = DbBinaryConvert.StringToBinary(value);
                    }
                    break;
                case SQLColumnType.Int:
                    if (IsValidInt(value))
                    {
                        _value = DbBinaryConvert.IntToBinary(value);
                    }
                    break;
                case SQLColumnType.DateTime:
                    if (IsValidDateTime(value))
                    {
                        _value = DbBinaryConvert.DateTimeToBinary(value);
                    }
                    break;
                case SQLColumnType.Bit:
                    if (IsValidBoolean(value))
                    {
                        _value = DbBinaryConvert.BooleanToBinary(value);
                    }
                    break;
                case SQLColumnType.Decimal:
                    if (IsValidDecimal(value))
                    {
                        _value = DbBinaryConvert.DecimalToBinary(value);
                    }
                    break;
                default:
                    throw new UnknownSQLTypeException($"{Column.DataType.GetType().ToString()} is unknown");
            }
        }

        public RowValueStruct(ColumnSchemaStruct column, ReadOnlySpan<byte> value, bool padIfNeeded = false)
        {
            Column = column;
            _value = value.ToArray();
        }

        public RowValueStruct(ColumnSchemaStruct column, byte[] value, bool padIfNeeded = false)
        {
            Column = column;
            _value = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a ReadOnlySpan of the value
        /// </summary>
        /// <param name="includeSizeInBinaryIfNotFixed">Specify true if you want non-fixed values to have an int sized prefix of the length. Usually <c>TRUE</c> if you
        /// are trying to save to a <seealso cref="IBaseDataPage"/>'s data. Otherwise usually <c>FALSE</c> if you just want the value itself.</param>
        /// <returns>A ReadOnlySpan of the the value</returns>
        /// <remarks>The non-fixed length value types (char/varchar) will need a prefix of the size (also in binary)</remarks>
        public ReadOnlySpan<byte> GetValueInByteSpan(bool includeSizeInBinaryIfNotFixed = true)
        {
            if (Column.IsFixedBinaryLength())
            {
                switch (Column.DataType)
                {
                    case SQLColumnType.Int:
                    case SQLColumnType.Bit:
                    case SQLColumnType.DateTime:
                    case SQLColumnType.Decimal:
                        return new ReadOnlySpan<byte>(_value);
                    default:
                        throw new InvalidOperationException("Unknown fixed binary length SQL type");
                }
            }
            else
            {
                if (Column.DataType == SQLColumnType.Varchar || Column.DataType == SQLColumnType.Char
                    || Column.DataType == SQLColumnType.Varbinary || Column.DataType == SQLColumnType.Binary)
                {
                    if (includeSizeInBinaryIfNotFixed)
                    {
                        var lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                        var resultArray = new byte[lengthArray.Length + _value.Length];
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

            return null;
        }

        /// <summary>
        /// Returns the value in a binary format
        /// </summary>
        /// <param name="includeSizeInBinaryIfNotFixed">Specify true if you want non-fixed values to have an int sized prefix of the length</param>
        /// <returns>The value in binary format</returns>
        /// <remarks>Use this function when saving to a <seealso cref="IBaseDataPage"/>'s data. The non-fixed length value types (char/varchar) will need a prefix of the size (also in binary).
        /// Usually <c>TRUE</c> if you are trying to save to a <seealso cref="IBaseDataPage"/>'s data. 
        /// Otherwise usually <c>FALSE</c> if you just want the value itself.</remarks>
        public byte[] GetValueInBinary(bool includeSizeInBinaryIfNotFixed = true)
        {
            if (Column.IsFixedBinaryLength())
            {
                switch (Column.DataType)
                {
                    case SQLColumnType.Int:
                    case SQLColumnType.Bit:
                    case SQLColumnType.DateTime:
                    case SQLColumnType.Decimal:
                        return _value;
                    default:
                        throw new InvalidOperationException("Unknown fixed binary length SQL type");
                }
            }
            else
            {
                if (Column.DataType == SQLColumnType.Varchar || Column.DataType == SQLColumnType.Char
                 || Column.DataType == SQLColumnType.Varbinary || Column.DataType == SQLColumnType.Binary)
                {
                    if (includeSizeInBinaryIfNotFixed)
                    {
                        var lengthArray = DbBinaryConvert.IntToBinary(_value.Length);
                        var resultArray = new byte[lengthArray.Length + _value.Length];
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

            return null;
        }

        /// <summary>
        /// Returns the binary array size of the value
        /// </summary>
        /// <returns>the binary array size of the value</returns>
        public int BinarySize()
        {
            if (Column.IsFixedBinaryLength())
            {
                switch (Column.DataType)
                {
                    case SQLColumnType.Int:
                        return Constants.SIZE_OF_INT;
                    case SQLColumnType.Bit:
                        return Constants.SIZE_OF_BOOL;
                    case SQLColumnType.DateTime:
                        return Constants.SIZE_OF_DATETIME;
                    case SQLColumnType.Decimal:
                        return Constants.SIZE_OF_DECIMAL;
                    default:
                        return 0;
                }
            }
            else
            {
                return _value.Length;
            }
        }

        /// <summary>
        /// Returns the value represented as a string
        /// </summary>
        /// <returns>The value in string format</returns>
        public string GetValueInString()
        {
            if (Column.IsFixedBinaryLength())
            {
                if (Column.DataType == SQLColumnType.Int)
                {
                    int value = DbBinaryConvert.BinaryToInt(_value);
                    return value.ToString();
                }

                if (Column.DataType == SQLColumnType.DateTime)
                {
                    DateTime value = DbBinaryConvert.BinaryToDateTime(_value);
                    return value.ToString();
                }

                if (Column.DataType == SQLColumnType.Decimal)
                {
                    decimal value = DbBinaryConvert.BinaryToDecimal(_value);
                    return value.ToString();
                }

                if (Column.DataType == SQLColumnType.Bit)
                {
                    bool value = DbBinaryConvert.BinaryToBoolean(_value);
                    return value.ToString();
                }
            }
            else
            {
                if (Column.DataType == SQLColumnType.Varchar)
                {
                    string value = DbBinaryConvert.BinaryToString(_value);
                    return value.ToString();
                }

                if (Column.DataType == SQLColumnType.Char)
                {
                    string value = DbBinaryConvert.BinaryToString(_value);
                    return value.ToString();
                }
            }

            return string.Empty;
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
            if (value.Length > Column.MaxLength)
            {
                throw new InvalidOperationException($"The value length: {value.Length.ToString()} is greater than the column's specified length {Column.MaxLength.ToString()} for {Column.Name}");
            }
            else
            {
                return true;
            }
        }

        private bool IsValidLengthChar(string value)
        {
            if (value.Length != Column.Length)
            {
                throw new InvalidOperationException($"The value length: {value.Length.ToString()} is not equal than the column's specified length {Column.Length.ToString()} for {Column.Name}");
            }
            else
            {
                return true;
            }
        }
        #endregion

    }
}
