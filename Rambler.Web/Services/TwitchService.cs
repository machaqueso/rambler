using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rambler.Web.Models;

namespace Rambler.Web.Services
{
    public class TwitchService
    {
        private readonly UserService userService;
        private readonly ILogger<TwitchService> logger;
        public IConfiguration configuration { get; }
        public ChatService chatService;

        public TwitchService(UserService userService, ILogger<TwitchService> logger, IConfiguration configuration, ChatService chatService)
        {
            this.userService = userService;
            this.logger = logger;
            this.configuration = configuration;
            this.chatService = chatService;
        }

        public async Task<HttpResponseMessage> Get(string request)
        {
            return await Get(request, string.Empty);
        }

        public async Task<HttpResponseMessage> Get(string request, string accessToken)
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await client.GetAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> Post(string url, IList<KeyValuePair<string, string>> data)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(data);

                var response = await client.PostAsync(url, content);
                return response;
            }
        }

        public async Task<AccessToken> GetToken()
        {
            var user = await userService.GetCurrentUser();

            var token = user.AccessTokens
                .FirstOrDefault(x => x.ApiSource == ApiSource.Twitch);

            return token;
        }

        public bool IsValidToken(AccessToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return token.Status == AccessTokenStatus.Ok
                   && !string.IsNullOrEmpty(token.access_token);
        }

        public async Task<bool> HasValidToken()
        {
            var token = await GetToken();

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return token.Status == AccessTokenStatus.Ok
                   && !string.IsNullOrEmpty(token.access_token);
        }


        public async Task RefreshToken(AccessToken token)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", configuration["Authentication:Twitch:ClientId"]),
                new KeyValuePair<string, string>("client_secret", configuration["Authentication:Twitch:ClientSecret"]),
                new KeyValuePair<string, string>("refresh_token", token.refresh_token),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            };

            var response = await Post("https://id.twitch.tv/oauth2/token", data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Error refreshing token: {response.StatusCode} - {response.ReasonPhrase}\n{content}");
            }

            var accessToken = JsonConvert.DeserializeObject<AccessToken>(content);
            accessToken.Id = token.Id;
            await userService.UpdateToken(accessToken);
        }

        public async Task ProcessMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var prefix = string.Empty;
            var command = string.Empty;
            var parameters = string.Empty;
            var author = string.Empty;

            if (message.Contains(":") && !message.StartsWith(":"))
            {
                message = message.Substring(message.IndexOf(':'));
            }

            if (message.StartsWith(":"))
            {
                prefix = message.Substring(0, message.IndexOf(" ", StringComparison.Ordinal));
                var parts = message.Split(" ");
                command = parts[1];
                parameters = string.Join(' ', parts.Skip(2));
            }
            else
            {
                var parts = message.Split(" ");
                command = parts[0];
                parameters = string.Join(' ', parts.Skip(1));
            }

            if (prefix.Contains("!"))
            {
                var index = prefix.IndexOf('!') - 1;
                if (index < 1)
                {
                    index = 1;
                }
                author = prefix.Substring(1, index);
            }

            if (command != "PRIVMSG")
            {
                return;
            }

            var displayMessage = parameters.Substring(parameters.IndexOf(':') + 1);

            await chatService.CreateMessage(new ChatMessage
            {
                Author = author,
                Message = displayMessage,
                Date = DateTime.UtcNow,
                Source = ApiSource.Twitch
            });
        }
    }
};