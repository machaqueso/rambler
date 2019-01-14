using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rambler.Web.Models;

namespace Rambler.Web.Services
{
    public class TwitchService
    {
        private readonly UserService userService;
        private readonly ILogger<TwitchService> logger;

        public TwitchService(UserService userService, ILogger<TwitchService> logger)
        {
            this.userService = userService;
            this.logger = logger;
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

        //public ChatMessage MapToChatMessage(TwitchChatMessage item)
        //{
        //    return new ChatMessage
        //    {
        //        Author = item.AuthorDetails.displayName,
        //        Message = item.snippet.displayMessage,
        //        Date = item.snippet.publishedAt,
        //        Source = "Youtube",
        //        SourceMessageId = item.id,
        //        SourceAuthorId = item.AuthorDetails.channelId
        //    };
        //}
    }
};