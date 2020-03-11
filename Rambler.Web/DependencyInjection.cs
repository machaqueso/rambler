using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web
{
    public class DependencyInjection
    {
        public static void ConfigureDependencies(IServiceCollection services)
        {
            services.AddTransient<UserService>();
            services.AddTransient<YoutubeService>();
            services.AddTransient<ChatProcessor>();
            services.AddTransient<DashboardService>();
            services.AddTransient<TwitchService>();
            services.AddTransient<IntegrationService>();

            services.AddSingleton<IntegrationManager>();
        }

    }
}
