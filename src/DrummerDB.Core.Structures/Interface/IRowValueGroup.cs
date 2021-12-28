using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    /// <summary>
    /// Represents a row that has <see cref="RowValue"/>
    /// </summary>
    internal interface IRowValueGroup
    {
        /// <summary>
        /// A list of Values for this row
        /// </summary>
        IRowValue[] Values { get; set; }

        /// <summary>
        /// Returns the value in string format for the specified column in this row 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        string GetValueInString(string columnName);

        /// <summary>
        /// Returns the value for the specified column in binary format. Does not include the size prefix if variable binary length.
        /// </summary>
        /// <param name="columnName">The column to get the value for</param>
        /// <returns>The value of the specified column in binary format</returns>
        byte[] GetValueInByte(string columnName);

        /// <summary>
        /// Attempts to populate a row's data based on the supplied byte span. 
        /// </summary>
        /// <param name="schema">The schema of the table the row belongs to</param>
        /// <param name="span">The byte array span (usually from the Page's data)</param>
        /// <remarks>This function assumes in the Span an INT prefix indicating the length of the value if it is
        /// a variable binary length type (<seealso cref="SQLChar"/>, <seealso cref="SQLVarChar"/>,
        /// <seealso cref="SQLBinary"/>,<seealso cref="SQLVarbinary"/>).</remarks>
        void SetRowData(ITableSchema schema, ReadOnlySpan<byte> span);

        /// <summary>
        /// Set a value for a specific column in this row 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        void SetValue(string columnName, string value);

        /// <summary>
        /// Sets a value for a specific column in the row
        /// </summary>
        /// <param name="columnName">The column to set the value</param>
        /// <param name="value">The value in byte array format</param>
        void SetValue(string columnName, byte[] value);

        /// <summary>
        /// Sorts the row values in binary format (for saving to disk)
        /// </summary>
        void SortBinaryOrder();

        /// <summary>
        /// Sorts the row values in column ordinal format
        /// </summary>
        void SortOrdinalOrder();

        public ReadOnlySpan<byte> GetValueInByteSpan(string columnName);

        void SetValueAsNullForColumn(string columnName);

        bool IsValueNull(string columnName);
    }
}
