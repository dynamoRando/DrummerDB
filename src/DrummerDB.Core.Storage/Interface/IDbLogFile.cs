using Drummersoft.DrummerDB.Core.Structures;

namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    internal interface IDbLogFile
    {
        void DeleteFromDisk();
        void LogCloseOpenTransaction(TransactionEntry transaction);
        bool LogFileHasOpenTransaction(TransactionEntryKey key);
        void LogOpenTransactionToDisk(TransactionEntry transaction);
        void RemoveOpenTransactionOnDisk(TransactionEntry transaction);
    }
}
