using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Exceptions;
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
        private readonly WordFilterService wordFilterService;
        private readonly BotService botService;
        private readonly IntegrationManager integrationManager;

        public ChatService(DataContext db, IHubContext<ChatHub> chatHubContext, ChannelService channelService,
            AuthorService authorService, WordFilterService wordFilterService, BotService botService, IntegrationManager integrationManager)
        {
            this.db = db;
            this.chatHubContext = chatHubContext;
            this.channelService = channelService;
            this.authorService = authorService;
            this.wordFilterService = wordFilterService;
            this.botService = botService;
            this.integrationManager = integrationManager;
        }

        public IQueryable<ChatMessage> GetMessages()
        {
            return db.Messages;
        }

        public async Task CreateMessage(ChatMessage message)
        {
            // Ignore duplicate messages from apis
            if (!string.IsNullOrEmpty(message.SourceMessageId) &&
                db.Messages.Any(x => x.SourceMessageId == message.SourceMessageId))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Author.Name))
            {
                throw new UnprocessableEntityException("Author's name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(message.Author.Source))
            {
                throw new UnprocessableEntityException("Author's source cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(message.Author.SourceAuthorId))
            {
                throw new UnprocessableEntityException("Author's source id cannot be empty");
            }

            // Tries to match author to existing one coming from same source and id
            var author = await authorService.GetAuthors()
                .Where(x => string.IsNullOrWhiteSpace(x.Source))
                .Where(x => string.IsNullOrWhiteSpace(x.SourceAuthorId))
                .SingleOrDefaultAsync(x => x.Source == message.Author.Source
                                           && x.SourceAuthorId == message.Author.SourceAuthorId);

            if (author != null)
            {
                // Housekeeping: syncs author's name changes
                if (author.Name != message.Author.Name)
                {
                    author.Name = message.Author.Name;
                    await db.SaveChangesAsync();
                }

                message.Author = null;
                message.AuthorId = author.Id;
            }

            db.Messages.Add(message);
            await db.SaveChangesAsync();

            await SendToChannels(message);

            // sends message to integrations if local
            if (message.Source == ApiSource.Rambler)
            {
                integrationManager.MessageSentEvent(message.Message);
            }

            var action = botService.Process(message);
            if (action == null)
            {
                return;
            }

            switch (action.Action)
            {
                case "Say":
                    var botMessage = new ChatMessage
                    {
                        Date = DateTime.UtcNow,
                        Message = action.Parameters,
                        Source = ApiSource.RamblerBot
                    };

                    var ramblerBot = await authorService.GetAuthors()
                        .FirstOrDefaultAsync(x => x.Source == ApiSource.RamblerBot
                                                  && x.Name == "RamblerBot"
                                                  && !string.IsNullOrWhiteSpace(x.SourceAuthorId));
                    if (ramblerBot == null)
                    {
                        ramblerBot = new Author
                        {
                            Name = "RamblerBot",
                            Source = ApiSource.RamblerBot,
                            SourceAuthorId = Guid.NewGuid().ToString()
                        };
                    }
                    else
                    {
                        botMessage.AuthorId = ramblerBot.Id;
                    }
                    botMessage.Author = ramblerBot;

                    await SendToChannels(botMessage);

                    break;
                default:
                    break;
            }
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

            if (message.AuthorId > 0)
            {
                message.Author = null;
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

            if (wordFilterService.GetWordFilters().Any(x => message.Message.Contains(x.Word)))
            {
                return false;
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

            if (wordFilterService.GetWordFilters().Any(x => message.Message.Contains(x.Word)))
            {
                return false;
            }

            if (message.Author.Score >= 0)
            {
                return true;
            }

            return false;
        }

        private bool ReaderRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            if (IsInList(message, authorFilters, FilterTypes.Banlist)
                || IsInList(message, authorFilters, FilterTypes.Blacklist)
                || message.Author.Score < -10)
            {
                return false;
            }

            return true;
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
