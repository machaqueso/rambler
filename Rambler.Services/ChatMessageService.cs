using System;
using System.Linq;
using System.Threading.Tasks;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Exceptions;

namespace Rambler.Services
{
    public class ChatMessageService
    {
        private readonly DataContext db;

        public ChatMessageService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<ChatMessage> GetMessages()
        {
            return db.Messages;
        }

        public async Task CreateMessage(ChatMessage message)
        {
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

            if (db.Messages.Any(x => !string.IsNullOrWhiteSpace(message.SourceMessageId)
                                     && x.SourceMessageId == message.SourceMessageId))
            {
                throw new ConflictException("Duplicate message");
            }

            // Prevents existing authors from being added as duplicates
            if (message.AuthorId > 0)
            {
                message.Author = null;
            }

            db.Messages.Add(message);
            await db.SaveChangesAsync();
        }

    }
}