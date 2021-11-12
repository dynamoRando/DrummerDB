using Drummersoft.DrummerDB.Core.Structures.Enum;

namespace Drummersoft.DrummerDB.Core.Structures.Interface
{
    internal interface IPage
    {
        byte[] Data { get; }
        int PageId();
        PageType Type { get; }
        bool IsDeleted();
    }
}
