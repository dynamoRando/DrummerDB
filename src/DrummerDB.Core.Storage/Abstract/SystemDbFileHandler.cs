using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Storage.Abstract
{
    internal abstract class SystemDbFileHandler
    {
        public abstract string DatabaseName { get; }
        public abstract int Version { get; }
        public abstract Guid DbId { get; }
        public abstract UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, int tableId);
        public abstract void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType);
        public abstract int GetTotalPages();
        public abstract int GetTotalPages(TreeAddress address);
        public abstract int GetMaxPageId(TreeAddress address);
        public abstract ISystemPage GetSystemPage();
        public abstract string DiskFileName { get; }
        public abstract void LogOpenTransactionToDisk(TransactionEntry transaction);
        public abstract void LogCloseOpenTransactionToDisk(TransactionEntry transaction);
        public abstract void RemoveOpenTransactionOnDisk(TransactionEntry transaction);
    }
}
