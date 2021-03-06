using Drummersoft.DrummerDB.Core.Storage.Abstract;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Tests.Mocks
{
    internal class MockStorageManager : IStorageManager
    {
        private TreeAddress _address;
        private List<MockDbFile> _files;
        private ITableSchema _schema;
        internal MockStorageManager()
        {
            _files = new List<MockDbFile>();
        }

        internal MockStorageManager(ITableSchema schema)
        {
            _files = new List<MockDbFile>();
            _schema = schema;
        }

        public void CreateSystemDatabase(string dbName, List<IPage> pages, DataFileType type, int version = 100)
        {
            throw new NotImplementedException();
        }

        public void CreateUserDatabase(string dbName, List<IPage> pages, DataFileType type, int version = Constants.MAX_DATABASE_VERSION)
        {
            throw new NotImplementedException();
        }

        public bool DeleteHostDatabase(string dbName)
        {
            throw new NotImplementedException();
        }

        public List<UserDataPage> GetAllUserDataPages(TreeAddress address)
        {
            throw new NotImplementedException();
        }

        public List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public IBaseDataPage GetAnyDataPage(int[] pagesInMemory, TreeAddress address, PageType type)
        {
            IBaseDataPage result = null;
            var file = _files.Where(f => f.Address == address).FirstOrDefault();

            if (!(file is null))
            {
                var intPageIds = new List<int>();
                foreach (var id in file.PageIds())
                {
                    intPageIds.Add((int)id);
                }

                int idToGet = intPageIds.Except(pagesInMemory).FirstOrDefault();
                if (idToGet != 0)
                {
                    result = file.Pages.Where(p => p.PageId() == idToGet && p.Type == type).FirstOrDefault();
                }
            }

            return result;
        }

        public UserDataPage GetAnySystemDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema)
        {
            throw new NotImplementedException();
        }

        public UserDataPage GetAnySystemDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema, uint tableId)
        {
            throw new NotImplementedException();
        }

        public UserDataPage GetAnySystemDataPage(uint[] pagesInMemory, TreeAddress address, ITableSchema schema, uint tableId)
        {
            throw new NotImplementedException();
        }

        public UserDataPage GetAnyUserDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema)
        {
            UserDataPage result = null;
            var file = _files.Where(f => f.Address == address).FirstOrDefault();

            if (!(file is null))
            {
                var intPageIds = new List<int>();
                foreach (var id in file.PageIds())
                {
                    intPageIds.Add((int)id);
                }
                int idToGet = intPageIds.Except(pagesInMemory).FirstOrDefault();
                if (idToGet != 0)
                {
                    result = file.Pages.Where(p => p.PageId() == idToGet).FirstOrDefault();
                }
            }

            return result;
        }

        public UserDataPage GetAnyUserDataPage(int[] pagesInMemory, TreeAddress address, ITableSchema schema, uint tableId)
        {
            UserDataPage result = null;
            var file = _files.Where(f => f.Address == address).FirstOrDefault();

            if (!(file is null))
            {
                var intPageIds = new List<int>();
                foreach (var id in file.PageIds())
                {
                    intPageIds.Add((int)id);
                }

                int idToGet = intPageIds.Except(pagesInMemory).FirstOrDefault();
                if (idToGet != 0)
                {
                    result = file.Pages.Where(p => p.PageId() == idToGet && p.TableId() == tableId).FirstOrDefault();
                }
            }

            return result;
        }

        public UserDataPage GetAnyUserDataPage(uint[] pagesInMemory, TreeAddress address, ITableSchema schema, uint tableId)
        {
            UserDataPage result = null;
            var file = _files.Where(f => f.Address == address).FirstOrDefault();

            var intPagesInMemory = new List<int>();
            foreach(var x in pagesInMemory)
            {
                intPagesInMemory.Add((int)x);
            }

            if (!(file is null))
            {
                var intPageIds = new List<int>();
                foreach (var id in file.PageIds())
                {
                    intPageIds.Add((int)id);
                }

                int idToGet = intPageIds.Except(intPagesInMemory).FirstOrDefault();
                if (idToGet != 0)
                {
                    result = file.Pages.Where(p => p.PageId() == idToGet && p.TableId() == tableId).FirstOrDefault();
                }
            }

            return result;
        }

        public uint GetMaxPageId(TreeAddress address)
        {
            uint result = 0;
            var item = _files.Where(f => f.Address == address).FirstOrDefault();
            if (!(item is null))
            {
                result = item.Pages.Select(i => i.PageId()).ToList().Max();
            }

            return result;
        }

        public SystemDbFileHandler GetSystemDatabaseFile(string dbName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSystemDatabaseNames()
        {
            throw new NotImplementedException();
        }

        public ISystemPage GetSystemPage(Guid dbId)
        {
            throw new NotImplementedException();
        }

        public ISystemPage GetSystemPageForSystemDatabase(string dbName)
        {
            throw new NotImplementedException();
        }

        public int GetTotalPages(TreeAddress address)
        {
            var item = _files.Where(f => f.Address == address).FirstOrDefault();
            if (item is null)
            {
                return 0;
            }
            else
            {
                return item.Pages.Count;
            }
        }

        uint IStorageManager.GetTotalPages(TreeAddress address)
        {
            var item = _files.Where(f => f.Address == address).FirstOrDefault();
            if (item is null)
            {
                return 0;
            }
            else
            {
                return (uint)item.Pages.Count;
            }
        }

        public List<UserDatabaseInformation> GetUserDatabasesInformation()
        {
            throw new NotImplementedException();
        }

        public bool HasSystemDatabaseFile(string dbName)
        {
            throw new NotImplementedException();
        }

        public bool IsSystemDatabase(Guid databaseId)
        {
            throw new NotImplementedException();
        }

        public bool IsUserDatabase(Guid databaseId)
        {
            return true;
        }

        public void LoadSystemDatabaseFilesIntoMemory()
        {
            throw new NotImplementedException();
        }

        public void LoadUserDatabaseFilesIntoMemory()
        {
            throw new NotImplementedException();
        }

        public void LogCloseTransaction(Guid databaseId, TransactionEntry transaction)
        {
            throw new NotImplementedException();
        }

        public bool LogFileHasOpenTransaction(Guid databaseId, TransactionEntryKey key)
        {
            throw new NotImplementedException();
        }

        public void LogOpenTransaction(Guid databaseId, TransactionEntry transaction)
        {
            throw new NotImplementedException();
        }

        public bool MarkPageAsDeleted(PageAddress address)
        {
            throw new NotImplementedException();
        }

        public void RemoveOpenTransaction(Guid databaseId, TransactionEntry transaction)
        {
            throw new NotImplementedException();
        }

        public bool SaveContractToDisk(Contract contract)
        {
            throw new NotImplementedException();
        }

        public void SavePageDataToDisk(PageAddress address, byte[] data, PageType type)
        {
            IBaseDataPage page = null;
            var treeAddress = new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId);
            var file = _files.Where(f => f.Address == treeAddress).FirstOrDefault();
            if (!(file is null))
            {
                page = file.Pages.Where(p => p.PageId() == address.PageId).FirstOrDefault();
                if (!(page is null))
                {
                    page = new UserDataPage100(data, _schema);
                }
                else
                {
                    file.Pages.Add(new UserDataPage100(data, _schema));
                }
            }
        }

        public void SavePageDataToDisk(PageAddress address, byte[] data, PageType type, DataPageType dataPageType)
        {
            IBaseDataPage page = null;
            var treeAddress = new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId);
            var file = _files.Where(f => f.Address == treeAddress).FirstOrDefault();
            if (!(file is null))
            {
                page = file.Pages.Where(p => p.PageId() == address.PageId).FirstOrDefault();
                if (!(page is null))
                {
                    page = new UserDataPage100(data, _schema);
                }
                else
                {
                    file.Pages.Add(new UserDataPage100(data, _schema));
                }
            }
        }

        public void SavePageDataToDisk(PageAddress address, byte[] data, PageType type, DataPageType dataPageType, bool isDeleted)
        {
            IBaseDataPage page = null;
            var treeAddress = new TreeAddress(address.DatabaseId, address.TableId, address.SchemaId);
            var file = _files.Where(f => f.Address == treeAddress).FirstOrDefault();
            if (!(file is null))
            {
                page = file.Pages.Where(p => p.PageId() == address.PageId).FirstOrDefault();
                if (!(page is null))
                {
                    page = new UserDataPage100(data, _schema);
                    if (isDeleted)
                    {
                        page.Delete();
                    }
                }
                else
                {
                    var x = new UserDataPage100(data, _schema);
                    if (isDeleted)
                    {
                        x.Delete();
                    }
                    file.Pages.Add(x);
                }
            }
        }

        public void SetAddress(TreeAddress address)
        {
            _address = address;

            var fakeFile = new MockDbFile { Address = address, Pages = new List<UserDataPage>() };
            fakeFile.Pages.Add(new UserDataPage100(new PageAddress
            (
                address.DatabaseId,
                address.TableId,
                1,
                _schema.Schema.SchemaGUID
            ), _schema));

            _files.Add(fakeFile);
        }
        public int TotalSystemDatabasesOnDisk()
        {
            throw new NotImplementedException();
        }

        public int TotalUserDatabasesOnDisk()
        {
            throw new NotImplementedException();
        }
    }
}
