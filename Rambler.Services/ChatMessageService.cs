using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Exceptions;

namespace Rambler.Services
{
    public class ChatMessageService
    {
        private readonly DataContext db;
        private readonly ILogger<ChatMessageService> logger;

        public ChatMessageService(DataContext db, ILogger<ChatMessageService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public IQueryable<ChatMessage> GetMessages()
        {
            return db.Messages;
        }

        public async Task CreateMessage(ChatMessage message)
        {
            if (message.Author == null && message.AuthorId == 0)
            {
                throw new UnprocessableEntityException("Author property cannot be null");
            }

            if (string.IsNullOrWhiteSpace(message.Author.Name))
            {
                logger.LogWarning("Author's name cannot be empty");
                throw new UnprocessableEntityException("Author's name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(message.Author.Source))
            {
                logger.LogWarning("Author's source cannot be empty");
                throw new UnprocessableEntityException("Author's source cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(message.Author.SourceAuthorId))
            {
                logger.LogWarning("Author's source id cannot be empty");
                throw new UnprocessableEntityException("Author's source id cannot be empty");
            }

            if (db.Messages.Any(x => !string.IsNullOrWhiteSpace(message.SourceMessageId)
                                     && x.SourceMessageId == message.SourceMessageId))
            {
                logger.LogWarning($"Duplicate message, {message.Source} SourceMessageId={message.SourceMessageId}");
                throw new ConflictException("Duplicate message");
            }

            // Prevents existing authors from being added as duplicates
            if (message.AuthorId > 0)
            {
                message.Author = null;
            }

            logger.LogInformation($"Source={message.Source}");
            logger.LogInformation($"SourceMessageId={message.SourceMessageId}");
            logger.LogInformation($"AuthorId = {message.AuthorId}");
            logger.LogInformation($"Message={message.Message}");
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            logger.LogInformation($"Message saved? {db.Messages.Any(x => x.SourceMessageId == message.SourceMessageId)}");

        }

        public async Task<bool> MessageExists(ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.SourceMessageId) || string.IsNullOrWhiteSpace(message.Source))
            {
                return false;
            }

            return await db.Messages.AnyAsync(x => x.Source == message.Source && x.SourceMessageId == message.SourceMessageId);
        }

    }
}