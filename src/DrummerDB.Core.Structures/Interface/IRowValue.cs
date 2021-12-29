using System;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface IRowValue
    {
        ColumnSchema Column { get; set; }

        /// <summary>
        /// The binary length of the value
        /// </summary>
        /// <returns>
        /// The binary length of the value. 
        /// If this is a non-nullable fixed length, this returns a fixed size. 
        /// If this is a non-nullable variable length, this returns a variable length size WITHOUT a leading 4 byte integer prefix indicating the length.
        /// If this is a nullable fixed length, this returns either a 1 byte size (if null, indicating <c>TRUE</c> for null) or a 1 byte size (for the nullable status) and the fixed size. (example: 5 bytes for a nullable int: 1 byte BOOL + 4 byte INT).
        /// If this is a nullable variable length, this returns either a 1 byte size (if null, indicating <c>TRUE</c> for null), or a 1 byte size (for the nullable status), and the variable byte array itself. THIS DOES NOT INCLUDE THE 4 BYTE INT SIZE PREFIX.
        /// </returns>
        uint BinarySize();

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value</returns>
        /// <exception cref="InvalidOperationException">Thrown if the value is NULL. You must check first by calling <see cref="IsNull"/>.</exception>
        string GetValueInString();
        void SetValue(ReadOnlySpan<byte> span);
        void SetValue(string value, bool padIfNeeded = false);

        /// <summary>
        /// Sets the binary value directly and performs no manipulation of the values.
        /// </summary>
        /// <param name="value">A binary representation of the value.</param>
        /// <remarks>Usually this is called by <see cref="Row.SetRowData(ITableSchema, ReadOnlySpan{byte})"/>. 
        /// 
        /// In that function, if the value is fixed binary, the value passed in is just the fixed binary array.
        /// 
        /// If the value is variable binary, the value passed in IS JUST THE VARIABLE LENGTH BINARY DATA (no leading 4 byte INT prefix). 
        /// 
        /// If the value is nullable fixed binary, the value is pased in with the leading 1 byte BOOL IsNull status along with the remaining fixed length array if not null. 
        /// If it is null, the value is just set to a 1 byte BOOL value of TRUE for NULL status.
        /// 
        /// If the value is nullable variable binary and not null, the value passed in is JUST THE VARIABLE LENGTH BINARY DATA (no leading 4 byte INT prefix).
        /// 
        /// If the value is nullable variable binary and null, the value itself is just set to a 1 byte BOOL value of TRUE for NULL status.</remarks>
        void SetValue(byte[] value);

        /// <summary>
        /// Returns a byte array representation of the value.
        /// </summary>
        /// <param name="includeSizeInBinaryIfNotFixed">Default <c>TRUE</c>. If the data type is variable length, include a 4 byte INT prefix of the size of the array. This is used when saving data to the page.</param>
        /// <param name="includeNullablePrefix">Default <c>TRUE</c>. If the datat ype is nullable, include a 1 byte BOOL prefix indicating if the value is null or not.</param>
        /// <returns>Returns a byte array representation of the value.
        /// If the value is fixed binary length: returns just the value itself.
        /// If the value is variable binary length: returns a 4 byte INT prefix of the length, plus the binary data itself.
        /// If the value is nullable fixed binary length: returns either a 1 byte BOOL indicating NULL (if null) or a 1 byte BOOL value, plus the fixed binary size itself.
        /// If the value is nullable variable binary length: returns either a 1 byte BOOL indicating NULL (if null) or a 1 byte BOOL value, a 4 byte INT length value, plus the variable length binary data itself.</returns>
        byte[] GetValueInBinary(bool includeSizeInBinaryIfNotFixed = true, bool includeNullablePrefix = true);
        ReadOnlySpan<byte> GetValueInByteSpan(bool includeSizeInBinaryIfNotFixed = true, bool includeNullablePrefix = true);

        /// <summary>
        /// Sets the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <exception cref="NullReferenceException"></exception>
        void SetColumn(ColumnSchema column);
        bool IsNull();
        void SetValueAsNull();

        /// <summary>
        /// The total length to parse for the value. If this is a fixed data type, then it's the size of the data type.
        /// If it's a variable data type, then it's the total length of the data type + the 4 byte INT size. If it's NULLABLE, then include
        /// the BOOL prefix before the rest of the length. See <seealso cref="Row.SetRowData(ITableSchema, ReadOnlySpan{byte})"/> for the implementation.
        /// </summary>
        uint ParseValueLength { get; set; }
    }
}
