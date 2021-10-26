using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;

namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    internal interface IDbFilePageReader
    {
        /// <summary>
        /// Returns the specified page from disk
        /// </summary>
        /// <param name="id">The page to get from disk</param>
        /// <returns>The specified page</returns>
        UserDataPage GetUserDataPage(int id);

        /// <summary>
        /// Returns the next available page that is not already in memory
        /// </summary>
        /// <param name="pagesInMemory">A list of pages already in memory</param>
        /// <returns>The next page found on disk that is not already in memory</returns>
        UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory);

        int FileOffset { get; set; }
    }
}
