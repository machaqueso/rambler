using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Exceptions;

namespace Rambler.Services
{
    public class AccountService
    {
        private readonly DataContext db;
        private readonly PasswordService passwordService;

        public AccountService(DataContext db, PasswordService passwordService)
        {
            this.db = db;
            this.passwordService = passwordService;
        }

        public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        public async Task<User> GetUser(int id)
        {
            return await db.Users.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreateUser(User user)
        {
            if (await GetUsers().AnyAsync(x => x.UserName == user.UserName))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                throw new InvalidOperationException("Password cannot be null");
            }

            if (string.IsNullOrEmpty(user.ConfirmPassword))
            {
                throw new InvalidOperationException("Confirm password cannot be null");
            }

            if (user.Password != user.ConfirmPassword)
            {
                throw new InvalidOperationException("Passwords do not match");
            }

            user.PasswordHash = Convert.ToBase64String(passwordService.HashPasswordV3(user.Password));

            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        public async Task UpdateUser(int id, User user)
        {
            var entity = await GetUser(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"User id '{id}' not found.");
            }

            db.Entry(entity).CurrentValues.SetValues(user);
            await db.SaveChangesAsync();
        }

        public async Task ChangePassword(User user)
        {
            if (string.IsNullOrEmpty(user.OldPassword))
            {
                throw new InvalidOperationException("Old password cannot be null");
            }

            if (!VerifyPassword(user, user.OldPassword))
            {
                throw new InvalidOperationException("Old password is wrong");
            }

            await SetPassword(user);
        }

        public async Task SetPassword(User user)
        {
            var entity = await GetUser(user.Id);
            if (entity == null)
            {
                throw new UnprocessableEntityException($"User id '{user.Id}' not found.");
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                throw new UnprocessableEntityException("Password cannot be null");
            }

            if (string.IsNullOrEmpty(user.ConfirmPassword))
            {
                throw new UnprocessableEntityException("Confirm password cannot be null");
            }

            if (user.Password != user.ConfirmPassword)
            {
                throw new UnprocessableEntityException("Passwords do not match");
            }

            entity.PasswordHash = Convert.ToBase64String(passwordService.HashPasswordV3(user.Password));
            entity.MustChangePassword = false;
            await db.SaveChangesAsync();
        }

        public async Task<User> FindByUsername(string username)
        {
            return await GetUsers().FirstOrDefaultAsync(x =>
                string.Equals(x.UserName, username, StringComparison.CurrentCultureIgnoreCase)
                || string.Equals(x.Email, username, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool VerifyPassword(User user, string password)
        {
            byte[] decodedHashedPassword = Convert.FromBase64String(user.PasswordHash);
            return passwordService.VerifyHashedPasswordV3(decodedHashedPassword, password, out int count);
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

    }
}