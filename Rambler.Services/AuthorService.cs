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
    public class AuthorService
    {
        private readonly DataContext db;
        private readonly ILogger<AuthorService> logger;

        public AuthorService(DataContext db, ILogger<AuthorService> logger)
        {
            this.db = db;
            this.logger = logger;
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

        public async Task<string> AuthorAction(int id, string action)
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
                return $"{author.Name} upvoted to {author.Score}";
            }

            if (action == ActionTypes.Downvote)
            {
                author.Score -= 1;
                await db.SaveChangesAsync();
                return $"{author.Name} downvoted to {author.Score}";
            }

            if (action == ActionTypes.Whitelist)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Whitelist))
                {
                    await RemoveFilter(id, FilterTypes.Whitelist);
                    return $"{author.Name} removed from {action}";
                }
                await AddFilter(id, FilterTypes.Whitelist);
                return $"{author.Name} added to {action}";
            }

            if (action == ActionTypes.Ignore)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Ignorelist))
                {
                    await RemoveFilter(id, FilterTypes.Ignorelist);
                    return $"{author.Name} removed from {action}";
                }
                await AddFilter(id, FilterTypes.Ignorelist);
                return $"{author.Name} added to {action}";
            }

            if (action == ActionTypes.Blacklist)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Blacklist))
                {
                    await RemoveFilter(id, FilterTypes.Blacklist);
                    return $"{author.Name} removed from {action}";
                }
                await AddFilter(id, FilterTypes.Blacklist);
                return $"{author.Name} added to {action}";
            }

            if (action == ActionTypes.Ban)
            {
                if (author.AuthorFilters.Any(x => x.FilterType == FilterTypes.Banlist))
                {
                    await RemoveFilter(id, FilterTypes.Banlist);
                    return $"{author.Name} removed from {action}";
                }
                await AddFilter(id, FilterTypes.Banlist);
                return $"{author.Name} added to {action}";
            }

            throw new NotImplementedException($"Action '{action}' is not implemented.");
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

        public async Task Create(Author author)
        {
            await db.Authors.AddAsync(author);
            await db.SaveChangesAsync();
        }

        public bool IsValid(Author author)
        {
            if (author == null)
            {
                return false;
            }

            if (author.Id > 0)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(author.Source)
                   && !string.IsNullOrWhiteSpace(author.SourceAuthorId)
                && !string.IsNullOrWhiteSpace(author.Name);

        }

        public async Task<Author> EnsureAuthor(int id, Author author)
        {
            if (id > 0)
            {
                return await GetAuthors().SingleOrDefaultAsync(x => x.Id == id);
            }

            if (!IsValid(author))
            {
                throw new UnprocessableEntityException("Author's Source, SourceAuthorId and/or Name are not defined");
            }

            var authors = GetAuthors().Where(x =>
                x.Source == author.Source
                && x.SourceAuthorId == author.SourceAuthorId
                && x.Name == author.Name);

            if (authors.Count() > 1)
            {
                // workaround to my previous terrible coding: warns about duplicate authors and keeps going
                // TODO: add a way to cleanup these from database
                logger.LogWarning($"Author has multiple records in database: Source='{author.Source}', SourceAuthorId='{author.SourceAuthorId}', Name='{author.Name}'");
                return await authors.FirstOrDefaultAsync();
            }

            if (await authors.AnyAsync())
            {
                return await authors.SingleOrDefaultAsync();
            }

            await Create(author);
            return author;
        }

        public async Task Update(Author author)
        {
            var entity = db.Authors.Find(author.Id);
            if (entity == null)
            {
                return;
            }

            db.Entry(entity).CurrentValues.SetValues(author);
            await db.SaveChangesAsync();
        }
    }
}