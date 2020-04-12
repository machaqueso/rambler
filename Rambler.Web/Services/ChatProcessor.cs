using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rambler.Models;
using Rambler.Models.Exceptions;
using Rambler.Services;
using Rambler.Web.Hubs;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rambler.Web.Services
{
    public class ChatProcessor
    {
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly AuthorService authorService;
        private readonly BotService botService;
        private readonly IntegrationManager integrationManager;
        private readonly ChatRulesService chatRulesService;
        private readonly ChatMessageService chatMessageService;
        private readonly ILogger<ChatMessageService> logger;
        private readonly MessageTemplateService messageTemplateService;

        public ChatProcessor(IHubContext<ChatHub> chatHubContext, AuthorService authorService,
            BotService botService, IntegrationManager integrationManager, ChatRulesService chatRulesService,
            ChatMessageService chatMessageService, ILogger<ChatMessageService> logger, MessageTemplateService messageTemplateService)
        {
            this.chatHubContext = chatHubContext;
            this.authorService = authorService;
            this.botService = botService;
            this.integrationManager = integrationManager;
            this.chatRulesService = chatRulesService;
            this.chatMessageService = chatMessageService;
            this.logger = logger;
            this.messageTemplateService = messageTemplateService;
        }


        public async Task ProcessIncomingMessage(ChatMessage message)
        {
            if (message == null)
            {
                throw new UnprocessableEntityException("Cannot process null message");
            }

            // check if message from same source with same SourceMessageId already in database
            if (await chatMessageService.MessageExists(message))
            {
                logger.LogInformation($"{message.Source} message id {message.SourceMessageId} already in database, skipping");
                return;
            }

            // - check for author
            if (message.Author == null)
            {
                if (message.AuthorId == 0)
                {
                    throw new UnprocessableEntityException("Author not defined");
                }

                message.Author = await authorService.GetAuthors().SingleOrDefaultAsync(x => x.Id == message.AuthorId);
            }

            if (message.Author == null)
            {
                logger.LogWarning($"Message without author... skipping");
                return;
            }

            if (!authorService.IsValid(message.Author))
            {
                logger.LogInformation($"invalid author, skipping: Source='{message.Author.Source}', SourceAuthorId='{message.Author.SourceAuthorId}', Name='{message.Author.Name}'");
                return;
            }

            var author = await authorService.EnsureAuthor(message.AuthorId, message.Author);

            // Housekeeping: syncs author's name changes
            if (author.Name != message.Author.Name)
            {
                author.Name = message.Author.Name;
            }

            // - check infractions
            if (chatRulesService.HasInfractions(message))
            {
                // apply infraction penalty
                author.Score -= 1;
            }
            else
            {
                author.Score += 1;
            }

            await authorService.Update(author);

            // save message
            if (message.AuthorId == 0)
            {
                message.AuthorId = author.Id;
            }
            await chatMessageService.CreateMessage(message);

            await SendToChannel("All", message);
            if (await botService.IsBotCommand(message))
            {
                await ProcessIncomingBotMessage(message);
                return;
            }

            // send message to view channels
            foreach (var channel in await chatRulesService.AllowedChannels(message))
            {
                await SendToChannel(channel, message);
            }

            //if (message.AuthorId > 0)
            //{
            //    message.Author = null;
            //}

        }

        public async Task ProcessIncomingBotMessage(ChatMessage message)
        {
            if (message.Author == null)
            {
                throw new InvalidOperationException("Message.Author cannot be null");
            }

            var action = await botService.Process(message);
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
                        Message = await messageTemplateService.Interpolate(action.Parameters, message.Author),
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
                            Source = message.Author.Source,
                            SourceAuthorId = Guid.NewGuid().ToString()
                        };
                    }
                    else
                    {
                        botMessage.AuthorId = ramblerBot.Id;
                    }

                    botMessage.Author = ramblerBot;

                    await ProcessOutgoingMessage(botMessage);

                    break;
                case "Play media":
                    await chatHubContext.Clients.All.SendAsync("PlayMedia", action.Parameters);
                    break;
                default:
                    break;
            }
        }

        public async Task ProcessOutgoingMessage(ChatMessage message)
        {
            integrationManager.MessageSentEvent(message.Message, message.Source == ApiSource.RamblerBot ? message.Author.Source : "");
        }

        public async Task SendToChannel(string channel, ChatMessage message)
        {
            var messageText = message.Message;

            // TODO: add rules of what is allowed on channels
            // Filter emojis out of TTS messages
            if (channel == "TTS")
            {
                messageText = Regex.Replace(messageText, @"\p{Cs}", "");
            }

            // SignalR appears to not like complex objects being passed down, so I changed this to send a dynamic instead of using ChannelMessage
            await chatHubContext.Clients.All.SendAsync("ReceiveChannelMessage", new
            {
                Channel = channel,
                ChatMessage = new
                {
                    message.Id,
                    message.Date,
                    Message = messageText,
                    message.Source,
                    Author = message.Author.Name,
                    message.Author.SourceAuthorId,
                    message.DisplayDate,
                    message.DisplayTime
                }
            });
        }
    }
}