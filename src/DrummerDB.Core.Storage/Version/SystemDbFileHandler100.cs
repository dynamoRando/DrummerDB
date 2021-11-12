using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Storage.Version
{
    /// <summary>
    /// Handles a system database files on disk (log and data) of version 100
    /// </summary>
    /// <seealso cref="Drummersoft.DrummerDB.Core.Storage.Abstract.SystemDbFileHandler" />
    /// <remarks>Strongly related to (almost a copy of) the functions of a <seealso cref="UserDbFileHandler"/></remarks>
    internal class SystemDbFileHandler100 : SystemDbFileHandler
    {
        #region Private Fields
        private string _diskFileName = string.Empty;
        private string _logFileExtension = string.Empty;
        private string _dataFileExtension = string.Empty;
        private string _storageFolderPath = string.Empty;
        private Guid _dbId;

        private DbSystemDataFile _dataFile;
        private IDbLogFile _logFile;
        #endregion

        #region Public Properties
        public override string DatabaseName => _dataFile.DatabaseName;
        public override int Version => Constants.DatabaseVersions.V100;
        public override Guid DbId => _dbId;
        public override string DiskFileName => _diskFileName;
        #endregion

        #region Constructors
        public SystemDbFileHandler100(string diskFileName, string storageFolderPath, string dataFileExtension, string logFileExtension, DbSystemDataFile dataFile, IDbLogFile logFile, Guid dbId)
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
        public override UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, int tableId)
        {
            return _dataFile.GetAnyUserDataPage(pagesInMemory, schema, tableId);
        }

        public override int GetMaxPageId(TreeAddress address)
        {
            return _dataFile.GetMaxPageId(address);
        }

        public override ISystemPage GetSystemPage()
        {
            return _dataFile.GetSystemPage();
        }

        public override int GetTotalPages()
        {
            return _dataFile.GetTotalPages();
        }

        public override int GetTotalPages(TreeAddress address)
        {
            return _dataFile.GetTotalPages(address);
        }

        public override void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType, bool isDeleted)
        {
            _dataFile.WritePageToDisk(pageData, address, type, dataPageType, isDeleted);
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
        #endregion

        #region Private Methods
        #endregion



    }
}
