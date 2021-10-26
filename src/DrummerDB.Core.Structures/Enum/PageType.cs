namespace Drummersoft.DrummerDB.Core.Structures.Enum
{
    internal enum PageType
    {
        /// <summary>
        /// An unknown type of page
        /// </summary>
        Unknown,

        /// <summary>
        /// Contains index information
        /// </summary>
        Index,

        /// <summary>
        /// Contains database metadata, i.e database name, database version, created date, etc.
        /// </summary>
        System,

        /// <summary>
        /// Contract information for participants in the database
        /// </summary>
        Contract,

        /// <summary>
        /// For table information, etc.
        /// </summary>
        Data
    }
}
