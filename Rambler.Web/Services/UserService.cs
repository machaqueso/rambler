using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Web.Services
{
    public class UserService
    {
        private readonly DataContext db;

        public UserService(DataContext db)
        {
            this.db = db;
        }

        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        public async Task<User> GetCurrentUser()
        {
            // HACK: get first (and only) user for now
            var user = await GetUsers()
                .Include(x => x.AccessTokens)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User();
                await Create(user);
            }

            return user;
        }

        public async Task AddToken(int id, string apiSource, AccessToken token)
        {
            var user = await db.Users.Include(x => x.AccessTokens)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new InvalidOperationException("userid not found");
            }

            var existingToken = user.AccessTokens.SingleOrDefault(x => x.ApiSource == apiSource);
            if (existingToken != null)
            {
                await UpdateToken(existingToken);
                return;
            }

            token.ExpirationDate = DateTime.UtcNow.AddSeconds(token.expires_in);
            token.ApiSource = apiSource;
            user.AccessTokens.Add(token);
            await db.SaveChangesAsync();
        }

        public async Task UpdateToken(AccessToken token)
        {
            var existingToken = await db.AccessTokens
                .FirstOrDefaultAsync(x => x.Id == token.Id);

            if (existingToken == null)
            {
                throw new InvalidOperationException("token not found");
            }

            existingToken.access_token = token.access_token;
            // Youtube doesn't send a new refresh_token
            // existingToken.refresh_token = token.refresh_token;
            existingToken.expires_in = token.expires_in;
            existingToken.ExpirationDate = DateTime.UtcNow.AddSeconds(token.expires_in);
            await db.SaveChangesAsync();
        }

        public async Task Create(User user)
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        public async Task DeleteToken(int id)
        {
            var existingToken = await db.AccessTokens
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingToken == null)
            {
                throw new InvalidOperationException("token not found");
            }

            db.AccessTokens.Remove(existingToken);
            await db.SaveChangesAsync();
        }

    }
}