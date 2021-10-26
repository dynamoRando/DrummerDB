using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.DatabaseService;
using Drummersoft.DrummerDB.Core.IdentityAccess.Structures.Enum;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Common.Communication.DatabaseService.DatabaseService;

namespace Drummersoft.DrummerDB.Core.Communication
{
    internal class DrummerDatabaseService : DatabaseServiceBase
    {
        private readonly ILogger<DrummerDatabaseService> _logger;
        private readonly DatabaseServiceHandler _handler;

        public DrummerDatabaseService(ILogger<DrummerDatabaseService> logger, DatabaseServiceHandler handler)
        {
            _handler = handler;
        }

        public override Task<TestReply> IsOnline(TestRequest request, ServerCallContext context)
        {
            _handler.HandleTest();
            var reply = new TestReply();
            reply.ReplyTimeUTC = DateTime.UtcNow.ToString();
            reply.ReplyEchoMessage = request.RequestEchoMessage;
            return Task.FromResult(reply);
        }

        public override Task<AuthResult> IsLoginValid(AuthRequest request, ServerCallContext context)
        {
            AuthResult result = null;
            bool hasLogin = _handler.SystemHasLogin(request.UserName, request.Pw);
            if (hasLogin)
            {
                result = new AuthResult();
                result.IsAuthenticated = true;
                result.AuthenticationMessage = "Login exists at system level";
                result.UserName = request.UserName;
            }

            return Task.FromResult(result);
        }

        public override Task<CreateDatabaseResult> CreateUserDatabase(CreateDatabaseRequest request, ServerCallContext context)
        {
            var hasLogin = IsLoginValid(request.Authentication, context);
            var result = new CreateDatabaseResult();

            if (hasLogin.Result.IsAuthenticated)
            {
                result.AuthenticationResult = hasLogin.Result;

                if (_handler.UserHasSystemPermission(request.Authentication.UserName, SystemPermission.CreateDatabase))
                {
                    Guid databaseId;
                    result.IsSuccessful = _handler.CreateUserDatabase(request.DatabaseName, out databaseId);

                    result.DatabaseName = request.DatabaseName;
                    result.DatabaseId = databaseId.ToString();
                }
                else
                {
                    result.IsSuccessful = false;
                    result.ResultMessage = $"{request.Authentication.UserName} does not have permission to create databases.";
                }
            }
            else
            {
                result.AuthenticationResult = hasLogin.Result;
            }

            return Task.FromResult(result);
        }
    }
}
