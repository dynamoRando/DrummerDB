using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Exceptions;
using Drummersoft.DrummerDB.Core.Structures.SQLType;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.DbDebug
{
    /// <summary>
    /// Used in debugging data for the Row. Most of these functions are copy/pasted from 
    /// <see cref="RowValue"/>
    /// </summary>
    internal class RowValueDebug
    {
        public ColumnSchema Column { get; set; }
        public byte[] Value { get; set; }
        public bool IsNull { get; set; }
        public int ParseValueLength { get; set; }
        public string ValueAsByteString => BitConverter.ToString(Value);
        public string DebugLine => Column.Name + ":" + ValueAsByteString;

        public string DebugValue()
        {
            if (Value is not null)
            {
                return BitConverter.ToString(Value);
            }

            return string.Empty;
        }

        public void SetValueAsNull()
        {
            if (Column.IsNullable)
            {
                IsNull = true;
                Value = DbBinaryConvert.BooleanToBinary(IsNull);
            }
            else
            {
                throw new InvalidOperationException($"You may not set a value as NULL on Non-Nullable Column {Column.Name}");
            }
        }

        public void SetValue(byte[] value)
        {
            Value = value;
            IsNull = false;
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
                    Value = span.ToArray();
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
                IsNull = DbBinaryConvert.BinaryToBoolean(span.Slice(0, Constants.SIZE_OF_BOOL));

                if (IsNull)
                {
                    return;
                }
                else
                {
                    if (Column.IsFixedBinaryLength)
                    {
                        Value = span.ToArray(); // set the value with the leading bool prefix
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
                            SetValue(span.Slice(Constants.SIZE_OF_INT + Constants.SIZE_OF_BOOL, length));
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Attempts to set the value based on the passed in string representation. It will validate if the value is appropriate for the columns data type before setting value.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="padIfNeeded">Pads the value passed in if needed</param>
        /// <remarks>Do not call this function if you are trying to set NULL. Use <see cref="SetValueAsNull"/> instead.</remarks>
        public void SetValue(string value, bool padIfNeeded = false)
        {
            IsNull = false;

            switch (Column.DataType)
            {
                case SQLVarChar varchar:
                    if (IsValidLengthVarChar(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.StringToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(IsNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            Value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, Value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, Value, isNull.Length, convertedValue.Length);

                            string debugFinalValue = BitConverter.ToString(Value);
                            Debug.WriteLine(debugFinalValue);

                        }
                        else
                        {
                            Value = DbBinaryConvert.StringToBinary(value);
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
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(IsNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            Value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, Value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, Value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(Value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            Value = DbBinaryConvert.StringToBinary(value);
                        }

                    }
                    break;
                case SQLInt foo:
                    if (IsValidInt(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.IntToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(IsNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            Value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, Value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, Value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(Value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            Value = DbBinaryConvert.IntToBinary(value);
                        }

                    }
                    break;
                case SQLDateTime a:
                    if (IsValidDateTime(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.DateTimeToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(IsNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            Value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, Value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, Value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(Value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            Value = DbBinaryConvert.DateTimeToBinary(value);
                        }

                    }
                    break;
                case SQLBit b:
                    if (IsValidBoolean(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.BooleanToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(IsNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            Value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, Value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, Value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(Value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            Value = DbBinaryConvert.BooleanToBinary(value);
                        }
                    }
                    break;
                case SQLDecimal c:
                    if (IsValidDecimal(value))
                    {
                        if (Column.IsNullable)
                        {
                            byte[] convertedValue = DbBinaryConvert.DecimalToBinary(value);
                            byte[] isNull = DbBinaryConvert.BooleanToBinary(IsNull);

                            string debugConvertedValue = BitConverter.ToString(convertedValue);
                            Debug.WriteLine(debugConvertedValue);

                            Value = new byte[convertedValue.Length + isNull.Length];
                            Array.Copy(isNull, 0, Value, 0, isNull.Length);
                            Array.Copy(convertedValue, 0, Value, isNull.Length, convertedValue.Length);

                            string debugValue = BitConverter.ToString(Value);
                            Debug.WriteLine(debugValue);
                        }
                        else
                        {
                            Value = DbBinaryConvert.DecimalToBinary(value);
                        }
                    }
                    break;
                default:
                    throw new UnknownSQLTypeException($"{Column.DataType.GetType().ToString()} is unknown");
            }
        }

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

    }
}
