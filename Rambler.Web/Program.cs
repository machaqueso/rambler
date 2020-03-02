using System.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Threading;

namespace Rambler.Web
{
    public class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(configuration["Logging:FilePath"], rollingInterval: RollingInterval.Day)
                .WriteTo.SQLite(configuration["ConnectionStrings:DefaultConnection"])
                .CreateLogger();

            Log.Information($"Base path: {Directory.GetCurrentDirectory()}");

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables(prefix: "Authentication");
                    config.AddConfiguration(configuration);
                })
                .UseStartup<Startup>();

        }

        public static void Shutdown()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
