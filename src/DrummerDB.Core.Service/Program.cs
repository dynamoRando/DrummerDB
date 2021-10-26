using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DrummerDB.Core.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args).Build();
            RunProgram(builder);
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                return Host.CreateDefaultBuilder(args)
                      .UseWindowsService(options =>
                        {
                            options.ServiceName = "DrummerDB";
                        })
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddLogging();
                            services.AddHostedService<DrummerCoreService>();
                        });
            }
            else
            {
                return Host.CreateDefaultBuilder(args).
                       UseSystemd()
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddLogging();
                            services.AddHostedService<DrummerCoreService>();
                        });
            }
        }


        private static async void RunProgram(IHost host)
        {
            //await host.RunAsync();
            //await host.StartAsync();
            host.Run();
        }
    }
}
