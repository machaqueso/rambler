using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Web.Data;
using Rambler.Web.Models;

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

        public async Task AddToken(int id, string apiSource, AccessToken token)
        {
            var user = await db.Users.Include(x => x.AccessTokens)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new InvalidOperationException("userid not found");
            }

            if (user.AccessTokens.Any(x => x.ApiSource == apiSource))
            {
                throw new InvalidOperationException("token exists, use refresh instead");
            }

            token.ExpirationDate = DateTime.UtcNow.AddSeconds(token.expires_in);
            token.ApiSource = apiSource;
            user.AccessTokens.Add(token);
            await db.SaveChangesAsync();
        }

        public async Task RefreshToken(AccessToken token)
        {
            var existingToken = await db.AccessTokens
                .FirstOrDefaultAsync(x => x.Id == token.Id);

            if (existingToken == null)
            {
                throw new InvalidOperationException("token not found");
            }

            existingToken.access_token = token.access_token;
            existingToken.refresh_token = token.refresh_token;
            existingToken.expires_in = token.expires_in;
            existingToken.ExpirationDate = DateTime.UtcNow.AddSeconds(token.expires_in);
            await db.SaveChangesAsync();
        }

        public async Task Create(User user)
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
    }
}