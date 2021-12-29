using C5;
using Drummersoft.DrummerDB.Core.Diagnostics;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Drummersoft.DrummerDB.Core.Memory
{
    /// <summary>
    /// An implementation of a ITreeContainer. Holds the pages for a tree (a database table) and allows read/write access of them in a thread safe manner. 
    /// </summary>
    internal class TreeContainer
    {
        #region Private Fields
        private TreeAddress _address;
        private TreeDictionary<uint, IBaseDataPage> _tree;
        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private LogService _log;
        #endregion

        #region Public Properties
        public TreeAddress Address => _address;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new container with an initial page
        /// </summary>
        /// <param name="address">The address of the container</param>
        /// <param name="page">The first page of the container</param>
        public TreeContainer(TreeAddress address, IBaseDataPage page)
        {
            _address = address;
            _tree = new TreeDictionary<uint, IBaseDataPage>();
            _tree.Add(page.PageId(), page);
        }

        public TreeContainer(TreeAddress address, IBaseDataPage page, LogService log)
        {
            _address = address;
            _tree = new TreeDictionary<uint, IBaseDataPage>();
            _tree.Add(page.PageId(), page);
            _log = log;
        }
        #endregion

        #region Public Methods
        public RowType GetRowType(uint rowId, uint pageId)
        {
            RowType type = RowType.Unknown;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);

            IBaseDataPage page = _tree.Values.Where(page => page.PageId() == pageId).FirstOrDefault();
            if (page is not null)
            {
                if (page.HasRow(rowId))
                {
                    var row = page.GetRow(rowId);
                    type = row.Type;
                }
            }

            _locker.ExitReadLock();

            return type;
        }

        /// <summary>
        /// Returns all the pages that have have either the row on them, or have a reference to the row but is forwarded
        /// </summary>
        /// <param name="rowId">The row id to find</param>
        /// <returns>An array of pageIds that have a reference to the row</returns>
        /// <remarks>This function will enter a read lock on the tree</remarks>
        public uint[] GetPageReferencesToRow(uint rowId)
        {
            List<uint> pages = new List<uint>();

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);

            foreach (var page in _tree.Values)
            {
                PageRowStatus status = page.GetRowStatus(rowId);
                if (status == PageRowStatus.IsForwardedToOtherPage || status == PageRowStatus.IsOnPageAndForwardedOnSamePage
                    || status == PageRowStatus.IsOnPage)
                {
                    pages.Add(page.PageId());
                }
            }

            _locker.ExitReadLock();

            uint[] result = pages.Distinct().ToArray();

            return result;
        }

        /// <summary>
        /// Returns the page id of the row (absolute location, not where the row has been forwarded)
        /// </summary>
        /// <param name="rowId">The row id to find</param>
        /// <returns>The page id where the row is currently located</returns>
        /// <remarks>This function will enter a read lock on the tree</remarks>
        public uint GetPageIdOfRow(uint rowId)
        {
            uint result = 0;
            uint[] pageIds = GetPageReferencesToRow(rowId);

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            foreach (var pageId in pageIds)
            {
                IBaseDataPage page = _tree.Values.Where(page => page.PageId() == pageId).FirstOrDefault();
                if (page is not null)
                {
                    if (page.HasRow(rowId))
                    {
                        result = page.PageId();
                    }
                }
            }

            _locker.ExitReadLock();

            return result;
        }

        /// <summary>
        /// Will return the row offsets on the specified page
        /// </summary>
        /// <param name="rowId">The row id to find</param>
        /// <param name="pageId">The page id to search</param>
        /// <returns>The offsets of the row on the page</returns>
        /// <remarks>This function will enter a read lock on the tree</remarks>
        public List<uint> GetRowOffsets(uint rowId, uint pageId)
        {
            var result = new List<uint>();

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);

            IBaseDataPage page = _tree.Values.Where(page => page.PageId() == pageId).FirstOrDefault();
            if (page is not null)
            {
                if (page.HasRow(rowId))
                {
                    result = page.GetRowOffsets(rowId);
                }
            }

            _locker.ExitReadLock();

            return result;
        }

        /// <summary>
        /// Attempts to update the specified row in this container. Returns the PageId of the page where the row was updated. This function will lock the container.
        /// </summary>
        /// <param name="row">The row to update</param>
        /// <returns>The Page Id where the row was updated.</returns>
        /// <remarks>This function tries to account for row forwards and when a page we're updating is full. This function will write lock the container. For more information on these concepts, see Page.md and Row.md</remarks>
        public uint UpdateRow(Row row)
        {
            if (_log is not null)
            {
                var sw = Stopwatch.StartNew();
                sw.Start();
                var result = Update(row);
                sw.Stop();
                _log.Performance(LogService.GetCurrentMethod(), sw.ElapsedMilliseconds);
                return result;
            }
            else
            {
                return Update(row);
            }
        }

        /// <summary>
        /// Attempts to add the row to this container. Returns the PageId where the row was added.
        /// </summary>
        /// <param name="row">The row to add</param>
        /// <returns>the PageId wher the row was added</returns>
        /// <remarks>This function will write lock the container.</remarks>
        public uint AddRow(Row row)
        {
            uint pageId = 0;

            _locker.TryEnterWriteLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);

            foreach (var page in _tree.Values)
            {
                if (!page.IsFull(row.TotalSize))
                {
                    page.AddRow(row);
                    pageId = page.PageId();
                }
            }

            _locker.ExitWriteLock();

            return pageId;
        }

        public bool DeleteRow(uint rowId)
        {
            uint pageId = 0;
            bool result = false;

            pageId = GetPageIdOfRow(rowId);
            if (pageId != 0)
            {
                _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
                IBaseDataPage page = _tree.Values.Where(page => page.PageId() == pageId).FirstOrDefault();
                if (page is not null)
                {
                    page.DeleteRow(rowId);
                    result = true;
                }
                _locker.ExitReadLock();
            }

            return result;
        }

        /// <summary>
        /// Checks to see if the container is full if we try to add a row of the specified size. Used to see if we needed to add another page from disk.
        /// </summary>
        /// <param name="rowSize">The size of the row to check</param>
        /// <returns>True if the container is full, otherwise false</returns>
        /// <remarks>This function will read lock the container.</remarks>
        public bool IsTreeFull(uint rowSize)
        {
            bool isFull = false;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            isFull = _tree.Values.All(page => page.IsFull(rowSize));
            _locker.ExitReadLock();

            return isFull;
        }

        /// <summary>
        /// Returns an array of PageIds in the tree (container).
        /// </summary>
        /// <returns>The PageIds in this ocntainer</returns>
        /// <remarks>This function will read lock the container.</remarks>
        public uint[] Pages()
        {
            uint[] pages;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            pages = _tree.Keys.ToArray();
            _locker.ExitReadLock();

            return pages;
        }

        /// <summary>
        /// Checks to see if there are no pages loaded in this tree.
        /// </summary>
        /// <returns>True if the tree is empty, otherwise false</returns>
        /// <remarks>This function will read lock the container.</remarks>
        public bool IsEmpty()
        {
            int count = 0;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            count = _tree.Keys.Count;
            _locker.ExitReadLock();

            return count == 0;
        }

        /// <summary>
        /// Returns the total number of pages on the tree.
        /// </summary>
        /// <returns>The number of pages on the tree</returns>
        /// <remarks>this function will read lock the container.</remarks>
        public int PageCount()
        {
            int count = 0;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            count = _tree.Keys.Count;
            _locker.ExitReadLock();

            return count;
        }

        /// <summary>
        /// Attempts to add a page to the tree
        /// </summary>
        /// <param name="page">The page to add</param>
        /// <remarks>This function will write lock the container.</remarks>
        public void AddPage(IBaseDataPage page)
        {
            _locker.TryEnterWriteLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            _tree.Add(page.PageId(), page);
            _locker.ExitWriteLock();
        }

        /// <summary>
        /// Returns a list of Page Addresses currently in the tree.
        /// </summary>
        /// <returns>A list of Page Addresses in the tree</returns>
        /// <remarks>This function will read lock the tree.</remarks>
        public PageAddress[] PageAddresses()
        {
            var list = new List<PageAddress>(_tree.Keys.Count);

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            foreach (var key in _tree.Keys)
            {
                list.Add(new PageAddress(_address.DatabaseId, _address.TableId, key, _address.SchemaId));
            }
            _locker.ExitReadLock();

            return list.ToArray();
        }

        /// <summary>
        /// Determines if the specified page id is in the tree
        /// </summary>
        /// <param name="pageId">The page id to find</param>
        /// <returns>True if the page is in the tree, otherwise false</returns>
        /// <remarks>This function will read lock the tree.</remarks>
        public bool HasPage(uint pageId)
        {
            bool hasPage = false;

            IBaseDataPage page;
            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            hasPage = _tree.Find(ref pageId, out page);
            _locker.ExitReadLock();

            return hasPage;
        }

        /// <summary>
        /// Attempts to get the specified page from the tree
        /// </summary>
        /// <param name="pageId">The page to get</param>
        /// <returns>The specified page</returns>
        /// <remarks>This function will read lock the tree. Note that generally speaking for thread safety we want to keep I/O for pages contained to the tree. This function might need to be removed. It is intended only for getting page data to save to disk.</remarks>
        public IBaseDataPage GetPage(uint pageId)
        {
            IBaseDataPage page;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            _tree.Find(ref pageId, out page);
            _locker.ExitReadLock();

            return page;
        }

        /// <summary>
        /// Returns the row specified. If not found, <c>NULL</c>.
        /// </summary>
        /// <param name="rowId">The row identifier.</param>
        /// <returns>The row with the specified id if found, otherwise <c>NULL</c></returns>
        /// <remarks>Note that this will return the row even if the row was marked as <see cref="IRow.IsDeleted"/></remarks>
        public Row GetRow(uint rowId)
        {
            Row result = null;

            _locker.TryEnterReadLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);
            foreach (var page in _tree.Values)
            {
                result = page.GetRow(rowId);
                if (!(result is null))
                {
                    if (result.IsForwarded && result.ForwardedPageId != page.PageId())
                    {
                        var actualPage = _tree.Values.Where(t => t.PageId() == result.ForwardedPageId).FirstOrDefault();
                        if (actualPage is not null)
                        {
                            result = actualPage.GetRow(rowId);
                            break;
                        }
                    }
                    break;
                }
            }
            _locker.ExitReadLock();

            return result;
        }
        #endregion

        #region Private Methods
        private uint Update(Row row)
        {
            _locker.TryEnterWriteLock(Constants.READ_WRITE_LOCK_TIMEOUT_MILLISECONDS);

            uint pageId = 0;
            uint rowId = row.Id;
            uint newRowOffset = 0;
            IBaseDataPage newPage = null;
            IBaseDataPage page = null;
            PageUpdateRowResult updateResult = PageUpdateRowResult.Unknown;

            // Figure out the status of our row. Is it currently on our page but forwarded? Is it forwarded to a different page?

            // if the row is on our page, but forwarded (has been updated at least once)
            var pages = _tree.Values.Where(p => p.GetRowStatus(rowId) == PageRowStatus.IsOnPageAndForwardedOnSamePage).ToList();

            if (pages.Count > 1)
            {
                throw new InvalidOperationException("Tree has an updated row that somehow exists on multiple pages");
            }
            else if (pages.Count == 1)
            {
                // the row exists on on the page, but has been forwarded at least once
                page = pages.First();
                var currentPageId = page.PageId();

                // try to update the row on the page
                updateResult = page.TryUpdateRowData(row, out _);

                // if the attempted update failed because there's not enough room on the page
                if (updateResult == PageUpdateRowResult.NotEnoughRoom)
                {
                    // need to find another page with enough room to update with the updated row data, then mark the existing rows as forwarded on this page
                    newPage = _tree.Values.Where(p => !p.IsFull(row.Size()) && p.PageId() != page.PageId()).FirstOrDefault();
                    if (newPage is not null)
                    {
                        uint newPageId = newPage.PageId();

                        // add the row to the new page
                        newRowOffset = newPage.AddRow(row);

                        // then go back and mark the previous rows as forwarded to this new page
                        pages.ForEach(p =>
                        {
                            string debug = $"Forwarding row {rowId.ToString()} to new page {newPageId.ToString()} at offset {newRowOffset.ToString()}";
                            Debug.WriteLine(debug);

                            p.ForwardRows(rowId, newPageId, newRowOffset);
                        });
                        pageId = newPage.PageId();

                        _locker.ExitWriteLock();
                        return pageId;
                    }
                }

                // good to go
                else if (updateResult == PageUpdateRowResult.Success)
                {
                    pageId = page.PageId();

                    _locker.ExitWriteLock();
                    return pageId;
                }
            }

            // if the row has been forward to other pages
            pages = _tree.Values.Where(p => p.GetRowStatus(rowId) == PageRowStatus.IsForwardedToOtherPage).ToList();

            // if we have pages where the row has been forwarded
            if (pages.Count >= 1)
            {
                // need to get the actual page with the row and see if we can update the row on that page (if there is room). So first, need to grab any of the forwarded rows for the page id of the 
                // page that actually contains the data
                var anyPage = pages.FirstOrDefault();
                if (anyPage is not null)
                {
                    var currentPage = anyPage.PageId();
                    var forwardedRow = anyPage.GetRow(rowId);

                    // sanity check, it should be forwarded
                    if (forwardedRow.IsForwarded)
                    {
                        uint pageIdToGet = forwardedRow.ForwardedPageId;

                        // get the actual page with the row
                        IBaseDataPage actualPage = _tree.Values.Where(p => p.PageId() == pageIdToGet).FirstOrDefault();

                        if (actualPage is not null)
                        {
                            uint actualPageId = actualPage.PageId();
                            uint newOffset = 0;

                            PageUpdateRowResult attemptToUpdate = actualPage.TryUpdateRowData(row, out newOffset);
                            if (attemptToUpdate == PageUpdateRowResult.Success)
                            {
                                // we need to go back and update all the forwards with the correct offset for the correct page
                                uint newPageId = actualPage.PageId();
                                newRowOffset = newOffset;

                                pages.ForEach(page =>
                                {
                                    string debug = $"Forwarding row {rowId.ToString()} to new page {newPageId.ToString()} at offset {newRowOffset.ToString()}";
                                    Debug.WriteLine(debug);

                                    page.ForwardRows(rowId, newPageId, newRowOffset);
                                });

                                _locker.ExitWriteLock();
                                return newPageId;
                            }

                            // there's not enough room on the actual page with the row
                            else if (attemptToUpdate == PageUpdateRowResult.NotEnoughRoom)
                            {
                                // need to find a page with room for the update, then go back and update all the other pages with the correct forwarded page id and offset
                                newPage = _tree.Values.Where(p => !p.IsFull(row.Size()) && p.PageId() != actualPage.PageId()).FirstOrDefault();
                                if (newPage is not null)
                                {
                                    uint newPageId = newPage.PageId();
                                    newRowOffset = newPage.AddRow(row);

                                    pages.ForEach(p =>
                                    {
                                        string debug = $"Forwarding row {rowId.ToString()} to new page {newPageId.ToString()} at offset {newRowOffset.ToString()}";
                                        Debug.WriteLine(debug);

                                        p.ForwardRows(rowId, newPageId, newRowOffset);
                                    });

                                    actualPage.ForwardRows(rowId, newPageId, newRowOffset);
                                    pageId = newPage.PageId();

                                    _locker.ExitWriteLock();
                                    return pageId;
                                }
                            }

                        }
                    }
                }
            }

            // if the row is on the page and has never been forwarded (updated)
            page = _tree.Values.Where(p => p.GetRowStatus(rowId) == PageRowStatus.IsOnPage).ToList().FirstOrDefault();

            newRowOffset = 0;
            updateResult = page.TryUpdateRowData(row, out newRowOffset);

            // if there's not enough room on the page for this update
            if (updateResult == PageUpdateRowResult.NotEnoughRoom)
            {
                // grab any page (that's not the current page) that isn't full and can fit our row size
                newPage = _tree.Values.Where(p => !p.IsFull(row.Size()) && p.PageId() != page.PageId()).FirstOrDefault();
                if (newPage is not null)
                {
                    newRowOffset = newPage.AddRow(row);
                    page.ForwardRows(rowId, newPage.PageId(), newRowOffset);
                    pageId = newPage.PageId();

                    _locker.ExitWriteLock();
                    return pageId;
                }
            }

            // we're good to go
            else if (updateResult == PageUpdateRowResult.Success)
            {
                pageId = page.PageId();

                _locker.ExitWriteLock();
                return pageId;
            }

            _locker.ExitWriteLock();
            return pageId;
        }
        #endregion

    }
}
