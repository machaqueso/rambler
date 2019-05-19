using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rambler.Data;
using Rambler.Models;
using Rambler.Web.Services;
using System;
using System.Linq;
using Xunit;

namespace Rambler.Services.Test.Services
{
    public class ChannelServiceTest
    {
        private readonly IConfiguration configuration;
        private readonly DataContext dataContext;
        private readonly ChannelService channelService;

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
        }

        [Fact]
        public void Given_channels_GetChannels_returns_items()
        {
            var channels = channelService.GetChannels();
            channels.Should().BeAssignableTo<IQueryable<Channel>>();
        }

    }
}
