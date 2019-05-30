using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Hubs;

namespace Rambler.Web.Services
{
    public class ChatService
    {
        private readonly DataContext db;
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly ChannelService channelService;
        private readonly AuthorService authorService;

        public ChatService(DataContext db, IHubContext<ChatHub> chatHubContext, ChannelService channelService,
            AuthorService authorService)
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
            var authorFilters = await authorService.GetFilters()
                .Where(x => x.Author.SourceAuthorId == message.Author.SourceAuthorId)
                .ToListAsync();

            foreach (var channel in channels)
            {
                if (AllowedMessage(message, channel.Name, authorFilters))
                {
                    channelNames.Add(channel.Name);
                }
            }

            return channelNames;
        }

        public bool AllowedMessage(ChatMessage message, string channel, IList<AuthorFilter> authorFilters)
        {
            switch (channel)
            {
                case "Reader":
                    return ReaderRules(message, authorFilters);
                case "OBS":
                    return OBSRules(message, authorFilters);
                case "TTS":
                    return TTSRules(message, authorFilters);
                default:
                    return GlobalRules(message, authorFilters);
            }
        }

        private bool TTSRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            if (IsInList(message, authorFilters, FilterTypes.Whitelist))
            {
                return true;
            }

            if (message.Author.Score >= 0)
            {
                return true;
            }

            return false;
        }

        private bool OBSRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            if (IsInList(message, authorFilters, FilterTypes.Whitelist))
            {
                return true;
            }

            if (message.Author.Score >= 0)
            {
                return true;
            }

            return false;
        }

        private bool ReaderRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            if (IsInList(message, authorFilters, FilterTypes.Whitelist))
            {
                return true;
            }

            if (IsInList(message, authorFilters, FilterTypes.Ignorelist))
            {
                return true;
            }

            return false;
        }

        public bool GlobalRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            if (IsInList(message, authorFilters, FilterTypes.Whitelist))
            {
                return true;
            }

            return false;
        }

        public bool IsInList(ChatMessage message, IList<AuthorFilter> authorFilters, string filterType)
        {
            return authorFilters.Any(x => x.FilterType == filterType);
        }
    }
}