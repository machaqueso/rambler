using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Web.Data;
using Rambler.Web.Hubs;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class ChatService
    {
        private readonly DataContext db;
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly ChannelService channelService;

        public ChatService(DataContext db, IHubContext<ChatHub> chatHubContext, ChannelService channelService)
        {
            this.db = db;
            this.chatHubContext = chatHubContext;
            this.channelService = channelService;
        }

        public IQueryable<ChatMessage> GetMessages()
        {
            return db.Messages;
        }

        public async Task CreateMessage(ChatMessage message)
        {
            if (!string.IsNullOrEmpty(message.SourceMessageId) &&
                db.Messages.Any(x => x.SourceMessageId == message.SourceMessageId))
            {
                return;
            }

            db.Messages.Add(message);
            await db.SaveChangesAsync();

            await SendToChannels(message);
        }

        public async Task SendToChannel(string channel, ChatMessage message)
        {
            await chatHubContext.Clients.All.SendAsync("ReceiveChannelMessage", new ChannelMessage
            {
                Channel = channel,
                ChatMessage = message
            });
        }

        public async Task SendToChannels(ChatMessage message)
        {
            var channels = await channelService.GetChannels().ToListAsync();

            foreach (var channel in channels)
            {
                // check for rule to send to this channel
                await SendToChannel(channel.Name, message);
            }
        }

    }
}