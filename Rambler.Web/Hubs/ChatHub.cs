using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Rambler.Web.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            if (Clients == null)
            {
                return;
            }

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}