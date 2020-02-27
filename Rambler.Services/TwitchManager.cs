using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Rambler.Data;
using Rambler.Models;
using Rambler.Models.Twitch;

namespace Rambler.Services
{
    public class TwitchManager
    {
        private readonly TwitchAPIv5 api;
        private readonly DataContext db;
        private readonly ConfigurationService configurationService;


        public TwitchManager(TwitchAPIv5 api, DataContext db, ConfigurationService configurationService)
        {
            this.api = api;
            this.db = db;
            this.configurationService = configurationService;
        }

        public async Task<TwitchUser> GetUser()
        {
            var token = await db.AccessTokens.FirstOrDefaultAsync(x => x.ApiSource == ApiSource.Twitch);
            if (token == null)
            {
                throw new UnauthorizedAccessException("access token not found");
            }

            if (token.Status == AccessTokenStatus.Expired)
            {
                throw new UnauthorizedAccessException("access token expired");
            }


            var response = await api.Get("https://api.twitch.tv/kraken/user", token.access_token,
                configurationService.GetValue("Authentication:Twitch:ClientId").Result);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Error refreshing token: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            return JsonSerializer.Deserialize<TwitchUser>(content);
        }

        public async Task<TwitchUser> FindUser(string username)
        {
            var cachedUser = await db.TwitchUsers.SingleOrDefaultAsync(x => x.name == username);
            if (cachedUser != null)
            {
                return cachedUser;
            }

            var token = await db.AccessTokens.FirstOrDefaultAsync(x => x.ApiSource == ApiSource.Twitch);
            if (token == null)
            {
                throw new UnauthorizedAccessException("access token not found");
            }

            if (token.Status == AccessTokenStatus.Expired)
            {
                throw new UnauthorizedAccessException("access token expired");
            }

            var response = await api.Get($"https://api.twitch.tv/kraken/users?login={username}", token.access_token,
                configurationService.GetValue("Authentication:Twitch:ClientId").Result);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Error refreshing token: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            var twitchResponse = JsonSerializer.Deserialize<TwitchGetUsersResponse>(content);
            if (twitchResponse.users.Any())
            {
                var user = twitchResponse.users.First();
                await db.TwitchUsers.AddAsync(user);
                await db.SaveChangesAsync();
            }

            return null;
        }

        public IQueryable<TwitchUser> GetAuthors()
        {
            return db.TwitchUsers;
        }
    }
}