using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rambler.Data;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Test
{
    public class DependencySetupFixture
    {
        public DependencySetupFixture()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IConfiguration>(sp =>
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.AddJsonFile("appsettings.json");
                return configurationBuilder.Build();
            });

            serviceCollection.AddDbContext<DataContext>(options => options.UseInMemoryDatabase(databaseName: "TestDatabase"));

            serviceCollection.AddTransient<UserService>();
            serviceCollection.AddTransient<YoutubeService>();
            serviceCollection.AddTransient<ChatService>();
            serviceCollection.AddTransient<DashboardService>();
            serviceCollection.AddTransient<TwitchService>();
            serviceCollection.AddTransient<ConfigurationService>();
            serviceCollection.AddTransient<IntegrationService>();
            serviceCollection.AddTransient<ChannelService>();
            serviceCollection.AddTransient<AccountService>();
            serviceCollection.AddTransient<PasswordService>();
            serviceCollection.AddTransient<BotService>();

            serviceCollection.AddTransient<TwitchAPIv5>();
            serviceCollection.AddTransient<TwitchManager>();
            serviceCollection.AddTransient<AuthorService>();
            serviceCollection.AddTransient<WordFilterService>();
            serviceCollection.AddTransient<ChatRulesService>();

            serviceCollection.AddSingleton<IHostedService, YoutubeBackgroundService>();
            serviceCollection.AddSingleton<IHostedService, TwitchBackgroundService>();
            serviceCollection.AddSingleton<IntegrationManager>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}