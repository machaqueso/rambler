using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Rambler.Web
{
    public class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.SQLite("./log.db")
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                var host = CreateWebHostBuilder(args).Build();
                host.RunAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Call additional providers here as needed.
                    // Call AddEnvironmentVariables last if you need to allow environment
                    // variables to override values from other providers.
                    config.AddEnvironmentVariables(prefix: "Authentication");
                })
                .UseSerilog((ctx, config) => { config.ReadFrom.Configuration(ctx.Configuration); })
                .UseStartup<Startup>();

        public static void Shutdown()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
