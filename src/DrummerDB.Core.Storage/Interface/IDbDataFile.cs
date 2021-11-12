using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    internal interface IDbDataFile
    {
        UserDataPage GetUserDataPage(int id);
        UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, int tableId);
        List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema);
        void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType, bool isDeleted);
        Guid DbId { get; }
        int Version { get; }
        ISystemPage GetSystemPage();
        string DatabaseName { get; }
        int GetTotalPages();
        int GetTotalPages(TreeAddress address);
        int GetMaxPageId(in TreeAddress address);
        void DeleteFromDisk();
    }
}
