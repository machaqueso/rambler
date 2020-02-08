using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rambler.Web.Services
{
    public class ChatService
    {
        private readonly DataContext db;
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly AuthorService authorService;
        private readonly BotService botService;
        private readonly IntegrationManager integrationManager;
        private readonly ChatRulesService chatRulesService;
        private readonly ChatMessageService chatMessageService;

        public ChatService(DataContext db, IHubContext<ChatHub> chatHubContext, AuthorService authorService,
            BotService botService, IntegrationManager integrationManager, ChatRulesService chatRulesService, ChatMessageService chatMessageService)
        {
            this.db = db;
            this.chatHubContext = chatHubContext;
            this.authorService = authorService;
            this.botService = botService;
            this.integrationManager = integrationManager;
            this.chatRulesService = chatRulesService;
            this.chatMessageService = chatMessageService;
        }

        public IQueryable<ChatMessage> GetMessages()
        {
            return chatMessageService.GetMessages();
        }

        public async Task CreateMessage(ChatMessage message)
        {
            // Ignore duplicate messages from apis
            if (!string.IsNullOrEmpty(message.SourceMessageId) &&
                db.Messages.Any(x => x.SourceMessageId == message.SourceMessageId))
            {
                return;
            }

            // Tries to match author to existing one coming from same source and id
            var author = await authorService.GetAuthors()
                .Where(x => string.IsNullOrWhiteSpace(x.Source))
                .Where(x => string.IsNullOrWhiteSpace(x.SourceAuthorId))
                .SingleOrDefaultAsync(x => x.Source == message.Author.Source
                                           && x.SourceAuthorId == message.Author.SourceAuthorId);

            if (author == null)
            {
                await authorService.Create(message.Author);
                message.AuthorId = message.Author.Id;
            }
            else
            {
                // Housekeeping: syncs author's name changes
                if (author.Name != message.Author.Name)
                {
                    author.Name = message.Author.Name;
                    await db.SaveChangesAsync();
                }

                //TODO: might need this if duplicate authors get inserted
                //message.Author = null;
                //message.AuthorId = author.Id;

                // apply infraction penalty
                if (chatRulesService.HasInfractions(message))
                {
                    author.Score -= 1;
                    await db.SaveChangesAsync();
                }
            }

            await chatMessageService.CreateMessage(message);
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

            foreach (var channel in await chatRulesService.AllowedChannels(message))
            {
                // check for rule to send to this channel
                await SendToChannel(channel, message);
            }

            if (message.AuthorId > 0)
            {
                message.Author = null;
            }
        }

    }
}
