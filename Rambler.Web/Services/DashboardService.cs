using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rambler.Web.Hubs;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class DashboardService
    {
        private readonly IHubContext<DashboardHub> dashboardHub;

        private static IList<ApiStatus> apiStatuses;

        public DashboardService(IHubContext<DashboardHub> dashboardHub)
        {
            this.dashboardHub = dashboardHub;

            if (apiStatuses == null)
            {
                Init();
            }
        }

        private void Init()
        {
            apiStatuses = new List<ApiStatus>();
            foreach (var apiSource in ApiSource.All.OrderBy(x => x))
            {
                apiStatuses.Add(new ApiStatus(apiSource));
            }
        }

        public async Task UpdateStatus(string name, string status, CancellationToken cancellationToken)
        {
            var apiStatus = apiStatuses.FirstOrDefault(x => x.Name == name);
            if (apiStatus == null)
            {
                return;
            }

            apiStatus.Status = status;
            apiStatus.UpdateDate = DateTime.UtcNow;

            await dashboardHub.Clients.All.SendAsync("updateStatus", apiStatuses, cancellationToken);
        }

        public async Task UpdateStatus(string name, string status)
        {
            await UpdateStatus(name, status, CancellationToken.None);
        }

        public async Task SendStatus()
        {
            await dashboardHub.Clients.All.SendAsync("updateStatus", apiStatuses, CancellationToken.None);
        }

        public IEnumerable<ApiStatus> GetApiStatuses()
        {
            return apiStatuses;
        }
    }
}