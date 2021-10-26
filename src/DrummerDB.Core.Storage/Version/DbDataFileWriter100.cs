using Drummersoft.DrummerDB.Core.Storage.Interface;
using System.IO;

namespace Drummersoft.DrummerDB.Core.Storage.Version
{
    internal class DbDataFileWriter100 : IDbDataFileWriter
    {

        #region Private Fields
        private string _fileName;
        #endregion

        #region Public Properties
        public void WritePageToDisk(string fileName, long offset, byte[] pageData)
        {
            using (var binaryWriter = new BinaryWriter(File.Open(fileName, FileMode.Open, FileAccess.ReadWrite)))
            {
                int location = 0;

                // TODO: .. not sure about this.
                checked
                {
                    location = (int)offset;
                }

                binaryWriter.Seek(location, SeekOrigin.Begin);
                binaryWriter.Write(pageData);
                binaryWriter.Close();
            }
        }

        #endregion

        #region Constructors
        public DbDataFileWriter100(string fileName)
        {
            _fileName = fileName;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
