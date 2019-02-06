using System;
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

        public async Task<TwitchUser> GetUser(AccessToken token)
        {
            var response = await api.Get("https://api.twitch.tv/kraken/user", token.access_token, configurationService.GetValue("Authentication:Twitch:ClientId").Result);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Error refreshing token: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            return JsonConvert.DeserializeObject<TwitchUser>(content);
        }
    }
}