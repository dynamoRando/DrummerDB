using Drummersoft.DrummerDB.Core.Databases.Interface;
using Drummersoft.DrummerDB.Core.IdentityAccess.Interface;
using Drummersoft.DrummerDB.Core.QueryTransaction.Interface;
using Drummersoft.DrummerDB.Core.Storage.Interface;
using Drummersoft.DrummerDB.Core.Structures;

namespace Drummersoft.DrummerDB.Core.Communication.Interface
{
    internal interface INetworkManager
    {
        public void StartServerForSQLService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, IQueryManager queryManager);
        public void StartServerForSQLService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, int portNumber, IQueryManager queryManager);
        public void StartServerForInfoService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager);
        public void StartServerForInfoService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, int portNumber);

        /// <summary>
        /// Starts the Database Service with the supplied parameters
        /// </summary>
        /// <param name="useHttps">If the connection should use HTTPS or not</param>
        /// <param name="authenticationManager">An instance of an auth manager</param>
        /// <param name="dbManager">An instance of a db manager</param>
        /// <param name="cache">An instance of a cache manager</param>
        /// <param name="crypt">An instance of a crypt manager</param>
        /// <remarks>The managers passed in are normally used in the creation of a new database</remarks>
        public void StartServerForDatabaseService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, IStorageManager storage);

        /// <summary>
        /// Starts the Database Service with the supplied parameters. Overrides the port number from what was loaded in settings.
        /// </summary>
        /// <param name="useHttps">If the connection should use HTTPS or not</param>
        /// <param name="authenticationManager">An instnace of an auth manager</param>
        /// <param name="dbManager">An instnace of a db manager</param>
        /// <param name="cache">An instance of a cache manager</param>
        /// <param name="crypt">An instance of a crypt manager</param>
        /// <param name="portNumber">The port number the server should listen on. This value overrides what is in the settings file.</param>
        /// <param name="storage">An instance of the storage manager (used to save pending contracts)</param>
        public void StartServerForDatabaseService(bool useHttps, IAuthenticationManager authenticationManager, IDbManager dbManager, int portNumber, IStorageManager storage);
        public void StopServerForSQLService();
        public void StopServerForInfoService();
        public void StopServerForDatabaseService();
        public void SetHostInfo(HostInfo info);
    }
}
