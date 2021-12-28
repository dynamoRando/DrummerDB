using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage.Abstract
{
    internal abstract class DbSystemDataFile : IDbDataFile
    {
        public abstract Guid DbId { get; }
        public abstract int Version { get; }
        public abstract string DatabaseName { get; }
        public abstract UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, uint tableId);
        public abstract uint GetMaxPageId(in TreeAddress address);
        public abstract ISystemPage GetSystemPage();
        public abstract uint GetTotalPages();
        public abstract uint GetTotalPages(TreeAddress address);
        public abstract UserDataPage GetUserDataPage(uint id);
        public abstract void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType, bool isDeleted);
        public abstract void DeleteFromDisk();
        public abstract List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema);
    }


}
