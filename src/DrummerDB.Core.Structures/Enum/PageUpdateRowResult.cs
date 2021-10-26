namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    internal enum PageUpdateRowResult
    {
        /// <summary>
        /// An unknown result
        /// </summary>
        Unknown,

        /// <summary>
        /// Specifies that there is not enough room on this page for the update
        /// </summary>
        NotEnoughRoom,

        /// <summary>
        /// Specifies that the update to this row on this page was successful
        /// </summary>
        Success
    }
}
