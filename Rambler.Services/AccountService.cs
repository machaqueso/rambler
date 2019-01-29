using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Rambler.Data;
using Rambler.Models;

namespace Rambler.Services
{
    public class AccountService
    {
        private readonly DataContext db;

        private const int IterationCount = 10000;
        private readonly RandomNumberGenerator rng;

        public AccountService(DataContext db)
        {
            this.db = db;

            rng = new RNGCryptoServiceProvider();
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

            user.PasswordHash = Convert.ToBase64String(HashPasswordV3(user.Password, rng));

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

        public async Task ChangePassword(int id, User user)
        {
            var entity = await GetUser(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"User id '{id}' not found.");
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

            entity.PasswordHash = Convert.ToBase64String(HashPasswordV3(user.Password, rng));
            await db.SaveChangesAsync();
        }

        public async Task<User> FindByUsername(string username)
        {
            return await GetUsers().FirstOrDefaultAsync(x => string.Equals(x.UserName, username, StringComparison.CurrentCultureIgnoreCase)
                                                             || string.Equals(x.Email, username, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool VerifyPassword(User user, string password)
        {
            var passwordHash = Convert.ToBase64String(HashPasswordV3(password, rng));
            return user.PasswordHash == passwordHash;
        }

        // stolen from Microsoft's identity: https://github.com/aspnet/Identity/blob/master/src/Core/PasswordHasher.cs
        private byte[] HashPasswordV3(string password, RandomNumberGenerator rng)
        {
            return HashPasswordV3(password, rng,
                prf: KeyDerivationPrf.HMACSHA256,
                iterCount: IterationCount,
                saltSize: 128 / 8,
                numBytesRequested: 256 / 8);
        }

        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            // Produce a version 3 (see comment above) text hash.
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}