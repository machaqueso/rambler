using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Rambler.Models;
using Rambler.Web.Services;

namespace Rambler.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatService chatService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ChatHub(ChatService chatService, IHttpContextAccessor httpContextAccessor)
        {
            this.chatService = chatService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task SendMessage(string author, string message)
        {
            var chatMessage = new ChatMessage
            {
                Date = DateTime.UtcNow,
                Author = new Author
                {
                    Name = author,
                    Source = ApiSource.Rambler
                },
                Message = message,
                Source = ApiSource.Rambler
            };

            var userClaim = httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userClaim != null)
            {
                chatMessage.Author.SourceAuthorId = userClaim.Value;
            }

            await chatService.CreateMessage(chatMessage);
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