namespace Drummersoft.DrummerDB.Core.Memory.Enum
{
    /// <summary>
    /// Used to report the state of an attempted update of a row in cache
    /// </summary>
    internal enum CacheUpdateRowResult
    {
        /// <summary>
        /// Default unknown situation
        /// </summary>
        Unknown,

        /// <summary>
        /// Occurs when a row is succesfully updated in cache
        /// </summary>
        Success,

        /// <summary>
        /// Occurs when the tree (the table) has not yet been loaded into memory
        /// </summary>
        TreeNotInMemory,

        /// <summary>
        /// Occurs when the tree is empty
        /// </summary>
        NoPagesOnTree,

        /// <summary>
        /// Occurs when all pages on the tree are full
        /// </summary>
        NoRoomOnTree
    }
}
