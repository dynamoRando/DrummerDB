using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures.Abstract;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// A structure used by <seealso cref="IDbDataFileReader"/> to find user data pages on disk and their location.
    /// </summary>
    internal class UserDataPageSearchResult
    {
        internal UserDataPage UserDataPage { get; set; }
        internal int Order { get; set; }
        internal int Offset { get; set; }
    }
}
