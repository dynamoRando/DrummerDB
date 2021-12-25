﻿using System;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface IRow
    {
        /// <summary>
        /// The Id of the row
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// If the row is local to the host database or not
        /// </summary>
        bool IsLocal { get; set; }

        /// <summary>
        /// The participant id if the row is remote to the host database
        /// </summary>
        Guid? ParticipantId { get; set; }

        /// <summary>
        /// A list of Values for this row
        /// </summary>
        IRowValue[] Values { get; set; }

        /// <summary>
        /// A hash of the row's data, populated only if the row is not local
        /// </summary>
        byte[] Hash { get; set; }

        /// <summary>
        /// Returns the entire row in binary format, ordered by binary save format (usually to save data to a Page or to send to a Participant over the wire)
        /// </summary>
        /// <returns>A binary representation of the row</returns>
        /// <remarks>A local row consists of: preamble + sizeOfRow + rowData (ordered by fixed binary columns first, then by variable binary columns, each with an INT size prefix before the actual data for variable size columns.)</remarks>
        byte[] GetRowInPageBinaryFormat();

        /// <summary>
        /// Returns the entire row in binary format, orderd by binary save format. This function is the same as <see cref="GetRowInPageBinaryFormat"/> if the row <see cref="IsLocal"/>, otherwise if it is remote it will return only the Preamble and ParticipantId.
        /// </summary>
        /// <returns>The row in binary format for saving to the transaction log.</returns>
        byte[] GetRowInTransactionBinaryFormat();

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
        /// Caculates the entire size of the row including the preamble and any size prefixes needed
        /// </summary>
        /// <returns></returns>
        int Size();

        /// <summary>
        /// Sorts the row values in binary format (for saving to disk)
        /// </summary>
        void SortBinaryOrder();

        /// <summary>
        /// Sorts the row values in column ordinal format
        /// </summary>
        void SortOrdinalOrder();

        /// <summary>
        /// If the row has been flagged as deleted
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// If the row has been updated and saved to a different location 
        /// </summary>
        bool IsForwarded { get; set; }

        /// <summary>
        /// Sets the row to forwarded and sets the Offset value as the offset where the row can now be found on the specified page
        /// </summary>
        /// <param name="newOffset">The byte offset where the row can now be found</param>
        /// <param name="pageId">The page where the forwarded row can be found</param>
        void ForwardRow(int newOffset, int pageId);

        /// <summary>
        /// The number of bytes offset on the currrent page for the forwarded values
        /// </summary>
        public int ForwardOffset { get; set; }

        /// <summary>
        /// The page id of the forwarded row. If the row is on the current page, this will be zero.
        /// </summary>
        public int ForwardedPageId { get; set; }
        public ReadOnlySpan<byte> GetValueInByteSpan(string columnName);
        void SetValueAsNullForColumn(string columnName);

        bool IsValueNull(string columnName);
    }
}
