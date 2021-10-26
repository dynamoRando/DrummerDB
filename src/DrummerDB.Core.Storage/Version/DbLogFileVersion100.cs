using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using System;
using System.IO;
using System.Threading;

namespace Drummersoft.DrummerDB.Core.Storage.Version
{
    /*
      note - should probably split out sub-classes here for a LogFileWriter and LogFileReader to keep pattern in place with data file,
             need to do later
     */

    internal class DbLogFileVersion100 : IDbLogFile
    {
        #region Private Fields
        private long _fileSize = 0;
        private int _V100 = Constants.DatabaseVersions.V100;
        private string _fileName;
        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private TransactionItemMap _map;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a Db Log file Version 100. If the file does not exist on disk, it will create it.
        /// </summary>
        /// <param name="fileName"></param>
        public DbLogFileVersion100(string fileName)
        {
            _map = new TransactionItemMap();
            _fileName = fileName;

            if (!File.Exists(fileName))
            {
                CreateDbLogFile(fileName);
            }

            SetFileSize();
        }

        #endregion

        #region Public Methods
        public void DeleteFromDisk()
        {
            File.Delete(_fileName);
        }

        public bool LogFileHasOpenTransaction(TransactionEntryKey key)
        {
            if (!_map.HasTransaction(key))
            {
                PopulateMap();
            }

            long offset = _map.GetOffset(key);

            offset = _map.GetOffset(key);

            // the first four bytes are the size of the transaction, so skip the size
            offset += Constants.SIZE_OF_INT;

            ReadOnlySpan<byte> preamble = GetPremableAtOffset(offset);

            var readTransaction = new TransactionEntry(preamble);

            if (readTransaction.Key != key)
            {
                throw new InvalidOperationException($"Incorrect transaction read, " +
                    $"expected {key.ToString()} but found {readTransaction.Key.ToString()} ");
            }

            return readTransaction.IsCompleted == false;
        }

