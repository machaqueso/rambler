using Rambler.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Rambler.Test.Integration.Web.Api
{
    public class AccountApiControllerTest
        : IClassFixture<CustomWebApplicationFactory<Rambler.Web.Startup>>
    {
        private readonly CustomWebApplicationFactory<Rambler.Web.Startup> _factory;

        public AccountApiControllerTest(CustomWebApplicationFactory<Rambler.Web.Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("", "", HttpStatusCode.UnprocessableEntity)]
        [InlineData("foo", "", HttpStatusCode.UnprocessableEntity)]
        [InlineData("", "bar", HttpStatusCode.UnprocessableEntity)]
        [InlineData("foo", "bar", HttpStatusCode.UnprocessableEntity)]
        [InlineData("admin", "password", HttpStatusCode.OK)]
        public async Task Given_valid_user_login_returns_Ok(string username, string password, HttpStatusCode expected)
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/account/login", new User
            {
                UserName = username,
                Password = password
            });

            Assert.Equal(expected, response.StatusCode);
        }
    }
}
