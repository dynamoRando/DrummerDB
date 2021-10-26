using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Interface;
using System;
using System.Collections.Generic;

namespace Drummersoft.DrummerDB.Core.IdentityAccess.Structures
{
    public sealed class SystemRole : IRole
    {
        #region Private Fields
        #endregion

        #region Public Properties
        public readonly Guid Id;
        public readonly string Name;
        public readonly List<SystemPermission> Permisisons;
        #endregion

        #region Constructors
        public SystemRole(string name, List<SystemPermission> permissions)
        {
            Permisisons = permissions;
            Name = name;
            Id = Guid.NewGuid();

        }

        public SystemRole(Guid id, string name, List<SystemPermission> permissions)
        {
            Permisisons = permissions;
            Name = name;
            Id = id;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
