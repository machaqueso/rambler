using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Rambler.IntegrationTest
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //builder.ConfigureServices(services =>
            //{
            //    // Create a new service provider.
            //    var serviceProvider = new ServiceCollection()
            //        .AddEntityFrameworkInMemoryDatabase()
            //        .BuildServiceProvider();

            //    // Add a database context (ApplicationDbContext) using an in-memory 
            //    // database for testing.
            //    services.AddDbContext<DataContext>(options =>
            //    {
            //        options.UseInMemoryDatabase("InMemoryDbForTesting");
            //        options.UseInternalServiceProvider(serviceProvider);
            //    });

            //    // Build the service provider.
            //    var sp = services.BuildServiceProvider();

            //    // Create a scope to obtain a reference to the database
            //    // context (ApplicationDbContext).
            //    using (var scope = sp.CreateScope())
            //    {
            //        var scopedServices = scope.ServiceProvider;
            //        var db = scopedServices.GetRequiredService<DataContext>();
            //        var logger = scopedServices
            //            .GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            //        // Ensure the database is created.
            //        db.Database.EnsureCreated();

            //        try
            //        {
            //            // Seed the database with test data.
            //            //Utilities.InitializeDbForTests(db);
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.LogError(ex, $"An error occurred seeding the " +
            //                                "database with test messages. Error: {ex.Message}");
            //        }
            //    }

            //});

            builder.ConfigureTestServices(services =>
            {
                services
                    .AddMvcCore()
                    .AddApplicationPart(typeof(TestAccountController).GetTypeInfo().Assembly);
            });
        }
    }
}
