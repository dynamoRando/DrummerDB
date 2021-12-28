using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface IRow
    {
        public uint Id { get; }
        public RowType Type { get; }

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
        /// Caculates the entire size of the row including the preamble and any size prefixes needed
        /// </summary>
        /// <returns></returns>
        uint TotalSize { get; }
        uint RemoteSize { get; }
        uint ValueSize { get; }

        /// <summary>
        /// If the row has been flagged as deleted
        /// </summary>
        bool IsLogicallyDeleted { get; }

        /// <summary>
        /// If the row has been updated and saved to a different location 
        /// </summary>
        bool IsForwarded { get; }

        /// <summary>
        /// Sets the row to forwarded and sets the Offset value as the offset where the row can now be found on the specified page
        /// </summary>
        /// <param name="newOffset">The byte offset where the row can now be found</param>
        /// <param name="pageId">The page where the forwarded row can be found</param>
        void ForwardRow(uint newOffset, uint pageId);

        /// <summary>
        /// The number of bytes offset on the currrent page for the forwarded values
        /// </summary>
        public uint ForwardOffset { get; }

        /// <summary>
        /// The page id of the forwarded row. If the row is on the current page, this will be zero.
        /// </summary>
        public uint ForwardedPageId { get; }
   
    }
}
