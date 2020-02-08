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

        public async Task AuthorAction(int id, string action)
        {
            var author = await GetAuthors()
                .Include(x => x.AuthorFilters)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (author == null)
            {
                throw new InvalidOperationException($"Author Id {id} not found.");
            }

            if (action == ActionTypes.Upvote)
            {
                author.Score += 1;
                await db.SaveChangesAsync();
                return;
            }

            if (action == ActionTypes.Downvote)
            {
                author.Score -= 1;
                await db.SaveChangesAsync();
                return;
            }

            if (action == ActionTypes.Whitelist)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Whitelist))
                {
                    await RemoveFilter(id, FilterTypes.Whitelist);
                }
                await AddFilter(id, FilterTypes.Whitelist);
                return;
            }

            if (action == ActionTypes.Ignore)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Ignorelist))
                {
                    await RemoveFilter(id, FilterTypes.Ignorelist);
                }
                await AddFilter(id, FilterTypes.Ignorelist);
                return;
            }

            if (action == ActionTypes.Blacklist)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Blacklist))
                {
                    await RemoveFilter(id, FilterTypes.Blacklist);
                }
                await AddFilter(id, FilterTypes.Blacklist);
                return;
            }

            if (action == ActionTypes.Ban)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Banlist))
                {
                    await RemoveFilter(id, FilterTypes.Banlist);
                }
                await AddFilter(id, FilterTypes.Banlist);
            }
        }

        public async Task DeleteFilter(int id)
        {
            var entity = await db.AuthorFilters.SingleOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                throw new InvalidOperationException($"Filter id '{id}' not found.");
            }

            db.AuthorFilters.Remove(entity);
            await db.SaveChangesAsync();
        }

        public async Task Create(Author messageAuthor)
        {
            await db.Authors.AddAsync(messageAuthor);
            await db.SaveChangesAsync();
        }
    }
}