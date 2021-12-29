using Drummersoft.DrummerDB.Core.Memory.Enum;
using Drummersoft.DrummerDB.Core.Structures;
using Drummersoft.DrummerDB.Core.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Memory.Interface
{
    internal interface IMemory
    {
        void AddSystemDbSystemPage(ISystemPage page);
        void AddUserDbSystemPage(ISystemPage page);
        uint GetMaxRowIdForTree(TreeAddress address);

        TreeStatus GetTreeMemoryStatus(TreeAddress address);

        TreeStatus GetTreeSizeStatus(TreeAddress address, uint sizeOfDataToAdd);
        bool HasUserDataAddress(TreeAddress address);

        bool HasUserDataPage(PageAddress address);
        void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address);
        void UserDataAddIntitalData(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendly);
        void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address);
        void UserDataAddPageToContainer(IBaseDataPage page, TreeAddress address, TreeAddressFriendly friendlyName);
        int[] UserDataGetContainerPages(TreeAddress address);
        IBaseDataPage UserDataGetPage(PageAddress address);
        bool UserSystemCacheHasDatabase(Guid dbId);
        List<PageAddress> GetPageAddressesForTree(TreeAddress address);

    }
}
