using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Rambler.Models;
using Rambler.Web.Hubs;
using Rambler.Web.Services;
using Xunit;

namespace Rambler.Services.Test.Services
{
    public class DashboardServiceTest
    {
        private readonly IHubContext<DashboardHub> hub;
        private readonly DashboardService dashboardService;
        private readonly IFixture fixture;

        public DashboardServiceTest()
        {
            hub = Substitute.For<IHubContext<DashboardHub>>();
            dashboardService = new DashboardService(hub);
            fixture = new Fixture();
        }

        [Fact]
        public void GetApiStatuses_returns_enumerable()
        {
            var items = dashboardService.GetApiStatuses();
            items.Should().BeAssignableTo<IEnumerable<ApiStatus>>();
        }

        [Fact]
        public async Task UpdateStatus_updates_status()
        {
            var items = dashboardService.GetApiStatuses();

            foreach (var apiStatus in items)
            {
                var originalStatus = apiStatus.Status;
                var newStatus = fixture.Create<string>();

                await dashboardService.UpdateStatus(apiStatus.Name, newStatus);

                Assert.True(dashboardService.GetApiStatuses().ToList().Any(x => x.Name == apiStatus.Name && x.Status == newStatus));
            }
        }

    }
}