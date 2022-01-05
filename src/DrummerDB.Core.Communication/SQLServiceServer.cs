using Drummersoft.DrummerDB.Core.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Drummersoft.DrummerDB.Core.Communication
{
    internal class SQLServiceServer
    {
        private IHost _server;

        #region Public Methods
        public static void Run(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Starts a server with defaults, the supplied URLs, and the supplied processor
        /// </summary>
        /// <param name="args">Command line arguments for ASP.NET</param>
        /// <param name="url">The urls to host</param>
        /// <param name="processor">The message processor</param>
        public static void Run(string[] args, string[] url, SQLServiceHandler processor, PortSettings portSettings)
        {
            CreateHostBuilder(args, url, processor, portSettings).Build().Run();
        }

        public Task RunAsync(string[] args, string[] url, SQLServiceHandler processor, PortSettings portSettings)
        {
            if (_server is null)
            {
                _server = CreateHostBuilder(args, url, processor, portSettings).Build();
            }

            return _server.RunAsync();
        }

        public Task RunAsync(string[] args, string[] url, SQLServiceHandler processor, PortSettings portSettings, LogService logging)
        {
            if (_server is null)
            {
                _server = CreateHostBuilder(args, url, processor, portSettings, logging).Build();
            }

            return _server.RunAsync();
        }

        public static Task RunAsync(string[] args, SQLServiceHandler processor, PortSettings portSettings)
        {
            return CreateHostBuilder(args, processor, portSettings).Build().RunAsync();
        }

        public Task StopAsync()
        {
            return _server.StopAsync();
        }
        #endregion

        #region Private Methods
        private static IHostBuilder CreateHostBuilder(string[] args, SQLServiceHandler processor, PortSettings portSettings)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<SQLServiceStartup>();
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(portSettings.PortNumber, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                });
            }).ConfigureServices(
                foo =>
                {
                    foo.Add(new ServiceDescriptor(typeof(SQLServiceHandler), processor));
                });
        }

        private static IHostBuilder CreateHostBuilder(string[] args, string[] url, SQLServiceHandler processor, PortSettings portSettings)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<SQLServiceStartup>();
                webBuilder.UseUrls(url);
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(portSettings.PortNumber, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                });
            }).ConfigureServices(
                foo =>
                {
                    foo.Add(new ServiceDescriptor(typeof(SQLServiceHandler), processor));
                });
        }

        private static IHostBuilder CreateHostBuilder(string[] args, string[] url, SQLServiceHandler processor, PortSettings portSettings, LogService logging)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<SQLServiceStartup>();
                webBuilder.UseUrls(url);
                webBuilder.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(portSettings.PortNumber, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                });
            }).ConfigureServices(
                foo =>
                {
                    foo.Add(new ServiceDescriptor(typeof(SQLServiceHandler), processor));
                    foo.Add(new ServiceDescriptor(typeof(LogService), logging));
                });
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<SQLServiceStartup>();
                });
        #endregion
    }
}
