using Drummersoft.DrummerDB.Core.Structures;

namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    internal interface IDbLogFileWriter
    {
        void LogOpenTransactionToDisk(TransactionEntry transaction);
        void LogCloseOpenTransaction(TransactionEntry transaction);
    }
}
