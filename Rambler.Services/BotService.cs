using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Exceptions;

namespace Rambler.Services
{
    public class BotService
    {
        private readonly DataContext db;

        public BotService(DataContext db)
        {
            this.db = db;
        }

        public async Task<BotAction> Process(ChatMessage message)
        {
            foreach (var word in message.Message.Split(' '))
            {
                // match first command and return
                // TODO: handle multiple commands on single line?
                var command = await GetBotActions()
                    .FirstOrDefaultAsync(x => String.Equals(x.Command, word, StringComparison.CurrentCultureIgnoreCase));
                if (command != null)
                {
                    return new BotAction
                    {
                        Command = command.Command,
                        Action = command.Action,
                        Parameters = command.Parameters
                    };
                }
            }

            return null;
        }

        public IQueryable<BotAction> GetBotActions()
        {
            return db.BotActions;
        }

        public async Task<BotAction> GetBotAction(int id)
        {
            return await db.BotActions.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreateBotAction(BotAction botAction)
        {
            if (string.IsNullOrWhiteSpace(botAction.Command))
            {
                throw new UnprocessableEntityException("Command is required");
            }
            if (string.IsNullOrWhiteSpace(botAction.Action))
            {
                throw new UnprocessableEntityException("Action is required");
            }
            if (string.IsNullOrWhiteSpace(botAction.Parameters))
            {
                throw new UnprocessableEntityException("Parameters are required");
            }
            if (await db.BotActions.AnyAsync(x =>
                x.Command == botAction.Command))
            {
                throw new ConflictException("Bot command already exists");
            }

            await db.BotActions.AddAsync(botAction);
            await db.SaveChangesAsync();
        }

        public async Task UpdateBotAction(int id, BotAction botAction)
        {
            var entity = await GetBotAction(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Bot action id '{id}' not found.");
            }

            if (string.IsNullOrWhiteSpace(botAction.Command))
            {
                throw new UnprocessableEntityException("Command is required");
            }
            if (string.IsNullOrWhiteSpace(botAction.Action))
            {
                throw new UnprocessableEntityException("Action is required");
            }
            if (string.IsNullOrWhiteSpace(botAction.Parameters))
            {
                throw new UnprocessableEntityException("Parameters are required");
            }
            if (await db.BotActions.AnyAsync(x =>
                x.Id != id
                && x.Command == botAction.Command))
            {
                throw new ConflictException("Bot command already exists");
            }

            db.Entry(entity).CurrentValues.SetValues(botAction);
            await db.SaveChangesAsync();
        }

        public async Task DeleteBotAction(int id)
        {
            var entity = await GetBotAction(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Bot action id '{id}' not found.");
            }

            db.BotActions.Remove(entity);
            await db.SaveChangesAsync();
        }

        public async Task<bool> IsBotCommand(ChatMessage message)
        {
            return await GetBotActions().AnyAsync(x => message.Message.Contains(x.Command));
        }
    }
}
