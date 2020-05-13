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

            return JsonConvert.DeserializeObject<TwitchUser>(content);
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
                    $"Twitch API error: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            var twitchResponse = JsonConvert.DeserializeObject<TwitchGetUsersResponse>(content);
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

        public async Task ImportEmoticons()
        {
            var token = await db.AccessTokens.FirstOrDefaultAsync(x => x.ApiSource == ApiSource.Twitch);
            if (token == null)
            {
                throw new UnauthorizedAccessException("access token not found");
            }

            var user = await GetUser();

            var emoticonSetResponse = await api.Get($"https://api.twitch.tv/kraken/users/{user._id}/emotes",
                token.access_token,
                configurationService.GetValue("Authentication:Twitch:ClientId").Result);

            var jsonContent = await emoticonSetResponse.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TwitchUserEmoticonSetResponse>(jsonContent);

            var emoticonSets = new List<string>();
            foreach (var item in data.emoticon_sets)
            {
                emoticonSets.Add(item.Name);
            }

            var response = await api.GetNoAuth($"https://api.twitch.tv/kraken/chat/emoticons",
                configurationService.GetValue("Authentication:Twitch:ClientId").Result);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Twitch API error: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            var twitchResponse = JsonConvert.DeserializeObject<TwitchChatEmoticonsResponse>(content);

            db.Emoticons.RemoveRange(db.Emoticons.Where(x => x.ApiSource == ApiSource.Twitch));
            await db.SaveChangesAsync();

            foreach (var item in twitchResponse.emoticons
                .Where(x => emoticonSets.Contains(x.images.emoticon_set))
                .Take(1000))
            {
                //var emoticon = await db.Emoticons.SingleOrDefaultAsync(x =>
                //    x.ApiSource == ApiSource.Twitch && x.SourceId == item.id.ToString());

                //if (emoticon == null)
                //{
                db.Emoticons.Add(new Emoticon
                {
                    Regex = item.regex,
                    Url = item.images.url,
                    SourceId = item.id.ToString(),
                    ApiSource = ApiSource.Twitch
                });
                //    continue;
                //}

                //emoticon.Regex = item.regex;
                //emoticon.Url = item.images.url;
                //await db.SaveChangesAsync();
            }
            await db.SaveChangesAsync();
        }
    }
}