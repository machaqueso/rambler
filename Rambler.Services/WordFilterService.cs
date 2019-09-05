using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Services
{
    public class WordFilterService
    {
        private readonly DataContext db;

        public WordFilterService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<WordFilter> GetWordFilters()
        {
            return db.WordFilters;
        }

        public async Task<WordFilter> GetWordFilter(int id)
        {
            return await db.WordFilters.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreateWordFilter(WordFilter wordFilter)
        {
            db.WordFilters.Add(wordFilter);
            await db.SaveChangesAsync();
        }

        public async Task UpdateWordFilter(int id, WordFilter wordFilter)
        {
            var entity = await GetWordFilter(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"WordFilter id '{id}' not found.");
            }

            db.Entry(entity).CurrentValues.SetValues(wordFilter);
            await db.SaveChangesAsync();
        }

        public async Task DeleteWordFilter(int id)
        {
            var entity = await GetWordFilter(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"WordFilter id '{id}' not found.");
            }

            db.WordFilters.Remove(entity);
            await db.SaveChangesAsync();
        }


    }
}