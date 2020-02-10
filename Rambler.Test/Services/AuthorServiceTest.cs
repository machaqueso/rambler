using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Models;
using Rambler.Services;
using Xunit;

namespace Rambler.Test.Services
{
    public class AuthorServiceTest : IClassFixture<DependencySetupFixture>
    {
        private readonly ServiceProvider serviceProvider;
        private IFixture autoFixture;

        public AuthorServiceTest(DependencySetupFixture fixture)
        {
            autoFixture = new Fixture();
            serviceProvider = fixture.ServiceProvider;
        }
        
        [Theory]
        [InlineData("", "", "", false)]
        [InlineData("foo", "", "", false)]
        [InlineData("", "foo", "", false)]
        [InlineData("", "", "foo", false)]
        [InlineData("foo", "bar", "", false)]
        [InlineData("", "foo", "bar", false)]
        [InlineData("foo", "", "bar", false)]
        [InlineData("foo", "bar", "quack", true)]
        public void Given_valid_author_IsValid_returns_true(string source, string sourceAuthorId, string name, bool expected)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<AuthorService>();
                Assert.True(service.IsValid(new Author
                {
                    Source = source,
                    SourceAuthorId = sourceAuthorId,
                    Name = name
                }) == expected);
            }
        }

    }
}