using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Drummersoft.DrummerDB.Core.Structures.Version.SystemSchemaConstants100.Maps;

namespace Drummersoft.DrummerDB.Core.Storage.Version
{
    /// <summary>
    /// Reads a data file (of version 100) for various properties. 
    /// </summary>
    /// <remarks>Use this object sparingly, to only get the basic file items to get initial constructor populated. 
    /// Leverage the call chain via <seealso cref="ICacheManager"/> for most data file actions.</remarks>
    internal class DbDataFileReader100 : IDbDataFileReader
    {
        #region Private Fields
        private int _V100 = Constants.DatabaseVersions.V100;
        private string _fileName;
        private string _dbName;
        private DateTime _createdDate;
        private Guid _databaseId;
        #endregion

        #region Public Properties
        public string DatabaseName => _dbName;
        public DateTime CreatedDate => _createdDate;
        public Guid DatabaseId => _databaseId;
        #endregion

        #region Constructors
        public DbDataFileReader100(string fileName)
        {
            _fileName = fileName;
            SetFields();
        }
        #endregion

        #region Public Methods

        public List<UserDataPage> GetAllUserDataPages(TreeAddress address, ITableSchema schema)
        {
            var result = new List<UserDataPage>();
            var items = GetPageItems(_fileName);

            foreach (var item in items)
            {
                if (item.DataPageType != DataPageType.NotApplicable || item.DataPageType != DataPageType.Unknown)
                {
                    if (item.TableId == address.TableId)
                    {
                        result.Add(GetUserDataPageAtPosition(item.Offset, schema));
                    }
                }
            }

            return result;
        }

        public List<PageItem> GetPageItems(string fileName)
        {
            var result = new List<PageItem>();

            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position = 0;
            var fi = new FileInfo(fileName);
            var totalFileLength = fi.Length;
            int order = 0;

            bool quit = false;

            while (!quit)
            {
                if (position >= totalFileLength)
                {
                    break;
                }

                using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    binaryReader.BaseStream.Position = position;
                    binaryReader.Read(span);
                }

                int iPageId = DbBinaryConvert.BinaryToInt(span.Slice(PageConstants.PageIdOffset(), PageConstants.SIZE_OF_PAGE_ID(Constants.DatabaseVersions.V100)));
                int iPageType = DbBinaryConvert.BinaryToInt(span.Slice(PageConstants.PageTypeOffset(), PageConstants.SIZE_OF_PAGE_TYPE(Constants.DatabaseVersions.V100)));

                var pageType = (PageType)iPageType;

                if (pageType is PageType.Data)
                {
                    var iPageDataType = DbBinaryConvert.BinaryToInt(span.Slice(DataPageConstants.DataPageTypeOffset(Constants.DatabaseVersions.V100),
                        DataPageConstants.SIZE_OF_DATA_PAGE_TYPE(Constants.DatabaseVersions.V100)));

                    var dataPageType = (DataPageType)iPageDataType;

                    if (dataPageType is DataPageType.User)
                    {
                        var iDbId = DbBinaryConvert.BinaryToGuid(span.Slice(DataPageConstants.DatabaseIdOffset(Constants.DatabaseVersions.V100), DataPageConstants.SIZE_OF_DATABASE_ID(Constants.DatabaseVersions.V100)));
                        var iTableId = DbBinaryConvert.BinaryToInt(span.Slice(DataPageConstants.TableIdOffset(Constants.DatabaseVersions.V100), DataPageConstants.SIZE_OF_TABLE_ID(Constants.DatabaseVersions.V100)));

                        var pageItem = new PageItem(iPageId, PageType.Data, dataPageType, order, iTableId, position);

                        if (!result.Contains(pageItem))
                        {
                            result.Add(pageItem);

                            order++;
                            position = order * Constants.PAGE_SIZE;
                        }
                        else
                        {
                            throw new InvalidOperationException("Duplicate page on disk");

                            //order++;
                            //position = order * Constants.PAGE_SIZE;
                        }
                    }
                    else
                    {
                        var iDbId = DbBinaryConvert.BinaryToGuid(span.Slice(DataPageConstants.DatabaseIdOffset(Constants.DatabaseVersions.V100), DataPageConstants.SIZE_OF_DATABASE_ID(Constants.DatabaseVersions.V100)));
                        var iTableId = DbBinaryConvert.BinaryToInt(span.Slice(DataPageConstants.TableIdOffset(Constants.DatabaseVersions.V100), DataPageConstants.SIZE_OF_TABLE_ID(Constants.DatabaseVersions.V100)));


                        var pageItem = new PageItem(iPageId, pageType, dataPageType, order, iTableId, position);
                        result.Add(pageItem);

                        order++;
                        position = order * Constants.PAGE_SIZE;
                    }
                }
                else
                {
                    // is the first system page
                    var pageItem = new PageItem(iPageId, pageType, DataPageType.NotApplicable, order, 0, position);
                    result.Add(pageItem);

                    order++;
                    position = order * Constants.PAGE_SIZE;
                }
            }

            return result;
        }


        public UserDataPageSearchResult GetAnyUserDataPage(string fileName, ITableSchema schema, PageAddress[] pagesInMemory, int tableId)
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position = 0;
            int order = 1;
            UserDataPageSearchResult result = null;

