namespace Drummersoft.DrummerDB.Core.Memory.Enum
{
    internal enum TreeStatus
    {
        /// <summary>
        /// Occurs when the tree (the table) has not yet been loaded into memory
        /// </summary>
        TreeNotInMemory,

        /// <summary>
        /// Occurs when all pages on the tree are full
        /// </summary>
        NoRoomOnTree,

        /// <summary>
        /// Occurs when a row is succesfully added to cache
        /// </summary>
        Ready,

        /// <summary>
        /// Occurs when the tree is empty
        /// </summary>
        NoPagesOnTree,

        /// <summary>
        /// Default unknown situation
        /// </summary>
        Unknown
    }
}
