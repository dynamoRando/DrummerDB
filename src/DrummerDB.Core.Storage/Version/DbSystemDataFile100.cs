using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Factory;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Factory;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Maps;

namespace Drummersoft.DrummerDB.Core.Storage.Version
{

    internal class DbSystemDataFile100 : DbSystemDataFile
    {
        #region Private Fields
        private string _fileName;
        private PageMap _map;
        private int _V100 = Constants.DatabaseVersions.V100;
        private Guid _dbId;
        private string _dbName;
        private IDbDataFileReader _reader;
        private IDbDataFileWriter _writer;
        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        #endregion

        #region Public Properties
        public override Guid DbId => _dbId;
        public override int Version => _V100;
        public override string DatabaseName => _dbName;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a system data file on disk 
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <param name="pages">The inital data pages to be saved</param>
        /// <param name="dbId">The database id</param>
        /// <param name="cache">A reference to cache</param>
        /// <param name="crypt">A reference to crypt</param>
        /// <param name="dbName">The name of the database</param
        /// <param name="dbManager">A reference to db manager</param>
        /// <remarks>Use this constructor when making a new object to be saved to disk</remarks>
        public DbSystemDataFile100(string fileName, List<IPage> pages, string dbName)
        {
            _fileName = fileName;
            _dbName = dbName;

            _map = new PageMap();
            int order = 0;

            _locker.EnterWriteLock();
            if (!File.Exists(fileName))
            {

                int totalLength = 0;
                pages.ForEach(page => totalLength += page.Data.Length);

                var allBytes = new byte[totalLength];

                int offset = 0;
                pages.ForEach(page =>
                {
                    Array.Copy(page.Data, 0, allBytes, offset, page.Data.Length);
                    offset += page.Data.Length;
                });

                int runningLength = 0;
                pages.ForEach(page =>
                {
                    order++;

                    if (page.Type == PageType.Data)
                    {
                        if (page is SystemDataPage)
                        {
                            var pData = page as SystemDataPage;
                            var pDataType = pData.DataPageType();
                            runningLength += page.Data.Length;
                            var item = new PageItem(page.PageId(), page.Type, pDataType, order, pData.TableId(), runningLength);
                            _map.AddItem(item);
                        }

                        if (page is UserDataPage)
                        {
                            var pData = page as UserDataPage;
                            var pDataType = pData.DataPageType();
                            runningLength += page.Data.Length;
                            var item = new PageItem(page.PageId(), page.Type, pDataType, order, pData.TableId(), runningLength);
                            _map.AddItem(item);
                        }
                    }
                    else
                    {
                        runningLength += page.Data.Length;
                        var item = new PageItem(page.PageId(), page.Type, DataPageType.Unknown, order, 0, runningLength);
                        _map.AddItem(item);
                    }
                });

                // should probably use a DbFileWriter here instead
                File.WriteAllBytes(fileName, allBytes);

                _reader = DbDataFileReaderFactory.GetDbDataFileReader(_fileName, _V100);
                _writer = DbDataFileWriterFactory.GetDbDataFileWriter(_fileName, _V100);
            }
            _locker.ExitWriteLock();
        }

        /// <summary>
        /// Creates a new instance of a system data file and sets up the metadata
        /// </summary>
        /// <param name="fileName">The path to the file</param>
        /// <param name="cache">A reference to the Cache Manager</param>
        /// <param name="dbId">The name of the database</param>
        /// <param name="crypt">A reference to the Crypto manager</param>
        /// <remarks>Use this constructor when initalizing a Process</remarks>
        public DbSystemDataFile100(string fileName, Guid dbId)
        {
            _fileName = fileName;
            _dbId = dbId;
            _map = new PageMap();
            _reader = DbDataFileReaderFactory.GetDbDataFileReader(_fileName, _V100);
            _writer = DbDataFileWriterFactory.GetDbDataFileWriter(_fileName, _V100);
            _dbName = DbDataFileReader100.GetDatabaseName(_fileName).Trim();
        }
        #endregion

        #region Public Methods
        public override void DeleteFromDisk()
        {
            File.Delete(_fileName);
        }

        public override UserDataPage GetAnyUserDataPage(PageAddress[] pagesInMemory, ITableSchema schema, int tableId)
        {
            UserDataPage result = null;

            if (pagesInMemory.Length > 0)
            {
                // see if we have the page in our page map
                foreach (var page in pagesInMemory)
                {
                    // if it doesn't have it, go add it to our map from disk
                    if (!_map.HasPage(page.PageId, PageType.Data))
                    {

                        _locker.EnterReadLock();
                        var searchResult = _reader.GetAnyUserDataPage(_fileName, schema, pagesInMemory, tableId);
                        _locker.ExitReadLock();

                        result = searchResult.UserDataPage;
                        var item = new PageItem(searchResult.UserDataPage.PageId(), PageType.Data, DataPageType.User, searchResult.Order, searchResult.UserDataPage.TableId(), searchResult.Offset);
                        _map.AddItem(item);
                    }
                }
            }
            else
            {
                // we don't have anything in memory, go get something from disk
                _locker.EnterReadLock();
                var searchResult = _reader.GetAnyUserDataPage(_fileName, schema, pagesInMemory, tableId);
                _locker.ExitReadLock();

                if (searchResult is not null)
                {
                    result = searchResult.UserDataPage;
                    var item = new PageItem(searchResult.UserDataPage.PageId(), PageType.Data, DataPageType.User, searchResult.Order, searchResult.UserDataPage.TableId(), searchResult.Offset);
                    if (!_map.HasItem(item))
                    {
                        _map.AddItem(item);
                    }
                }
            }

            return result;
        }

        public override int GetMaxPageId(in TreeAddress address)
        {
            int maxPageId = 0;

            foreach (var item in _map.Items)
            {
                if (item.TableId == address.TableId)
                {
                    if (item.PageId > maxPageId)
                    {
                        maxPageId = item.PageId;
                    }
                }
            }

            return maxPageId;
        }

        public override ISystemPage GetSystemPage()
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position;

            _locker.EnterReadLock();
            using (var binaryReader = new BinaryReader(File.Open(_fileName, FileMode.Open)))
            {
                position = binaryReader.Read(span);
            }
            _locker.ExitReadLock();

            ISystemPage page = SystemPageFactory.GetSystemPage100(span.ToArray());

            return page;
        }

        public override int GetTotalPages()
        {
            return _map.TotalPages();
        }

        public override int GetTotalPages(TreeAddress address)
        {
            return _map.TotalPages(address);
        }

        public override UserDataPage GetUserDataPage(int id)
        {
            throw new NotImplementedException();
        }

        public override void WritePageToDisk(byte[] pageData, PageAddress address, PageType type, DataPageType dataPageType)
        {
            long offset = 0;
            if (_map.HasPage(address.PageId, address.TableId, type))
            {
                offset = _map.GetOffset(address.PageId, address.TableId, type);
            }
            else
            {
                var info = new FileInfo(_fileName);
                offset = info.Length;
            }

            _locker.EnterWriteLock();
            _writer.WritePageToDisk(_fileName, offset, pageData);
            _locker.ExitWriteLock();

            if (!_map.HasPage(address.PageId, address.TableId, type))
            {
                checked
                {
                    var pageItem = new PageItem(address.PageId, type, dataPageType, _map.GetMaxOrder() + 1, address.TableId, (int)offset);
                    _map.AddItem(pageItem);
                }
            }
        }

        public override List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema)
        {
            _locker.EnterReadLock();
            var result = _reader.GetAllUserDataPages(address, schema);
            _locker.ExitReadLock();
            return result;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
