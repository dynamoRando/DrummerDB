using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using System;

namespace Drummersoft.DrummerDB.Core.Storage
{
    /// <summary>
    /// Used to read pages from a database file
    /// </summary>
    internal class DbFilePageReader : IDbFilePageReader
    {
        #region Private Fields
        private string _fileName = string.Empty;
        #endregion

        #region Public Properties
        public int FileOffset { get; set; }
        #endregion

        #region Constructors
        internal DbFilePageReader(string fileName)
        {
            _fileName = fileName;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the next available page that is not already in memory
        /// </summary>
        /// <param name="pagesInMemory">A list of pages already in memory</param>
        /// <returns>The next page found on disk that is not already in memory</returns>
        UserDataPage IDbFilePageReader.GetAnyUserDataPage(PageAddress[] pagesInMemory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the specified page from disk
        /// </summary>
        /// <param name="id">The page to get from disk</param>
        /// <returns>The specified page</returns>
        UserDataPage IDbFilePageReader.GetUserDataPage(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
