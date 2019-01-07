﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rambler.Web.Data;
using Rambler.Web.Hubs;
using Rambler.Web.Models;

namespace Rambler.Web.Services
{
    public class ChatService
    {
        private readonly DataContext db;
        private readonly IHubContext<ChatHub> chatHubContext;

        public ChatService(DataContext db, IHubContext<ChatHub> chatHubContext)
        {
            this.db = db;
            this.chatHubContext = chatHubContext;
        }

        public IQueryable<ChatMessage> GetMessages()
        {
            return db.Messages;
        }

        public async Task CreateMessage(ChatMessage message)
        {
            db.Messages.Add(message);
            await db.SaveChangesAsync();
            await chatHubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}