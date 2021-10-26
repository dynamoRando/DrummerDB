using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using Drummersoft.DrummerDB.Core.Structures.Version;

namespace Drummersoft.DrummerDB.Core.Structures.Factory
{
    internal static class BaseDataPageFactory
    {
        /// <summary>
        /// Creates a Base Data Page (version 100) based on the supplied byte array and table schema. 
        /// </summary>
        /// <param name="data">The bytes that make the data page</param>
        /// <param name="schema">The schema for the data page</param>
        /// <returns>A Base Data Page (Version 100)</returns>
        /// <remarks>Use when populating the object from disk</remarks>
        internal static BaseDataPage100 GetBaseDataPage100(byte[] data, ITableSchema schema)
        {
            return new BaseDataPage100(data, schema);
        }

        /// <summary>
        /// Creates a Base Data Page (version 100) based on the supplied page address, table schema, and the data page type (see DataPageType)
        /// </summary>
        /// <param name="address">The address of the page</param>
        /// <param name="schema">The schema for the page</param>
        /// <param name="dataPageType">The type of data page</param>
        /// <param name="cache">A reference to cache</param>
        /// <returns>A Base Data Page (Version 100)</returns>
        /// <remarks>Use when populating a new object in memory</remarks>
        internal static BaseDataPage100 GetBaseDataPage100(PageAddress address, ITableSchema schema, DataPageType dataPageType)
        {
            return new BaseDataPage100(address, schema, dataPageType);
        }
    }
}
