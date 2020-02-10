using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Data;
using Rambler.Models;
using Xunit;

namespace Rambler.Test.Integration.Web.Api
{
    public class ChatApiControllerTest : IClassFixture<WebApplicationFactory<Rambler.Web.Startup>>
    {
        private readonly WebApplicationFactory<Rambler.Web.Startup> _factory;
        private IFixture fixture;

        public ChatApiControllerTest(WebApplicationFactory<Rambler.Web.Startup> factory)
        {
            _factory = factory;
            fixture = new Fixture();
        }

        [Fact]
        public async Task Given_valid_message_CreateMessage_saves_it_to_database()
        {
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    services.AddDbContext<DataContext>(options =>
                        options.UseInMemoryDatabase(databaseName: "TestDatabase"));

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetService<DataContext>();
                        db?.Database.EnsureDeleted();
                        // No need to call migrate here, it's already been called by regular startup
                    }
                });
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => { });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test");

            var message = fixture
                .Build<ChatMessage>()
                .Without(x => x.Author)
                .Without(x => x.Infractions)
                .Create();

            var response = await client.PostAsync("/api/chat", message, new JsonMediaTypeFormatter());
            var contents = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}