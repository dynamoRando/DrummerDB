using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.Tests.Mocks
{
    public class MockAuthenticationManager : IAuthenticationManager
    {
        public IEnumerable<SystemRole> GetSystemRolesForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SystemRole> GetSystemRolesForUser(string userName, Guid userGuid)
        {
            throw new NotImplementedException();
        }

        public bool IsUserInSystemRole(string userName)
        {
            throw new NotImplementedException();
        }

        public bool IsUserInSystemRole(string userName, Guid userGUID)
        {
            throw new NotImplementedException();
        }

        public void SetInitalSystemAdmin(string initialSystemLogin, string intialSystemPw)
        {
            throw new NotImplementedException();
        }

        public bool SystemHasLogin(string userName, string pw)
        {
            throw new NotImplementedException();
        }

        public bool UserHasDbPermission(string userName, string pw, string dbName, DbPermission permission, Guid objectId)
        {
            throw new NotImplementedException();
        }

        public bool UserHasSystemPermission(string userName, SystemPermission permission)
        {
            throw new NotImplementedException();
        }

        public bool ValidateLogin(string userName, string pwInput)
        {
            throw new NotImplementedException();
        }
    }
}
