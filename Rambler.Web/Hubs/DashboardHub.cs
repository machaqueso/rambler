using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rambler.Web.Services;

namespace Rambler.Web.Hubs
{
    public class DashboardHub : Hub
    {
        private readonly DashboardService dashboardService;

        public DashboardHub(DashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        public async Task UpdateStatus(string name, string status)
        {
            await dashboardService.UpdateStatus(name, status);
        }
    }
}