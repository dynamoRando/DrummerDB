namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    /// <summary>
    /// Allows for the transaction data to be saved in binary format
    /// </summary>
    internal interface ITransactionBinary
    {
        byte[] GetDataInTransactionBinaryFormat();
    }
}
