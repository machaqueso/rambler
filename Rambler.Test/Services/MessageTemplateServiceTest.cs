using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Models;
using Rambler.Services;
using Xunit;

namespace Rambler.Test.Services
{
    public class MessageTemplateServiceTest : IClassFixture<DependencySetupFixture>
    {
        private readonly ServiceProvider serviceProvider;
        private IFixture autoFixture;

        public MessageTemplateServiceTest(DependencySetupFixture fixture)
        {
            autoFixture = new Fixture();
            serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task Given_message_template_Interpolate_works()
        {
            var author = autoFixture.Build<Author>()
                .Without(x => x.AuthorFilters)
                .Without(x => x.AuthorScoreHistories)
                .Without(x => x.ChatMessages)
                .With(x => x.Name, "Lucio")
                .With(x => x.Score, 100)
                .Create();

            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<MessageTemplateService>();
                var result = await service.Interpolate("@{author.name} your score is {author.score}", author);

                result.Should().Be($"@{author.Name} your score is {author.Score}");
            }


        }
    }
}