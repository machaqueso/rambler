using System;
using Rambler.Data;
using Rambler.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rambler.Services
{
    public class ChatRulesService
    {
        private readonly DataContext db;
        private readonly AuthorService authorService;
        private readonly WordFilterService wordFilterService;
        private readonly BotService botService;
        private readonly ChannelService channelService;

        public ChatRulesService(DataContext db, AuthorService authorService, WordFilterService wordFilterService, BotService botService, ChannelService channelService)
        {
            this.db = db;
            this.authorService = authorService;
            this.wordFilterService = wordFilterService;
            this.botService = botService;
            this.channelService = channelService;
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

        public bool IsInList(ChatMessage message, IList<AuthorFilter> authorFilters, string filterType)
        {
            return authorFilters.Any(x => x.FilterType == filterType);
        }

        public bool ContainsBadWord(ChatMessage message)
        {
            var isInfraction = wordFilterService.GetWordFilters().Any(x => message.Message.Contains(x.Word));

            if (isInfraction)
            {
                message.AddInfraction(new MessageInfraction
                {
                    InfractionType = MessageInfractionTypes.ContainsBadWord
                });
            }

            return isInfraction;
        }

        /// <summary>
        /// Messages allowed to be read by TTS engine
        /// </summary>
        /// <param name="message"></param>
        /// <param name="authorFilters"></param>
        /// <returns></returns>
        private bool TTSRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            // Unless whitelisted ...
            if (IsInList(message, authorFilters, FilterTypes.Whitelist))
            {
                return true;
            }

            // Only messages from readers with positive score
            if (message.Author.Score < 0)
            {
                return false;
            }

            // Apply global rules
            return GlobalRules(message, authorFilters);
        }

        /// <summary>
        /// Messages allowed to be shown by chat embedded in OBS
        /// </summary>
        /// <param name="message"></param>
        /// <param name="authorFilters"></param>
        /// <returns></returns>
        private bool OBSRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            // Unless whitelisted ...
            if (IsInList(message, authorFilters, FilterTypes.Whitelist))
            {
                return true;
            }

            // Only messages from readers with positive score
            if (message.Author.Score >= 0)
            {
                return false;
            }

            // Apply global rules
            return GlobalRules(message, authorFilters);
        }

        // Reader shows everything except banned/blacklisted and low score
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

        public bool HasTooManyDuplicateWords(ChatMessage message)
        {
            var wordGroups = message.Message.Split(' ')
                .GroupBy(x => new { x })
                .Select(s => new { s.Key, Count = s.Count() });

            // TODO: max duplicate words should be configurable
            var isInfraction = wordGroups.Any(x => x.Count >= 5);

            if (isInfraction)
            {
                message.AddInfraction(new MessageInfraction
                {
                    InfractionType = MessageInfractionTypes.HasTooManyDuplicateWords
                });
            }

            return isInfraction;
        }

        public bool HasTooManyDuplicateCharacters(ChatMessage message)
        {
            var isInfraction = Regex.IsMatch(message.Message, @"(.)\1{5,}");

            if (isInfraction)
            {
                message.AddInfraction(new MessageInfraction
                {
                    InfractionType = MessageInfractionTypes.HasTooManyDuplicateCharacters
                });
            }

            return isInfraction;
        }

        public bool ContainsWordTooLong(ChatMessage message)
        {
            var isInfraction = message.Message.Split(' ').Any(x => x.Length >= 20);

            if (isInfraction)
            {
                message.AddInfraction(new MessageInfraction
                {
                    InfractionType = MessageInfractionTypes.ContainsWordTooLong
                });
            }

            return isInfraction;
        }

        /// <summary>
        /// Author has already posted this same message several times in a short period of time
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool MessageRepeatedTooOften(ChatMessage message)
        {
            var startDate = DateTime.UtcNow.AddMinutes(-60);
            
            var isInfraction = db.Messages
                                   .Where(x => x.Date > startDate)
                                   .Count(x => x.AuthorId == message.AuthorId && x.Message == message.Message) > 5;

            if (isInfraction)
            {
                message.AddInfraction(new MessageInfraction
                {
                    InfractionType = MessageInfractionTypes.MessageRepeatedTooOften
                });
            }

            return isInfraction;
        }

        public bool HasInfractions(ChatMessage message)
        {
            if (ContainsBadWord(message))
            {
                return true;
            }

            if (HasTooManyDuplicateWords(message))
            {
                return true;
            }

            if (HasTooManyDuplicateCharacters(message))
            {
                return true;
            }

            if (ContainsWordTooLong(message))
            {
                return true;
            }

            if (MessageRepeatedTooOften(message))
            {
                return true;
            }

            return false;
        }


        public bool GlobalRules(ChatMessage message, IList<AuthorFilter> authorFilters)
        {
            if (message.Infractions == null)
            {
                message.Infractions = new List<MessageInfraction>();
                return HasInfractions(message);
            }

            return message.Infractions.Any();
        }

    }
}
