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
using Xunit;

namespace Rambler.Services.Test.Services
{
    public class ChannelServiceTest
    {
        private readonly IConfiguration configuration;
        private readonly DataContext dataContext;
        private readonly ChannelService channelService;
        private IFixture fixture;

        public ChannelServiceTest()
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

            channelService = new ChannelService(dataContext);

            fixture = new Fixture();
        }

        [Fact]
        public void Given_channels_GetChannels_returns_items()
        {
            var channels = channelService.GetChannels();
            channels.Should().BeAssignableTo<IQueryable<Channel>>();
        }

        [Fact]
        public async Task Given_existing_channel_UpdateChannel_updates_it()
        {
            var channel = fixture.Build<Channel>()
                .Without(x => x.Id)
                .Create();

            await dataContext.AddAsync(channel);
            await dataContext.SaveChangesAsync();

            var channelToUpdate = await channelService.GetChannels().SingleOrDefaultAsync(x => x.Id == channel.Id);
            channelToUpdate.Name = fixture.Create<string>();

            await channelService.UpdateChannel(channel.Id, channelToUpdate);

            Assert.True(channelService.GetChannels().Any(x => x.Id == channel.Id
                                                              && x.Name == channelToUpdate.Name));
        }

        [Fact]
        public async Task Given_nonexistent_channel_UpdateChannel_throws()
        {
            var id = fixture.Create<int>();
            var channel = fixture.Build<Channel>()
                .Create();

            await Assert.ThrowsAsync<InvalidOperationException>(() => channelService.UpdateChannel(id, channel));
        }

    }
}