        public void RemoveOpenTransactionOnDisk(TransactionEntry transaction)
        {
            long offset = 0;

            if (!_map.HasTransaction(transaction.Key))
            {
                PopulateMap();
            }

            offset = _map.GetOffset(transaction.Key);

            // the first four bytes are the size of the transaction, so skip the size
            offset += Constants.SIZE_OF_INT;

            ReadOnlySpan<byte> preamble = GetPremableAtOffset(offset);

            var readTransaction = new TransactionEntry(preamble);

            if (readTransaction.Key != transaction.Key)
            {
                throw new InvalidOperationException($"Incorrect transaction read, " +
                    $"expected {transaction.Key.ToString()} but found {readTransaction.Key.ToString()} ");
            }

            offset += TransactionConstants.IsDeletedOffset(_V100);
            var bIsDeleted = DbBinaryConvert.BooleanToBinary(transaction.IsDeleted.ToString());

            _locker.EnterWriteLock();

            using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Inheritable | FileShare.ReadWrite | FileShare.Delete))
            using (var writer = new BinaryWriter(fileStream))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(bIsDeleted);
            }

            _locker.ExitWriteLock();


            throw new NotImplementedException();
        }

        public void LogCloseOpenTransaction(TransactionEntry transaction)
        {
            long offset = 0;

            if (!_map.HasTransaction(transaction.Key))
            {
                PopulateMap();
            }

            offset = _map.GetOffset(transaction.Key);

            // the first four bytes are the size of the transaction, so skip the size
            offset += Constants.SIZE_OF_INT;

            ReadOnlySpan<byte> preamble = GetPremableAtOffset(offset);

            var readTransaction = new TransactionEntry(preamble);

            if (readTransaction.Key != transaction.Key)
            {
                throw new InvalidOperationException($"Incorrect transaction read, " +
                    $"expected {transaction.Key.ToString()} but found {readTransaction.Key.ToString()} ");
            }

            offset += TransactionConstants.IsCompletedOffset(_V100);
            var bIsCompleted = DbBinaryConvert.BooleanToBinary(transaction.IsCompleted.ToString());

            _locker.EnterWriteLock();

            using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Inheritable | FileShare.ReadWrite | FileShare.Delete))
            using (var writer = new BinaryWriter(fileStream))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(bIsCompleted);
            }

            _locker.ExitWriteLock();
        }

        public void LogOpenTransactionToDisk(TransactionEntry transaction)
        {
            long offset;
            long order;

            GetTransactionOffsetAndOrder(transaction, out offset, out order);

            LogEntry(offset, transaction.BinaryData);
            SetFileSize();

            AddTransactionToMap(transaction, offset, order);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns the binary preamble at the specified offset
        /// </summary>
        /// <param name="offset">The offset from the start of the log file</param>
        /// <returns>The binary transaction preamble</returns>
        /// <remarks>When using this function, fast-forward past the 4 byte INT transaction size.</remarks>
        private ReadOnlySpan<byte> GetPremableAtOffset(long offset)
        {
            _locker.EnterReadLock();

            // do a quick sanity check to make sure we're at the correct location
            int preambleLength = TransactionConstants.TransactionPreambleSize(_V100);
            var bpreamble = new byte[preambleLength];

            using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Inheritable | FileShare.ReadWrite | FileShare.Delete))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                fileStream.Read(bpreamble, 0, preambleLength);
            }

            _locker.ExitReadLock();

            return new ReadOnlySpan<byte>(bpreamble);
        }

        private void AddTransactionToMap(TransactionEntry entry, long offset, long order)
        {
            if (_map.HasTransaction(entry.Key))
            {
                _map.UpdateOffset(entry, offset);
            }
            else
            {
                _map.AddItem(new TransactionItem(entry, offset, order));
            }
        }

        /// <summary>
        /// Writes the binary representation of the <seealso cref="TransactionEntry"/> to disk at the specified file offset (starting at the beginning of the file.)
        /// </summary>
        /// <param name="offset">The file offset starting at the beginning of the file</param>
        /// <param name="data">The binary representation of a <seealso cref="TransactionEntry"/></param>
        /// <remarks>This function will log an INT prefix of the size of the <seealso cref="TransactionEntry"/> before writing the actual bytes to disk. 
        /// Be sure to account for this when reading the transaction back from disk. The <paramref name="offset"/> is expected to include the INT prefix.</remarks>
        private void LogEntry(long offset, byte[] data)
        {
            byte[] bDataLength = DbBinaryConvert.IntToBinary(data.Length);

            _locker.EnterWriteLock();

            using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Inheritable | FileShare.ReadWrite | FileShare.Delete))
            using (var writer = new BinaryWriter(fileStream))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(bDataLength);

                offset += bDataLength.Length;

                fileStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(data);
            }

            _locker.ExitWriteLock();
        }

        private void CreateDbLogFile(string fileName)
        {
            using (var stream = File.Create(fileName))
            {
            }
        }

        /// <summary>
        /// Checks and populates the transaction map if needed for a record of the <seealso cref="TransactionEntry"/>. Returns the offset for the entry if an entry already
        /// exists, otherwise returns either 0 (for a new file) or the end of the file (for a new entry).
        /// </summary>
        /// <param name="entry">The entry to check in the map</param>
        /// <param name="offset">The file offset, starting at the beginning of the file. This will be the existing location if the transaction already exists, or 0 if the file is new, or the end of the file if the transaction hasn't been logged yet.</param>
        /// <param name="order">The order of the transaction in the file, starting at 0.</param>
        private void GetTransactionOffsetAndOrder(TransactionEntry entry, out long offset, out long order)
        {
            // we want to check the map to see if we already have a location
            // for this transaction

            // if the map hasn't been populated, then we need to fill the map
            // reading the file and either return the offset where the transaction alerady is
            // in the file, or return the first available offset, which may be at the end of the file

            if (_map.Count() == 0)
            {
                PopulateMap();
            }

            offset = _map.GetOffset(entry.Key);
            order = _map.GetOrder(entry.Key);

            // brand new log file
            if (_fileSize == 0 && offset == 0)
            {
                offset = 0;
                order = 0;
            }

            // we have a file, but this is a new transaction, so just add the transaction to the end of the file
            if (_fileSize > 0 && offset == 0)
            {
                offset = _fileSize;
                order = _map.MaxOrder() + 1;
            }
        }

        /// <summary>
        /// Parses the log file to populate the in-memory <seealso cref="TransactionItemMap"/>
        /// </summary>
        private void PopulateMap()
        {
            int order = 0;
            long currentOffset = 0;
            int preambleSize = TransactionConstants.TransactionPreambleSize(_V100);
            var file = new FileInfo(_fileName);
            long fileLength = file.Length;

            if (file.Length <= 0)
            {
                file = null;
                return;
            }

            byte[] fileData = new byte[fileLength];
            file = null;

            _locker.EnterReadLock();

            // read the entire file into a byte array
            using (var fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Inheritable | FileShare.ReadWrite | FileShare.Delete))
            {
                while (currentOffset < fileStream.Length)
                {
                    checked
                    {
                        fileStream.Read(fileData, 0, (int)fileLength);
                        currentOffset = fileStream.Length;
                    }
                }

                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }

            _locker.ExitReadLock();

            // use a span to parse the byte array
            var spanFileData = new ReadOnlySpan<byte>(fileData);

            int parseOffset = 0;

            while (parseOffset < spanFileData.Length)
            {
                // get the size of the record
                int recordSize = DbBinaryConvert.BinaryToInt(spanFileData.Slice(parseOffset, Constants.SIZE_OF_INT));

                parseOffset += Constants.SIZE_OF_INT;

                var fileEntry = new TransactionEntry(spanFileData.Slice(parseOffset, preambleSize));

                parseOffset += preambleSize;

                fileEntry.SetActionFromBinary(spanFileData.Slice(parseOffset, fileEntry.ActionBinaryLength));

                // set userName
                parseOffset += fileEntry.ActionBinaryLength;

                fileEntry.SetUserNameFromBinary(spanFileData.Slice(parseOffset, fileEntry.UserNameLength));
                parseOffset += fileEntry.UserNameLength;

                var mapEntry = new TransactionItem(fileEntry, parseOffset - preambleSize, order);

                if (!_map.HasTransaction(mapEntry.Key))
                {
                    _map.AddItem(mapEntry);
                    order++;
                }
            }
        }

        private void SetFileSize()
        {
            var file = new FileInfo(_fileName);
            _fileSize = file.Length;
        }

        #endregion

    }
}
