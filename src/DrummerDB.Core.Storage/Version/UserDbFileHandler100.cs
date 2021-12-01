using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// Handles a user database's files on disk (log and data) of version 100
    /// </summary>
    internal class UserDbFileHandler100 : UserDbFileHandler
    {
        #region Private Fields
        private string _diskFileName = string.Empty;
        private string _logFileExtension = string.Empty;
        private string _dataFileExtension = string.Empty;
        private string _storageFolderPath = string.Empty;
        private Guid _dbId;

        private IDbDataFile _dataFile;
        private IDbLogFile _logFile;
        #endregion

        #region Public Properties
        public override Guid DbId => _dbId;

        public override int Version => Constants.DatabaseVersions.V100;
        public override string DatabaseName => _dataFile.DatabaseName;
        #endregion

        #region Constructors
        public UserDbFileHandler100(string diskFileName, string storageFolderPath, string dataFileExtension, string logFileExtension, IDbDataFile dataFile, IDbLogFile logFile, Guid dbId)
        {
            _diskFileName = diskFileName;
            _logFileExtension = logFileExtension;
            _dataFileExtension = dataFileExtension;
            _storageFolderPath = storageFolderPath;

            _dataFile = dataFile;
            _logFile = logFile;
            _dbId = dbId;
        }
        #endregion

        #region Public Methods
        public override void DeleteFromDisk()
        {
            _dataFile.DeleteFromDisk();
            _logFile.DeleteFromDisk();
        }

        public override bool OpenTransactionIsOnDisk(TransactionEntryKey key)
        {
            return _logFile.LogFileHasOpenTransaction(key);
        }

        public override void LogOpenTransactionToDisk(TransactionEntry transaction)
        {
            _logFile.LogOpenTransactionToDisk(transaction);
        }

        public override void LogCloseOpenTransactionToDisk(TransactionEntry transaction)
        {
            _logFile.LogCloseOpenTransaction(transaction);
        }

        public override void RemoveOpenTransactionOnDisk(TransactionEntry transaction)
        {
            _logFile.RemoveOpenTransactionOnDisk(transaction);
        }

        public override int GetMaxPageId(in TreeAddress address)
        {
            return _dataFile.GetMaxPageId(address);
        }

        public override int GetTotalPages()
        {
            return _dataFile.GetTotalPages();
        }

        public override int GetTotalPages(TreeAddress address)
        {
            return _dataFile.GetTotalPages(address);
        }

        /// <summary>
        /// Returns the specified page from disk
        /// </summary>
        /// <param name="id">The page to get from disk</param>
        /// <returns>The specified page</returns>
        public override UserDataPage GetUserDataPage(int id)
        {
            return _dataFile.GetUserDataPage(id);
        }

        /// <summary>
        /// Returns the next available page that is not already in memory
        /// </summary>
        /// <param name="pagesInMemory">A list of pages already in memory</param>
        /// <returns>The next page found on disk that is not already in memory</returns>
        public override UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, int tableId)
        {
            return _dataFile.GetAnyUserDataPage(pagesInMemory, schema, tableId);
        }

        public override List<UserDataPage> GetAllPages(TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public override void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType, bool isDeleted)
        {
            _dataFile.WritePageToDisk(pageData, address, type, dataPageType, isDeleted);
        }

        public override ISystemPage GetSystemPage()
        {
            return _dataFile.GetSystemPage();
        }

        public override List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema)
        {
            return _dataFile.GetAllUserDataPages(address, schema);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
