using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage.Abstract
{
    internal abstract class UserDbFileHandler : IDbDataFile
    {
        public abstract Guid DbId { get; }
        public abstract int Version { get; }
        public abstract string DatabaseName { get; }
        public abstract UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, uint tableId);
        public abstract UserDataPage GetUserDataPage(uint id);
        public abstract List<UserDataPage> GetAllPages(TreeAddress address);
        public abstract void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType, bool isDeleted);
        public abstract ISystemPage GetSystemPage();
        public abstract uint GetTotalPages();
        public abstract uint GetMaxPageId(in TreeAddress address);
        public abstract void LogOpenTransactionToDisk(TransactionEntry transaction);
        public abstract void LogCloseOpenTransactionToDisk(TransactionEntry transaction);
        public abstract bool OpenTransactionIsOnDisk(TransactionEntryKey key);
        public abstract void RemoveOpenTransactionOnDisk(TransactionEntry transaction);
        public abstract void DeleteFromDisk();
        public abstract uint GetTotalPages(TreeAddress address);
        public abstract List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema);
        public abstract DataFileType DataFileType { get; }
    }
}