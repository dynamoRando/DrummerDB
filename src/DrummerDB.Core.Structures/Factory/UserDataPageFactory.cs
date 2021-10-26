using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Interface;

namespace Drummersoft.DrummerDB.Core.Structures.Factory
{

    /// <summary>
    /// Returns version appropriate Data Page objects
    /// </summary>
    internal static class UserDataPageFactory
    {
        /// <summary>
        /// Returns a new Data Page (Version 100). Use when populating an object from a byte array from disk
        /// </summary>
        /// <param name="data">The byte array from disk</param>
        /// <param name="schema">The table schema for the Data Page</param>
        /// <returns>A new Data Page (Version 100) populated from a byte array</returns>
        public static UserDataPage GetUserDataPage100(byte[] data, ITableSchema schema)
        {
            return new UserDataPage100(data, schema);
        }

        /// <summary>
        /// Returns a new Data Page (Version 100) and attempts to save various properties to the page's data. Use when constructing the object in memory. 
        /// </summary>
        /// <param name="address">The address of the page</param>
        /// <param name="schema">The table schema for the Data Page</param>
        /// <returns>A new Data Page (Version 100).</returns>
        public static UserDataPage GetUserDataPage100(PageAddress address, ITableSchema schema)
        {
            return new UserDataPage100(address, schema);
        }
    }
}