            var fi = new FileInfo(fileName);
            var totalFileLength = fi.Length;

            bool quit = false;

            while (!quit)
            {
                if (position >= totalFileLength)
                {
                    break;
                }

                using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    binaryReader.BaseStream.Position = position;
                    binaryReader.Read(span);
                }

                int iPageId = DbBinaryConvert.BinaryToInt(span.Slice(PageConstants.PageIdOffset(), PageConstants.SIZE_OF_PAGE_ID(Constants.DatabaseVersions.V100)));
                int iPageType = DbBinaryConvert.BinaryToInt(span.Slice(PageConstants.PageTypeOffset(), PageConstants.SIZE_OF_PAGE_TYPE(Constants.DatabaseVersions.V100)));

                var pageType = (PageType)iPageType;

                if (pageType is PageType.Data)
                {
                    var iPageDataType = DbBinaryConvert.BinaryToInt(span.Slice(DataPageConstants.DataPageTypeOffset(Constants.DatabaseVersions.V100),
                        DataPageConstants.SIZE_OF_DATA_PAGE_TYPE(Constants.DatabaseVersions.V100)));

                    var dataPageType = (DataPageType)iPageDataType;

                    if (dataPageType is DataPageType.User)
                    {
                        var iDbId = DbBinaryConvert.BinaryToGuid(span.Slice(DataPageConstants.DatabaseIdOffset(Constants.DatabaseVersions.V100), DataPageConstants.SIZE_OF_DATABASE_ID(Constants.DatabaseVersions.V100)));
                        var iTableId = DbBinaryConvert.BinaryToInt(span.Slice(DataPageConstants.TableIdOffset(Constants.DatabaseVersions.V100), DataPageConstants.SIZE_OF_TABLE_ID(Constants.DatabaseVersions.V100)));

                        var address = new PageAddress(iDbId, iTableId, iPageId, schema.Schema.SchemaGUID);

                        if (!pagesInMemory.Contains(address))
                        {
                            // then this is a page we want;
                            if (iTableId == tableId)
                            {
                                result = new UserDataPageSearchResult();
                                result.UserDataPage = new UserDataPage100(span.ToArray(), schema);
                                result.Order = order;
                                result.Offset = position;
                                quit = true;
                            }
                            else
                            {
                                // ?? just keep going ??
                                // we wind up in this state because the page/table we are looking for may not be saved to disk yet
                                order++;
                                position = order * Constants.PAGE_SIZE;
                            }
                        }

                    }
                    else
                    {
                        order++;
                        position = order * Constants.PAGE_SIZE;
                    }
                }
                else
                {
                    order++;
                    position = order * Constants.PAGE_SIZE;
                }
            }

            return result;
        }

        public static string GetDatabaseName(string fileName)
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position;
            string result = string.Empty;

            using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                position = binaryReader.Read(span);
            }

            if (position == Constants.PAGE_SIZE)
            {
                result = DbBinaryConvert.BinaryToString(span.Slice(SystemPageConstants.DatabaseNameOffset(Constants.DatabaseVersions.V100), SystemPageConstants.SIZE_OF_DATABASE_NAME(Constants.DatabaseVersions.V100)));
            }

            return result;
        }

        public static DateTime GetCreatedDate(string fileName)
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position;
            DateTime result = new DateTime();

            using (var binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                position = binaryReader.Read(span);
            }

            if (position == Constants.PAGE_SIZE)
            {
                result = DbBinaryConvert.BinaryToDateTime(span.Slice(SystemPageConstants.CreatedDateOffset(Constants.DatabaseVersions.V100), SystemPageConstants.SIZE_OF_CREATED_DATE(Constants.DatabaseVersions.V100)));
            }

            return result;
        }
        #endregion

        #region Private Methods
        private UserDataPage100 GetUserDataPageAtPosition(int position, ITableSchema schema)
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);

            using (var binaryReader = new BinaryReader(File.Open(_fileName, FileMode.Open)))
            {
                binaryReader.BaseStream.Position = position;
                binaryReader.Read(span);
            }

            return new UserDataPage100(span.ToArray(), schema);
        }

        private void SetFields()
        {
            byte[] data = new byte[Constants.PAGE_SIZE];
            var span = new Span<byte>(data);
            int position;
            string result = string.Empty;

            using (var binaryReader = new BinaryReader(File.Open(_fileName, FileMode.Open)))
            {
                position = binaryReader.Read(span);
            }

            if (position == Constants.PAGE_SIZE)
            {
                _dbName = DbBinaryConvert.BinaryToString(span.Slice(SystemPageConstants.DatabaseNameOffset(_V100), SystemPageConstants.SIZE_OF_DATABASE_NAME(_V100))).Trim();
                _createdDate = DbBinaryConvert.BinaryToDateTime(span.Slice(SystemPageConstants.CreatedDateOffset(_V100), SystemPageConstants.SIZE_OF_CREATED_DATE(_V100)));
                _databaseId = DbBinaryConvert.BinaryToGuid(span.Slice(SystemPageConstants.DatabaseIdOffset(_V100), SystemPageConstants.SIZE_OF_DATABASE_ID(_V100)));
            }
        }
        #endregion
    }
}
