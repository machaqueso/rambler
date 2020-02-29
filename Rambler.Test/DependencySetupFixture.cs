using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Data;

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

            serviceCollection.AddDbContext<DataContext>(options =>
                options.UseSqlite("DataSource=:memory:"));

            Rambler.Web.DependencyInjection.ConfigureDependencies(serviceCollection);
            Rambler.Data.DependencyInjection.ConfigureDependencies(serviceCollection);
            Rambler.Services.DependencyInjection.ConfigureDependencies(serviceCollection);

            serviceCollection.AddLogging();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}