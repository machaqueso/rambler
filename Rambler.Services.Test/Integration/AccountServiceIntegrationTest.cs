using System.Linq;
using Microsoft.Extensions.Configuration;
using Rambler.Data;
using Xunit;

namespace Rambler.Services.Test.Integration
{
    public class AccountServiceIntegrationTest
    {
        private readonly AccountService accountService;

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            return config;
        }

        public AccountServiceIntegrationTest()
        {
            var configuration = InitConfiguration();
            var datacontext = new DataContext(configuration);
            var passwordservice = new PasswordService();

            accountService = new AccountService(datacontext, passwordservice);
        }

        [Fact]
        public void GetUsers_itWorks()
        {
            var result = accountService.GetUsers();
            Assert.True(result.Any());
        }
    }
}
