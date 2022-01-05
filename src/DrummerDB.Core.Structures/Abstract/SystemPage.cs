using Drummersoft.DrummerDB.Core.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Structures.Abstract
{
    internal abstract class SystemPage : IPage, ISystemPage
    {
        public abstract byte[] Data { get; }
        public abstract PageType Type { get; }
        public abstract ushort DatabaseVersion { get; }
        public abstract string DatabaseName { get; }
        public abstract Guid DatabaseId { get; }

        public abstract uint PageId();
        public abstract void SetDatabaseName(string databaseName);
        public abstract uint GetMaxSystemDataPage();
        public abstract void SetMaxSystemDataPage(int max);
        /// <summary>
        /// Returns the number of bytes from the beginning of the file to the first system page offset
        /// </summary>
        /// <returns>The number of bytes until the first system page offset</returns>
        public abstract uint FirstSystemDataPageOffset();
        public abstract bool IsDeleted();
    }
}
