using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Memory.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Databases.Abstract
{
    /// <summary>
    /// A base representation of a database. Note that a database is an abstraction, the actual data structures are in 
    /// memory (<seealso cref="ICacheManager"/>) or are on disk (<seealso cref="IStorageManager"/>).
    /// </summary>
    internal abstract class UserDatabase : IDatabase
    {
        #region Private Fields
        #endregion

        #region Public Properties
        public abstract string Name { get; }
        public abstract int Version { get; }
        public abstract Guid Id { get; }
        #endregion

        #region Constructors
        public UserDatabase(DatabaseMetadata metadata) { }
        #endregion

        #region Public Methods        
        public abstract DatabaseType DatabaseType { get; }
        public abstract bool IsReadyForCooperation();

        /// <summary>
        /// Adds the table to the database. This will add the table to Cache (and thru Cache, to Storage) and to the database's internal metadata object.
        /// </summary>
        /// <param name="schema">The schema of the table to add</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
        public abstract bool AddTable(TableSchema schema, out Guid tableObjectId);
        public abstract bool TryAddTable(TableSchema schema, TransactionRequest transaction, TransactionMode transactionMode, out Guid tableObjectId);
        public abstract bool TryDropTable(string tableName, TransactionRequest transaction, TransactionMode transactionMode);

        /// <summary>
        /// Checks the db's <seealso cref="DatabaseMetadata"/> (and therefore the System Data Pages) to see if the database has the specified table
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <returns><c>true</c> if the database has the table, otherwise <c>false</c></returns>
        public abstract bool HasTable(string tableName);
        public abstract bool HasTable(string tableName, string schemaName);
        public abstract bool HasTable(int tableId);
        public abstract bool HasUser(string userName);
        public abstract bool HasUser(string userName, Guid userId);
        public abstract bool TryCreateSchema(string schemaName, TransactionRequest request, TransactionMode transactionMode);
        public abstract bool HasSchema(string schemaName);
        public abstract DatabaseSchemaInfo GetSchemaInformation(string schemaName);

        /// <summary>
        /// Creates a user with the specified userName and pw. This will hash the pw and store in the database's metadata.
        /// </summary>
        /// <param name="userName">The userName to be created</param>
        /// <param name="pwInput">The pw to be assigned</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
        public abstract bool CreateUser(string userName, string pwInput);
        public abstract bool ValidateUser(string userName, string pwInput);
        public abstract bool AuthorizeUser(string userName, string pwInput, DbPermission permission, Guid objectId);

        /// <summary>
        /// Returns a reference to the specified table in the database
        /// </summary>
        /// <param name="tableName">The name of table to find</param>
        /// <returns>The table specified</returns>
        public abstract Table GetTable(string tableName);
        public abstract Table GetTable(string tableName, string schemaName);
        public abstract Table GetTable(int tableId);
        public abstract List<TransactionEntry> GetOpenTransactions();

        /// <summary>
        /// Checks the actual log file to see if the transaction id specified is on disk and is marked as open
        /// </summary>
        /// <param name="transactionId">The transaction to find on disk</param>
        /// <returns><c>true</c> if the transaciton is open, otherwise <c>false</c></returns>
        public abstract bool LogFileHasOpenTransaction(TransactionEntryKey key);
        public abstract bool GrantUserPermission(string userName, DbPermission permission, Guid objectId);
        public abstract Guid GetTableObjectId(string tableName);
        public abstract int GetMaxTableId();
        #endregion

        #region Private Methods
        #endregion
    }
}
