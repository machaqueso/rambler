using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Models;
using Rambler.Web.Hubs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rambler.Services;

namespace Rambler.Web.Services
{
    public class DashboardService
    {
        private readonly IHubContext<DashboardHub> dashboardHub;
        private readonly IntegrationService integrationService;
        private readonly ILogger<DashboardService> logger;

        public DashboardService(IHubContext<DashboardHub> dashboardHub, IntegrationService integrationService, ILogger<DashboardService> logger)
        {
            this.dashboardHub = dashboardHub;
            this.integrationService = integrationService;
            this.logger = logger;
        }

        public async Task UpdateStatus(string name, string status, CancellationToken cancellationToken)
        {
            await integrationService.UpdateStatus(name, status);
            await dashboardHub.Clients.All.SendAsync("updateStatus", integrationService.GetIntegrations().OrderBy(x => x.Name), cancellationToken);
        }

        public async Task UpdateStatus(string name, string status)
        {
            await UpdateStatus(name, status, CancellationToken.None);
        }
    }
}