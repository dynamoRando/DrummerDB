using Drummersoft.DrummerDB.Core.Structures.Version;

namespace Drummersoft.DrummerDB.Core.Structures.Factory
{
    internal static class SystemSchemaFactory
    {
        public static SystemSchema100 GetSystemSchema100()
        {
            return new SystemSchema100();
        }
    }
}
