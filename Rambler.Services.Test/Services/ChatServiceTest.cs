using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rambler.Data;
using Rambler.Models;
using Rambler.Web.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Rambler.Web.Hubs;
using Xunit;

namespace Rambler.Services.Test.Services
{
    public class ChatServiceTest
    {
        private readonly IConfiguration configuration;
        private readonly DataContext dataContext;
        private readonly IHubContext<ChatHub> chatHub;
        private readonly ChannelService channelService;
        private readonly ChatService chatService;
        private IFixture fixture;

        public ChatServiceTest()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            // Duplicate here any configuration sources you use.
            configurationBuilder.AddJsonFile("appsettings.json");
            configuration = configurationBuilder.Build();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            dataContext = new DataContext(configuration, options);
            dataContext.Database.EnsureDeleted();

            chatHub = Substitute.For<IHubContext<ChatHub>>();
            channelService = Substitute.For<ChannelService>(dataContext);

            chatService = new ChatService(dataContext, chatHub, channelService);

            fixture = new Fixture();
        }

        [Fact]
        public void GetMessages_returns_items()
        {
            var items = chatService.GetMessages();
            items.Should().BeAssignableTo<IQueryable<ChatMessage>>();
        }

        [Fact]
        public async Task Given_new_message_CreateMessage_adds_it()
        {
            var message = fixture.Build<ChatMessage>()
                .Without(x => x.Id)
                .Create();

            await chatService.CreateMessage(message);

            Assert.True(chatService.GetMessages().Any(x => x.Author == message.Author
                                                           && x.Message == message.Message));
        }

        [Fact]
        public async Task SendToChannel_calls_SendAsync()
        {
            var channel = fixture.Build<Channel>()
                .Create();
            var message = fixture.Build<ChatMessage>()
                .Without(x => x.Id)
                .Create();

            await chatService.SendToChannel(channel.Name, message);

            Received.InOrder(async () =>
            {
                await chatHub.Clients.All.SendAsync(Arg.Any<string>(), Arg.Any<ChatMessage>());
            });
        }

    }
}
