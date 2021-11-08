using System;

namespace Drummersoft.DrummerDB.Core.Structures.DbDebug
{
    internal class PageDebug
    {
        #region Private Fields
        private byte[] _data;
        private int _V100 = Constants.DatabaseVersions.V100;
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public PageDebug(byte[] data)
        {
            _data = data;
        }
        #endregion

        #region Public Methods
        public int PageId()
        {
            var idSpan = new ReadOnlySpan<byte>(_data);
            ReadOnlySpan<byte> idBytes = idSpan.Slice(PageConstants.PageIdOffset(), PageConstants.SIZE_OF_PAGE_ID(_V100));
            var result = BitConverter.ToInt32(idBytes);
            return result;
        }

        public string PageIdDebug()
        {
            return DebugSlice(PageConstants.PageIdOffset(), PageConstants.SIZE_OF_PAGE_ID(_V100));
        }

        public string DebugSlice(int index, int length)
        {
            var slice = new ReadOnlySpan<byte>(_data, index, length);
            return BitConverter.ToString(slice.ToArray());
        }

        public string RowDataDebug()
        {
            return DebugSlice(DataPageConstants.RowDataStartOffset(), Constants.PAGE_SIZE - DataPageConstants.RowDataStartOffset());
        }

        public string DebugData()
        {
            return BitConverter.ToString(_data);
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
