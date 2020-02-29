using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Rambler.Test.Integration.Web.Api
{
    public class ChatApiControllerTest : IClassFixture<CustomWebApplicationFactory<Rambler.Web.Startup>>
    {
        private readonly CustomWebApplicationFactory<Rambler.Web.Startup> factory;
        private IFixture fixture;
        private readonly HttpClient client;

        public ChatApiControllerTest(CustomWebApplicationFactory<Rambler.Web.Startup> factory)
        {
            this.factory = factory;
            client = this.factory.WithWebHostBuilder(builder =>
            {
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
            fixture = new Fixture();
        }

        [Fact]
        public async Task Given_valid_message_CreateMessage_saves_it_to_database()
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Test");

            var message = fixture
                .Build<ChatMessage>()
                .Without(x => x.Author)
                .Without(x => x.Infractions)
                .Create();

            var response = await client.PostAsync("/api/chat", message, new JsonMediaTypeFormatter());
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Created, content);
        }
    }
}