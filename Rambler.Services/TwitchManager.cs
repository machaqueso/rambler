using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public async Task<TwitchUserData> GetUser()
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

            var response = await api.Get("https://api.twitch.tv/helix/users", token.access_token, configurationService.GetValue("Authentication:Twitch:ClientId").Result);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Error refreshing token: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            var userData = JsonConvert.DeserializeObject<TwitchUserResponse>(content);

            return userData.data.FirstOrDefault();
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

            var response = await api.Get($"https://api.twitch.tv/helix/users?login={username}", token.access_token, configurationService.GetValue("Authentication:Twitch:ClientId").Result);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Twitch API error: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            var twitchResponse = JsonConvert.DeserializeObject<TwitchUserResponse>(content);
            if (twitchResponse.data != null && twitchResponse.data.Any())
            {
                var user = twitchResponse.data.First();
                await db.TwitchUsers.AddAsync(new TwitchUser
                {
                    _id = Convert.ToUInt64(user.id),
                    name = user.login,
                    display_name = user.display_name,
                    email = user.email,
                    type = user.broadcaster_type,
                    bio = user.description,
                    created_at = user.created_at
                });
                await db.SaveChangesAsync();
            }

            return null;
        }

        public IQueryable<TwitchUser> GetAuthors()
        {
            return db.TwitchUsers;
        }

        public async Task ImportEmoticons()
        {
            var token = await db.AccessTokens.FirstOrDefaultAsync(x => x.ApiSource == ApiSource.Twitch);
            if (token == null)
            {
                throw new UnauthorizedAccessException("access token not found");
            }

            var emoticonSetResponse = await api.Get($"https://api.twitch.tv/helix/chat/emotes/global",
                token.access_token,
                configurationService.GetValue("Authentication:Twitch:ClientId").Result);

            var jsonContent = await emoticonSetResponse.Content.ReadAsStringAsync();
            var twitchEmote = JsonConvert.DeserializeObject<TwitchEmote>(jsonContent);

            db.Emoticons.RemoveRange(db.Emoticons.Where(x => x.ApiSource == ApiSource.Twitch));
            await db.SaveChangesAsync();

            foreach (var item in twitchEmote.data)
            {
                db.Emoticons.Add(new Emoticon
                {
                    Regex = item.name,
                    Url = item.images.url_1x,
                    SourceId = item.id,
                    ApiSource = ApiSource.Twitch
                });
            }
            await db.SaveChangesAsync();
        }
    }
}