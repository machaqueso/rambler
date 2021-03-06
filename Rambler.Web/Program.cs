﻿using System.Diagnostics;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Threading;
using Serilog.Events;

namespace Rambler.Web
{
    public class Program
    {
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
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Error)
                .WriteTo.Console()
                .WriteTo.File(configuration["Logging:FilePath"], rollingInterval: RollingInterval.Day)
                .WriteTo.SQLite(configuration["ConnectionStrings:DefaultConnection"].Replace("Filename=",string.Empty))
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
    }
}
