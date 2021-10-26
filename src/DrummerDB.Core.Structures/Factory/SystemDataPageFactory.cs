using Drummersoft.DrummerDB.Core.Structures.Abstract;
using Drummersoft.DrummerDB.Core.Structures.Version;

namespace Drummersoft.DrummerDB.Core.Structures.Factory
{
    internal static class SystemDataPageFactory
    {
        public static SystemDataPage GetSystemDataPage100(byte[] data, SystemTableSchema schema)
        {
            return new SystemDataPage100(data, schema);
        }

        public static SystemDataPage GetSystemDataPage100(PageAddress address, SystemTableSchema schema)
        {
            return new SystemDataPage100(address, schema);
        }
    }
}
