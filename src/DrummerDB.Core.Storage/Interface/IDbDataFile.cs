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
        UserDataPage GetUserDataPage(uint id);
        UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, uint tableId);
        List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema);
        void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType, bool isDeleted);
        Guid DbId { get; }
        int Version { get; }
        ISystemPage GetSystemPage();
        string DatabaseName { get; }
        uint GetTotalPages();
        uint GetTotalPages(TreeAddress address);
        uint GetMaxPageId(in TreeAddress address);
        void DeleteFromDisk();
    }
}
