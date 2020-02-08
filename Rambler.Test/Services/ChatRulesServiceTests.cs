using Microsoft.Extensions.DependencyInjection;
using Rambler.Models;
using Rambler.Services;
using Xunit;

namespace Rambler.Test.Services
{
    public class ChatRulesServiceTest : IClassFixture<DependencySetupFixture>
    {
        private readonly ServiceProvider serviceProvider;

        public ChatRulesServiceTest(DependencySetupFixture fixture)
        {
            serviceProvider = fixture.ServiceProvider;
        }

        [Theory]
        [InlineData("rabble", false)]
        [InlineData("rabble rabble", false)]
        [InlineData("rabble rabble rabble ", false)]
        [InlineData("rabble rabble rabble rabble ", false)]
        [InlineData("rabble rabble rabble rabble rabble ", true)]
        [InlineData("rabble rabble rabble rabble rabble rabble ", true)]
        public void Given_message_with_duplicate_words_HasTooManyDuplicateWords_returns_true(string message, bool result)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ChatRulesService>();
                Assert.True(service.HasTooManyDuplicateWords(new ChatMessage { Message = message }) == result);
            }
        }

        [Theory]
        [InlineData("rabble", false)]
        [InlineData("raabble", false)]
        [InlineData("raaabble", false)]
        [InlineData("raaaabble", false)]
        [InlineData("raaaaabble", false)]
        [InlineData("raaaaaabble", true)]
        public void Given_message_with_duplicate_words_HasTooManyDuplicateCharacters_returns_true(string message, bool result)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ChatRulesService>();
                Assert.True(service.HasTooManyDuplicateCharacters(new ChatMessage { Message = message }) == result);
            }
        }

        [Theory]
        [InlineData("this is a super long phrase that should fail without spaces on it", false)]
        [InlineData("this isisasuperlongphrasewhatshouldfailthe test", true)]
        public void Given_message_with_duplicate_words_ContainsWordTooLong_returns_true(string message, bool result)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ChatRulesService>();
                Assert.True(service.ContainsWordTooLong(new ChatMessage { Message = message }) == result);
            }
        }
    }
}
