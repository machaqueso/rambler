using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Rambler.Web.Hubs
{
    public class DashboardHub : Hub
    {
        public async Task UpdateStatus(string apiSource, string status)
        {
            await Clients.All.SendAsync("updateStatus", apiSource, status);
        }
    }
}