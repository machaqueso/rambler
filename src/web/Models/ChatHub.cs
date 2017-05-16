using System;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.SignalR;
using Rambler.Models;

namespace web.Models
{
    public class ChatHub : Hub
    {
        public void NewMessage(string name, string message)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var chatMessage = new ChatMessage
            {
                Date = DateTime.Now,
                Author = name,
                Message = message
            };

            Clients.All.addChatMessageToPage(chatMessage.Author, chatMessage.Message);
        }
    }
}