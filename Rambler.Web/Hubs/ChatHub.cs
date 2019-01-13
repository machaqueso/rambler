using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rambler.Web.Models;
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
                Author = author,
                Message = message,
                Source = ApiSource.Rambler
            });
        }
    }
}