using System;
using Microsoft.AspNetCore.SignalR;

namespace Rambler.Web.Models
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

            //Clients.All.addChatMessageToPage(chatMessage.Author, chatMessage.Message);
        }
    }
}