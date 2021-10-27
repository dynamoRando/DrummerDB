using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Memory.Interface
{
    internal interface IMemory
    {
        void AddSystemDbSystemPage(ISystemPage page);
        void AddUserDbSystemPage(ISystemPage page);
        int GetMaxRowIdForTree(TreeAddress address);

        TreeStatus GetTreeMemoryStatus(TreeAddress address);

        TreeStatus GetTreeSizeStatus(TreeAddress address, int sizeOfDataToAdd);
        bool HasUserDataAddress(TreeAddress address);

        bool HasUserDataPage(PageAddress address);
        void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address);
        void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly);
        void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address);
        void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendlyName);
        int[] UserDataGetContainerPages(TreeAddress address);
        IBaseDataPage UserDataGetPage(PageAddress address);
        bool UserSystemCacheHasDatabase(Guid dbId);

    }
}
