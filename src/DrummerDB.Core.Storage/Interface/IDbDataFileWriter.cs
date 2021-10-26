namespace Drummersoft.DrummerDB.Core.Storage.Interface
{
    internal interface IDbDataFileWriter
    {
        void WritePageToDisk(string fileName, long offset, byte[] pageData);
    }
}
