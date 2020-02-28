using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Rambler.Models;
using Rambler.Services;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Xunit;

namespace Rambler.Test.Services
{
    public class ChatRulesServiceTest : IClassFixture<DependencySetupFixture>
    {
        private readonly ServiceProvider serviceProvider;
        private IFixture autoFixture;

        public ChatRulesServiceTest(DependencySetupFixture fixture)
        {
            autoFixture = new Fixture();
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
        [InlineData("raaaabble", false)]
        [InlineData("raaaaaabble", false)]
        [InlineData("raaaaaaaabble", false)]
        [InlineData("raaaaaaaaaabble", false)]
        [InlineData("raaaaaaaaaaaabble", true)]
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

        [Theory]
        [InlineData("", false)]
        [InlineData("this is a test message", true)]
        public void Given_valid_message_GlobalRules_returns_true(string message, bool expectedResult)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ChatRulesService>();
                var chatMessage = new ChatMessage
                {
                    Message = message
                };

                var result = service.GlobalRules(chatMessage);
                result.Should().Be(expectedResult, $"Infractions: {chatMessage.Infractions?.Count}");
            }
        }

        [Theory]
        [InlineData("", 0, false)]
        [InlineData("this is a test message", 0, true)]
        public void Given_valid_message_TTSRules_allows_it(string message, int score, bool expectedResult)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<DataContext>();
                db.Database.Migrate();

                var service = scope.ServiceProvider.GetService<ChatRulesService>();
                var chatMessage = new ChatMessage
                {
                    Message = message,
                    Author = new Author
                    {
                        Score = score
                    }
                };

                var authorFilters = new List<AuthorFilter>();
                Assert.True(service.TTSRules(chatMessage, authorFilters) == expectedResult);
            }

        }
    }
}
