using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rambler.Models;
using Rambler.Web.Services;

namespace Rambler.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatService chatService;

        public ChatHub(ChatService chatService)
        {
            this.chatService = chatService;
        }
        
        public async Task SendMessage(string author, string message)
        {
            await chatService.CreateMessage(new ChatMessage
            {
                Date = DateTime.UtcNow,
                Author = new Author
                {
                    Name = author
                },
                Message = message,
                Source = ApiSource.Rambler
            });
        }

        public async Task DirectMessage(string author, string message)
        {
            var chatMessage = new ChatMessage
            {
                Source = ApiSource.Rambler,
                Date = DateTime.UtcNow,
                Message = message
            };

            await Clients.All.SendAsync("ReceiveChannelMessage", new ChannelMessage
            {
                Channel = "All",
                ChatMessage = chatMessage
            });

        }

        public async Task TestMessage()
        {
            await Clients.All.SendAsync("ReceiveTestMessage", new 
            {
                Date = DateTime.UtcNow,
                Author = "Test",
                Message = Guid.NewGuid().ToString()
            });

        }

    }
}