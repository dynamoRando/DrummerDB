using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    /// <summary>
    /// Responsible for managing disk access
    /// </summary>
    internal interface IStorageManager
    {
        /// <summary>
        /// Creates a new system database on disk with the specified parameters
        /// </summary>
        /// <param name="dbName">The name of the db to create</param>
        /// <param name="pages">The initial set of pages to save to disk</param>
        /// <param name="auth">A reference to the authentication manager</param>
        /// <param name="version">The version number of the database</param>
        void CreateSystemDatabase(string dbName, List<IPage> pages, DataFileType type, int version = Constants.MAX_DATABASE_VERSION);

        /// <summary>
        /// Creates a new user database on disk with the specified parameters
        /// </summary>
        /// <param name="dbName">The name of the db to create</param>
        /// <param name="pages">The initial set of pages to save to disk</param>
        /// <param name="auth">A reference to the authentication manager</param>
        /// <param name="version">The version number of the database</param>
        void CreateUserDatabase(string dbName, List<IPage> pages, DataFileType type, int version = Constants.MAX_DATABASE_VERSION);

        bool DeleteUserDatabase(string dbName);

        public List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema);

        public UserDataPage GetAnySystemDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema, int tableId);

        /// <summary>
        /// Gets any user data page from disk, excluding the ones supplied that are already in memory.
        /// </summary>
        /// <param name="pagesInMemory">The pages that are already in memory.</param>
        /// <param name="address">The address of the page you need.</param>
        /// <param name="schema">The schema of table you will be needing.</param>
        /// <returns></returns>
        UserDataPage GetAnyUserDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema, int tableId);

        /// <summary>
        /// Returns the max page id found on disk. If there are no pages, will return 0. (For a new database/table)
        /// </summary>
        /// <param name="address">The address to look up</param>
        /// <returns>The max page id for the specified tree.</returns>
        int GetMaxPageId(TreeAddress address);

        List<string> GetSystemDatabaseNames();

        /// <summary>
        /// Gets the system page from disk for the specified database.
        /// </summary>
        /// <param name="dbId">The database identifier.</param>
        /// <returns>The system page from disk</returns>
        ISystemPage GetSystemPage(Guid dbId);

        /// <summary>
        /// Returns the <see cref="ISystemPage"/> for the specified system database name from disk. If the internal collection is not initalized or does not contain the specified file, it will attempt to load into memory.
        /// </summary>
        /// <param name="dbName">The system database name to get the system page from</param>
        /// <returns>The <see cref="ISystemPage"/> for the system db name specified, or <c>NULL</c> if not found</returns>
        ISystemPage GetSystemPageForSystemDatabase(string dbName);

        /// <summary>
        /// Returns the total number of pages on disk for the specified address
        /// </summary>
        /// <param name="address">The address of the pages</param>
        /// <returns>The total number of pages on disk. This will return 0 if a brand new database/table</returns>
        int GetTotalPages(TreeAddress address);

        List<UserDatabaseInformation> GetUserDatabasesInformation();

        bool IsSystemDatabase(Guid databaseId);

        bool IsUserDatabase(Guid databaseId);
        void LoadSystemDatabaseFilesIntoMemory();

        void LoadUserDatabaseFilesIntoMemory();

        /// <summary>
        /// Searches the log file for the transaction (by <see cref="TransactionEntry.TransactionId"/> and re-saves the entry
        /// for the field <see cref="TransactionEntry.IsCompleted"/>
        /// </summary>
        /// <param name="address">The address of the transaction (the database id)</param>
        /// <param name="transaction">A copy of the transaction</param>
        void LogCloseTransaction(Guid databaseId, TransactionEntry transaction);

        bool LogFileHasOpenTransaction(Guid databaseId, TransactionEntryKey key);

        /// <summary>
        /// Saves the specified transaction entry to disk
        /// </summary>
        /// <param name="address">The address of the transaction (the database)</param>
        /// <param name="transaction">A copy of the transaction to save to disk</param>
        void LogOpenTransaction(Guid databaseId, TransactionEntry transaction);

        /// <summary>
        /// Searches the log file for the transaction (by <see cref="TransactionEntry.TransactionId"/> and re-saves the entry
        /// for the field <see cref="TransactionEntry.IsDeleted"/>
        /// </summary>
        /// <param name="address">The address of the transaction (the database id)</param>
        /// <param name="transaction">A copy of the transaction</param>
        void RemoveOpenTransaction(Guid databaseId, TransactionEntry transaction);

        /// <summary>
        /// Saves the page data to disk.
        /// </summary>
        /// <param name="address">The address of the page.</param>
        /// <param name="data">The binary data.</param>
        /// <param name="type">The type of page to be saved to disk.</param>
        void SavePageDataToDisk(PageAddress address, byte[] data, PageType type, DataPageType dataPageType);
        int TotalSystemDatabasesOnDisk();
        int TotalUserDatabasesOnDisk();
        bool MarkPageAsDeleted(PageAddress address);
    }
}
