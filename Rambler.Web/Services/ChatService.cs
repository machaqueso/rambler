using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Web.Hubs;
using Rambler.Models;
using Rambler.Services;

namespace Rambler.Web.Services
{
    public class ChatService
    {
        private readonly DataContext db;
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly ChannelService channelService;
        private readonly AuthorService authorService;

        public ChatService(DataContext db, IHubContext<ChatHub> chatHubContext, ChannelService channelService, AuthorService authorService)
        {
            this.db = db;
            this.chatHubContext = chatHubContext;
            this.channelService = channelService;
            this.authorService = authorService;
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

            var author = await authorService.GetAuthors()
                .SingleOrDefaultAsync(x => x.Source == message.Author.Source
                                           && x.SourceAuthorId == message.Author.SourceAuthorId);
            if (author != null)
            {
                message.Author = null;
                message.AuthorId = author.Id;
            }

            db.Messages.Add(message);
            await db.SaveChangesAsync();

            await SendToChannels(message);
        }

        public async Task SendToChannel(string channel, ChatMessage message)
        {
            // SignalR appears to not like complex objects being passed down, so I changed this to send a dynamic instead of using ChannelMessage
            await chatHubContext.Clients.All.SendAsync("ReceiveChannelMessage", new 
            {
                Channel = channel,
                ChatMessage = new 
                {
                    message.Id,
                    message.Date,
                    message.Message,
                    message.Source,
                    Author = message.Author.Name,
                    message.Author.SourceAuthorId,
                    message.DisplayDate,
                    message.DisplayTime
                }
            });
        }

        public async Task SendToChannels(ChatMessage message)
        {
            await SendToChannel("All", message);

            foreach (var channel in await AllowedChannels(message))
            {
                // check for rule to send to this channel
                await SendToChannel(channel, message);
            }
        }

        public async Task<IEnumerable<string>> AllowedChannels(ChatMessage message)
        {
            var channels = await channelService.GetChannels()
                .Where(x => x.Name != "All")
                .ToListAsync();

            var channelNames = new List<string>();
            foreach (var channel in channels)
            {
                if (await AllowedMessage(message, channel.Name))
                {
                    channelNames.Add(channel.Name);
                }
            }

            return channelNames;
        }

        public async Task<bool> AllowedMessage(ChatMessage message, string channel)
        {
            switch (channel)
            {
                case "Reader":
                    return await ReaderRules(message);
                case "OBS":
                    return await OBSRules(message);
                case "TTS":
                    return await TTSRules(message);
                default:
                    return await GlobalRules(message);
            }
        }

        private async Task<bool> TTSRules(ChatMessage message)
        {
            return await GlobalRules(message);
        }

        private async Task<bool> OBSRules(ChatMessage message)
        {
            return await GlobalRules(message);
        }

        private async Task<bool> ReaderRules(ChatMessage message)
        {
            return await GlobalRules(message);
        }

        public async Task<bool> GlobalRules(ChatMessage message)
        {
            var authorFilters = authorService.GetFilters()
                .Where(x => x.Author.SourceAuthorId == message.Author.SourceAuthorId);

            if (!await authorFilters.AnyAsync())
            {
                return true;
            }

            if (!await authorFilters.AnyAsync(x => x.FilterType == FilterTypes.Whitelist))
            {
                return true;
            }

            // add other global rules here
            return true;
        }
    }
}