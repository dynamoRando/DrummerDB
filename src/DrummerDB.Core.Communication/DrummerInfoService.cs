using Drummersoft.DrummerDB.Common.Communication;
using Drummersoft.DrummerDB.Common.Communication.InfoService;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Drummersoft.DrummerDB.Common.Communication.InfoService.InfoService;

namespace Drummersoft.DrummerDB.Core.Communication
{
    internal class DrummerInfoService : InfoServiceBase
    {
        private readonly ILogger<DrummerInfoService> _logger;
        private readonly InfoServiceHandler _handler;

        public DrummerInfoService(ILogger<DrummerInfoService> logger, InfoServiceHandler handler)
        {
            _handler = handler;
        }

        public override Task<ProcessStatisticsReply> GetProcessStatistics(ProcessStatisticsRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
            return base.GetProcessStatistics(request, context);
        }

        public override Task<TestReply> IsOnline(TestRequest request, ServerCallContext context)
        {
            _handler.HandleTest();
            var reply = new TestReply();
            reply.ReplyTimeUTC = DateTime.UtcNow.ToString();
            reply.ReplyEchoMessage = request.RequestEchoMessage;
            return Task.FromResult(reply);

        }
    }
}
