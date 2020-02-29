using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Models;
using Rambler.Services;
using Xunit;

namespace Rambler.Test.Services
{
    public class ChatMessageServiceTest : IClassFixture<DependencySetupFixture>
    {
        private readonly ServiceProvider serviceProvider;
        private IFixture autoFixture;

        public ChatMessageServiceTest(DependencySetupFixture fixture)
        {
            serviceProvider = fixture.ServiceProvider;
            autoFixture = new Fixture();
        }

        [Fact]
        public async Task Given_valid_message_CreateMessage_saves_it_to_database()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                DatabaseHelper.Init(scope);
                var service = scope.ServiceProvider.GetService<ChatMessageService>();

                var author = autoFixture
                    .Build<Author>()
                    .Without(x => x.ChatMessages)
                    .Without(x => x.AuthorFilters)
                    .Without(x => x.AuthorScoreHistories)
                    .Without(x => x.Id)
                    .Create();

                var message = autoFixture
                    .Build<ChatMessage>()
                    .Without(x => x.AuthorId)
                    .With(x => x.Author, author)
                    .Without(x => x.Infractions)
                    .Create();

                await service.CreateMessage(message);
                Assert.True(message.Id > 0);
                Assert.True(service.GetMessages().Any(x => x.Id == message.Id));
            }
        }

        [Fact]
        public async Task Given_existing_message_MessageExists_returns_true()
        {
            var author = autoFixture
                .Build<Author>()
                .Without(x => x.ChatMessages)
                .Without(x => x.AuthorFilters)
                .Without(x => x.AuthorScoreHistories)
                .With(x => x.Id, 7)
                .Create();

            var message = new ChatMessage
            {
                Source = "Youtube",
                SourceMessageId =
                    "LCC.CkwSIQoYVUNtVWZrYWlEbEEzSVpCaWJuTXR5cWx3EgUvbGl2ZSonChhVQ21VZmthaURsQTNJWkJpYm5NdHlxbHcSCzNIM3c2azRGbWI0EjkKGkNQcnZxWW1DeHVjQ0ZRY01oQW9kZTEwTFh3EhtDUDYxN3VlQnh1Y0NGY1EwWkFvZDFGZ0d2ZzA",
                Message = "quack yt",
                AuthorId = 7
            };

            using (var scope = serviceProvider.CreateScope())
            {
                DatabaseHelper.Init(scope);
                var service = scope.ServiceProvider.GetService<ChatMessageService>();
                var authorService = scope.ServiceProvider.GetService<AuthorService>();

                await authorService.Create(author);

                Assert.False(await service.MessageExists(message));
                await service.CreateMessage(message);
                Assert.True(await service.MessageExists(message));

            }

        }

    }
}
