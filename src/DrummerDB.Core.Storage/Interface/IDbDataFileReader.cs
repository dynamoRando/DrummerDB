using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Maps;

namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    /// <summary>
    /// An object that reads a database file for basic properties. 
    /// </summary>
    /// <remarks>Use <see cref="IDbDataFileReader"/> sparingly, for limited database file reading activities, such as getting the database version or database id.
    /// Leverage the call chain thru <see cref="ICacheManager"/> for most of your database actions. 
    /// For more information see the markdown file Architecture.md</remarks>
    internal interface IDbDataFileReader
    {
        string DatabaseName { get; }
        DateTime CreatedDate { get; }
        Guid DatabaseId { get; }
        UserDataPageSearchResult GetAnyUserDataPage(string fileName, ITableSchema schema, PageAddress[] pagesInMemory, int tableId);

        // reads the entire data file and returns the page items on disk
        List<PageItem> GetPageItems(string fileName);
        List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema);

    }
}
