namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    /// <summary>
    /// Used to specify the status of a row on a page
    /// </summary>
    internal enum PageRowStatus
    {
        /// <summary>
        /// The row is on the page
        /// </summary>
        IsOnPage,

        /// <summary>
        /// The row is on the page and has been forwarded (i.e. it has been updated)
        /// </summary>
        IsOnPageAndForwardedOnSamePage,

        /// <summary>
        /// The row has been marked as forwarded to another page in cache
        /// </summary>
        IsForwardedToOtherPage,

        /// <summary>
        /// The row is not on the page
        /// </summary>
        NotOnPage,

        /// <summary>
        /// An unknown status of the row on page
        /// </summary>
        Unknown
    }
}
