using Drummersoft.DrummerDB.Core.Storage.Interface;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// Used to read user information from a database file
    /// </summary>
    internal class DbFileUserReader : IDbFileUserReader
    {
        #region Private Fields
        private string _fileName = string.Empty;
        #endregion

        #region Public Properties
        public int FileOffset { get; set; }
        #endregion

        #region Constructors
        public DbFileUserReader(string fileName)
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
