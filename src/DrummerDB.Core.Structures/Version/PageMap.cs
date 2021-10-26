using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Drummersoft.DrummerDB.Core.Structures.Version
{
    internal partial class SystemSchemaConstants100
    {
        internal static partial class Maps
        {
            /// <summary>
            /// An object used to help map page locations on disk
            /// </summary>
            internal class PageMap
            {
                #region Private Fields
                int _maxItem = 0;
                ConcurrentBag<PageItem> _items;
                #endregion

                #region Public Properties
                public ConcurrentBag<PageItem> Items => _items;
                #endregion

                #region Constructors
                public PageMap()
                {
                    _items = new ConcurrentBag<PageItem>();
                }
                #endregion

                #region Public Methods
                public void Clear()
                {
                    _items.Clear();
                    _maxItem = 0;
                }

                public int TotalPages()
                {
                    return _items.Count();
                }

                public int TotalPages(TreeAddress address)
                {
                    int totalPages = 0;

                    foreach (var item in _items)
                    {
                        if (item.TableId == address.TableId)
                        {
                            totalPages++;
                        }
                    }

                    return totalPages;
                }

                public void AddItem(PageItem item)
                {
                    _maxItem++;
                    item.Order = _maxItem;
                    _items.Add(item);
                }

                public bool HasItem(PageItem item)
                {
                    return _items.Any(i => i.Equals(item));
                }

                public bool HasTable(int tableId)
                {
                    return _items.Any(i => i.TableId == tableId);
                }

                public bool HasPage(int pageId, PageType type)
                {
                    foreach (var item in _items)
                    {
                        if (item.Type == type && item.PageId == pageId)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public bool HasPage(int pageId, int tableId, PageType type)
                {
                    foreach (var item in _items)
                    {
                        if (item.PageId == pageId && item.Type == type && item.TableId == tableId)
                        {
                            return true;
                        }
                    }

                    return false;

                }

                public int GetOffset(int pageId, int tableId, PageType type)
                {
                    int result = 0;
                    PageItem item = null;

                    foreach (var i in _items)
                    {
                        if (i.PageId == pageId && i.Type == type && i.TableId == tableId)
                        {
                            item = i;
                            break;
                        }
                    }

                    if (item is not null)
                    {
                        result = item.Offset;
                    }

                    return result;
                }

                public int GetMaxOrder()
                {
                    int result = 0;

                    foreach (var item in _items)
                    {
                        if (item.Order > result)
                        {
                            result = item.Order;
                        }
                    }
                    return result;
                }

                /// <summary>
                /// Returns the total byte size of the page map. Used when storing the page map to the System page.
                /// </summary>
                /// <returns></returns>
                public int Size()
                {
                    throw new NotImplementedException();
                }
                #endregion

                #region Private Methods
                #endregion
            }
        }
    }
}
