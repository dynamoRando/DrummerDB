using Drummersoft.DrummerDB.Core.Systems;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DrummerDB.Core.Service
{
    public class DrummerCoreService : BackgroundService
    {
        private readonly ILogger<DrummerCoreService> _logger;
        private Process _drummerProcess;

        public DrummerCoreService(ILogger<DrummerCoreService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    StartupProcess();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private void StartupProcess()
        {
            if (_drummerProcess is null)
            {
                _drummerProcess = new Process();

                _logger.LogInformation("DrummerDB starting...");
                _drummerProcess.Start();
                _logger.LogInformation("DrummerDB starting SQL endpoint...");
                _drummerProcess.StartSQLServer();
                _logger.LogInformation("DrummerDB starting Info endpoint...");
                _drummerProcess.StartInfoServer();
                _logger.LogInformation("DrummerDB starting Db endpoint...");
                _drummerProcess.StartDbServer();

                _logger.LogInformation("DrummerDB has started.");
            }
        }
    }
}
