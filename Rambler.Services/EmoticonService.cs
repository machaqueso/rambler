using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Services
{
    public class EmoticonService
    {
        private readonly DataContext db;

        public EmoticonService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<Emoticon> GetEmoticons()
        {
            return db.Emoticons;
        }

        public async Task<Emoticon> GetEmoticon(int id)
        {
            return await db.Emoticons.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreateEmoticon(Emoticon Emoticon)
        {
            db.Emoticons.Add(Emoticon);
            await db.SaveChangesAsync();
        }

        public async Task UpdateEmoticon(int id, Emoticon Emoticon)
        {
            var entity = await GetEmoticon(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Emoticon id '{id}' not found.");
            }

            db.Entry(entity).CurrentValues.SetValues(Emoticon);
            await db.SaveChangesAsync();
        }

        public async Task DeleteEmoticon(int id)
        {
            var entity = await GetEmoticon(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Emoticon id '{id}' not found.");
            }

            db.Emoticons.Remove(entity);
            await db.SaveChangesAsync();
        }


    }
}