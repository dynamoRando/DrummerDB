using Drummersoft.DrummerDB.Common;
using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.Structures.Enum;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.IdentityAccess
{
    internal class AuthenticationManager : IAuthenticationManager
    {
        #region Private Fields
        // managers
        private readonly IDbManager _dbs;

        // internal objects
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        public AuthenticationManager(IDbManager dbs)
        {
            _dbs = dbs;
        }

        #endregion

        #region Public Methods
        public bool SystemHasLogin(string userName, string pw)
        {
            return _dbs.GetSystemDatabase().ValidateLogin(userName, pw);
        }

        public bool IsUserInSystemRole(string userName)
        {
            return _dbs.GetSystemDatabase().IsUserInSystemRole(userName);
        }

        public bool IsUserInSystemRole(string userName, Guid userGUID)
        {
            return _dbs.GetSystemDatabase().IsUserInSystemRole(userName);
        }

        public IEnumerable<SystemRole> GetSystemRolesForUser(string userName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SystemRole> GetSystemRolesForUser(string userName, Guid userGuid)
        {
            throw new NotImplementedException();
        }

        public bool UserHasSystemPermission(string userName, SystemPermission permission)
        {
            return _dbs.GetSystemDatabase().UserHasSystemPermission(userName, permission);
        }

        public bool ValidateLogin(string userName, string pwInput)
        {
            return _dbs.GetSystemDatabase().ValidateLogin(userName, pwInput);
        }

        public bool UserHasDbPermission(string userName, string pw, string dbName, DbPermission permission, Guid objectId)
        {
            if (SystemHasLogin(userName, pw))
            {
                if (IsUserInSystemRole(userName))
                {
                    if (UserHasSystemPermission(userName, SystemPermission.FullAccess))
                    {
                        return true;
                    }
                }
                else
                {
                    var db = _dbs.GetUserDatabase(dbName, DatabaseType.Host);
                    if (db.HasUser(userName))
                    {
                        return db.AuthorizeUser(userName, pw, permission, objectId);
                    }
                }
            }

            return false;
        }

        public void SetInitalSystemAdmin(string initialSystemLogin, string intialSystemPw)
        {
            var systemDb = _dbs.GetSystemDatabase();
            systemDb.AddLogin(initialSystemLogin, intialSystemPw, Guid.NewGuid(), true);
            systemDb.AssignUserToDefaultSystemAdmin(initialSystemLogin);
        }

        #endregion

        #region Private Methods
        #endregion

    }
}
