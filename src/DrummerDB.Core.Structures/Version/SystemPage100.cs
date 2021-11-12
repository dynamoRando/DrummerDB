using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    /// <summary>
    /// A page containing database information
    /// </summary>
    internal class SystemPage100 : SystemPage
    {
        #region Private Fields
        private byte[] _data;
        private string _databaseName;
        private int _databaseVersion;
        private int _pageId = 0;
        private DateTime _createdDate;
        private int _V100 = Constants.DatabaseVersions.V100;
        private Guid _databaseId;
        #endregion

        #region Public Properties
        public override byte[] Data => _data;
        public override PageType Type => PageType.System;
        public override int DatabaseVersion => _databaseVersion;
        public override string DatabaseName => GetDbName();
        public override Guid DatabaseId => _databaseId;
        #endregion

        #region Constructors
        /// <summary>
        /// Make a new System Page based on data from disk and attempts to set attributes from binary data
        /// </summary>
        /// <param name="data">The byte array data from disk</param>
        public SystemPage100(byte[] data)
        {
            _data = data;

            SetDbName();
            SetDbId();
        }

        /// <summary>
        /// Make a new System Page in memory and sets binary data to supplied values
        /// </summary>
        /// <param name="databaseName">The name of the database. The maximum length for a database name is 30 characters.</param>
        /// <param name="dbId">The database Id</param>
        public SystemPage100(string databaseName, DataFileType dataFileType, Guid dbId)
        {
            _databaseVersion = _V100;
            _data = new byte[SystemPageConstants.SIZE_OF_SYSTEM_PAGE(_V100)];
            _databaseId = dbId;
            _createdDate = DateTime.Now;

            SavePageTypeToData();
            SavePageIdToData();
            SaveDatabaseVersionToData();
            SaveCreatedDateToData();
            SetDatabaseName(databaseName);
            SetMaxSystemDataPage(0);
            SaveDataFileType(dataFileType);
            SaveDbId();
        }
        #endregion

        #region Public Methods
        public override bool IsDeleted()
        {
            var span = new ReadOnlySpan<byte>(_data);
            bool isDeleted = DbBinaryConvert.BinaryToBoolean(span.Slice(PageConstants.PageIsDeletedOffset(_V100), PageConstants.SIZE_OF_IS_DELETED(_V100)));
            return isDeleted;
        }

        /// <summary>
        /// Returns the maximum System Data Page in this database, based on the System Page's Data
        /// </summary>
        /// <returns>The max Meta Page Id</returns>
        public override int GetMaxSystemDataPage()
        {
            var span = new ReadOnlySpan<byte>(_data);
            int maxPage = DbBinaryConvert.BinaryToInt(span.Slice(SystemPageConstants.MaxSystemDataPageOffset(_V100), SystemPageConstants.SIZE_OF_MAX_SYSTEM_DATA_PAGE(_V100)));
            return maxPage;
        }

        /// <summary>
        /// Saves the specified max Meta Page Id to the System Page's Data
        /// </summary>
        /// <param name="max">The max Meta Page Id</param>
        public override void SetMaxSystemDataPage(int maxId)
        {
            var bMaxArray = DbBinaryConvert.IntToBinary(maxId);
            Array.Copy(bMaxArray, 0, _data, SystemPageConstants.MaxSystemDataPageOffset(_V100), bMaxArray.Length);
        }

        /// <summary>
        /// Validates the supplied database name and if valid, will set to Page's Data
        /// </summary>
        /// <param name="databaseName">The databaseName to set. The maximum length for a database name is 30 characters.</param>
        /// <remarks>Can use this function to rename the database if needed</remarks>
        public override void SetDatabaseName(string databaseName)
        {
            if (IsValidLength(databaseName))
            {
                _databaseName = PadDbName(databaseName);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Supplied Name {databaseName} is greater than limit {SystemPageConstants.MAX_LENGTH_DB_NAME(_V100).ToString()}");
            }

            SaveDbNameToData();
        }

        public override int PageId()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            var idBytes = idSpan.Slice(PageConstants.PageIdOffset(), PageConstants.SIZE_OF_PAGE_ID(_V100));
            var result = BitConverter.ToInt32(idBytes);
            return result;
        }

        public override int FirstSystemDataPageOffset()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void SaveDataFileType(DataFileType dataFileType)
        {
            var bType = DbBinaryConvert.IntToBinary((int)dataFileType);
            Array.Copy(bType, 0, _data, SystemPageConstants.DataFileTypeOffset(_V100), bType.Length);
        }

        private void SaveDatabaseVersionToData()
        {
            var bVersion = DbBinaryConvert.IntToBinary(_databaseVersion);
            Array.Copy(bVersion, 0, _data, SystemPageConstants.DatabaseVersionOffset(_V100), bVersion.Length);
        }
        private void SavePageTypeToData()
        {
            var bType = DbBinaryConvert.IntToBinary((int)Type);
            Array.Copy(bType, 0, _data, PageConstants.PageTypeOffset(), bType.Length);
        }

        /// <summary>
        /// Saves the Page's Id to the Page's data
        /// </summary>
        private void SavePageIdToData()
        {
            var bId = BitConverter.GetBytes(_pageId);
            bId.CopyTo(_data, PageConstants.PageIdOffset());
        }

        /// <summary>
        /// Checks to make sure that the supplied name is not longer than the max length allowed for a database name
        /// </summary>
        /// <param name="dbName">The db name to check</param>
        /// <returns>True if valid, otherwise false</returns>
        private bool IsValidLength(string dbName)
        {
            if (dbName.Length > SystemPageConstants.MAX_LENGTH_DB_NAME(_V100))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SetDbName()
        {
            var span = new ReadOnlySpan<byte>(_data);
            _databaseName = DbBinaryConvert.BinaryToString(span.Slice(SystemPageConstants.DatabaseNameOffset(_V100), SystemPageConstants.SIZE_OF_DATABASE_NAME(_V100)));
        }

        private void SetDbId()
        {
            var span = new ReadOnlySpan<byte>(_data);
            _databaseId = DbBinaryConvert.BinaryToGuid(span.Slice(SystemPageConstants.DatabaseIdOffset(_V100), SystemPageConstants.SIZE_OF_DATABASE_ID(_V100)));
        }

        private void SaveDbNameToData()
        {
            var bName = DbBinaryConvert.StringToBinary(_databaseName);
            Array.Copy(bName, 0, _data, SystemPageConstants.DatabaseNameOffset(_V100), bName.Length);
        }

        private string PadDbName(string dbName)
        {
            return dbName.PadRight(SystemPageConstants.MAX_LENGTH_DB_NAME(_V100));
        }

        private string GetDbName()
        {
            return _databaseName.Trim();
        }

        private void SaveCreatedDateToData()
        {
            var bCreatedDate = DbBinaryConvert.DateTimeToBinary(_createdDate.ToString());
            Array.Copy(bCreatedDate, 0, _data, SystemPageConstants.CreatedDateOffset(_V100), bCreatedDate.Length);
        }

        private void SaveDbId()
        {
            var array = DbBinaryConvert.GuidToBinary(_databaseId);
            Array.Copy(array, 0, _data, SystemPageConstants.DatabaseIdOffset(_V100), array.Length);
        }
        #endregion
    }
}
