using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Interface
{
    interface IAuthenticationManager
    {
        bool SystemHasLogin(string userName, string pw);
        public bool IsUserInSystemRole(string userName);
        public bool IsUserInSystemRole(string userName, Guid userGUID);
        public IEnumerable<SystemRole> GetSystemRolesForUser(string userName);
        public IEnumerable<SystemRole> GetSystemRolesForUser(string userName, Guid userGuid);
        public bool UserHasSystemPermission(string userName, SystemPermission permission);
        public bool ValidateLogin(string userName, string pwInput);

    }
}
