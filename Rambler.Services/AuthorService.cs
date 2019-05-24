using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Services
{
    public class AuthorService
    {
        private readonly DataContext db;

        public AuthorService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<Author> GetAuthors()
        {
            return db.Authors;
        }

        public async Task AddFilter(int id, string filterType)
        {
            var entity = await GetAuthors().SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new InvalidOperationException("Author not found");
            }

            if (entity.AuthorFilters.Any(x => x.FilterType == filterType))
            {
                return;
            }

            entity.AuthorFilters.Add(new AuthorFilter
            {
                FilterType = filterType,
                Date = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        public async Task RemoveFilter(int id, string filterType)
        {
            var entity = await GetAuthors().SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                throw new InvalidOperationException("Author not found");
            }

            var filter = entity.AuthorFilters.SingleOrDefault(x => x.FilterType == filterType);
            if (filter == null)
            {
                return;
            }

            entity.AuthorFilters.Remove(filter);
            await db.SaveChangesAsync();
        }

        public IQueryable<AuthorFilter> GetFilters()
        {
            return db.AuthorFilters;
        }

    }
}