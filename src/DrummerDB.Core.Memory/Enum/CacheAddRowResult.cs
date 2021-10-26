namespace Drummersoft.DrummerDB.Core.Memory.Enum
{
    /// <summary>
    /// Used to report the state of an attempted addition of a row to cache
    /// </summary>
    internal enum CacheAddRowResult
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
        Success,

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
