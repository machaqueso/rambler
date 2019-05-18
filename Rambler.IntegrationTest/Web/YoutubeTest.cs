using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Web;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rambler.IntegrationTest.Web
{
    public class YoutubeTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> factory;

        public YoutubeTest(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Index_works()
        {
            var client = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
                services => services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new FakeUserFilter());
                    }))).CreateClient();

            var response = await client.GetAsync("/youtube");
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Given_Unconfigured_API_Authorize_returns_configuration_view()
        {
            var client = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
                services => services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new FakeUserFilter());
                    }))).CreateClient();

            var response = await client.GetAsync("/youtube/authorize");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Configuration", response.RequestMessage.RequestUri.PathAndQuery);
        }
    }
}
