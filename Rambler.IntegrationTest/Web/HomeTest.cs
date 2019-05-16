using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Web;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rambler.IntegrationTest.Web
{
    public class HomeTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> factory;

        public HomeTest(WebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task Given_unauthenticated_user_homepage_redirects_to_login()
        {
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var response = await client.GetAsync("/");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("Login", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task Given_authenticated_user_homepage_redirects_to_dashboard()
        {
            var client = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(
                services => services.AddMvc(
                    options =>
                    {
                        options.Filters.Add(new FakeUserFilter());
                    }))).CreateClient();

            var response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();

            Assert.Contains("Dashboard", response.RequestMessage.RequestUri.PathAndQuery);
        }
    }
}
