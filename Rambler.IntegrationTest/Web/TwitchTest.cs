using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Web;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rambler.IntegrationTest.Web
{
    public class TwitchTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> factory;

        public TwitchTest(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Index_works()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/twitch");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Given_Unconfigured_API_Authorize_returns_configuration_view()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/twitch/authorize");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Configuration", response.RequestMessage.RequestUri.PathAndQuery);
        }
    }
}
