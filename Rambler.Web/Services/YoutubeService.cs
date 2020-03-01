using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rambler.Models;
using Rambler.Models.Youtube.LiveBroadcast;
using Rambler.Models.Youtube.LiveChat;
using Rambler.Services;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Rambler.Web.Services
{
    public class YoutubeService
    {
        private readonly UserService userService;
        private readonly ILogger<YoutubeService> logger;
        public readonly ConfigurationService configurationService;
        public readonly IntegrationService IntegrationService;

        public YoutubeService(UserService userService, ILogger<YoutubeService> logger,
            ConfigurationService configurationService, IntegrationService integrationService)
        {
            this.userService = userService;
            this.logger = logger;
            this.configurationService = configurationService;
            IntegrationService = integrationService;
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
                .FirstOrDefault(x => x.ApiSource == ApiSource.Youtube);

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
            return IsValidToken(token);
        }

        public async Task<LiveChatMessageList> GetLiveChatMessages(string liveChatId, string nextPageToken)
        {
            if (string.IsNullOrEmpty(liveChatId))
            {
                throw new ArgumentNullException(liveChatId);
            }

            var token = await GetToken();
            if (token != null && token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
            {
                await RefreshToken(token);
            }
            if (!IsValidToken(token))
            {
                logger.LogWarning($"[YoutubeService:GetLiveChatMessages] Invalid Token");
                return null;
            }

            var url = $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&part=id,snippet,authorDetails";
            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                logger.LogDebug($"calling GetLiveChatMessages with nextPageToken='{nextPageToken}'");
                url += $"&nextPageToken={nextPageToken}";
            }

            var response = await Get(url, token.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"GetLiveChatMessages error: {(int)response.StatusCode} - {response.ReasonPhrase}", response);
                logger.LogDebug($"Content: {content}");
                return null;
            }

            var liveChatMessageList = JsonConvert.DeserializeObject<LiveChatMessageList>(content);
            return liveChatMessageList;
        }

        public ChatMessage MapToChatMessage(LiveChatMessage item)
        {
            return new ChatMessage
            {
                Message = item.snippet.displayMessage,
                Date = item.snippet.publishedAt,
                Source = ApiSource.Youtube,
                SourceMessageId = item.id,
                Author = new Author
                {
                    Name = item.AuthorDetails.displayName,
                    Source = ApiSource.Youtube,
                    SourceAuthorId = item.AuthorDetails.channelId
                },
                Type = item.snippet.type
            };
        }

        public async Task<LiveBroadcastItem> GetLiveBroadcast(string id = null)
        {
            var token = await GetToken();
            if (token != null && token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
            {
                await RefreshToken(token);
            }

            if (!IsValidToken(token))
            {
                return null;
            }

            var url = "https://www.googleapis.com/youtube/v3/liveBroadcasts?part=snippet,status&broadcastType=persistent&mine=true";
            if (!string.IsNullOrWhiteSpace(id))
            {
                url += $"&id={id}";
            }

            var response = await Get(url, token.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    $"GetLiveBroadcast error: {(int)response.StatusCode} - {response.ReasonPhrase}: {content}");
                return null;
            }

            var liveBroadcastList = JsonConvert.DeserializeObject<LiveBroadcastList>(content);
            if (liveBroadcastList?.items == null || !liveBroadcastList.items.Any())
            {
                return null;
            }

            if (liveBroadcastList.items.Count() > 1 && liveBroadcastList.items.Any(x => x.status.lifeCycleStatus == "live"))
            {
                return liveBroadcastList.items.FirstOrDefault(x => x.status.lifeCycleStatus == "live");
            }

            return liveBroadcastList.items.FirstOrDefault();
        }

        public async Task RefreshToken(AccessToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (!token.HasRefreshToken)
            {
                throw new InvalidOperationException("invalid refresh_token");
            }

            var clientId = await configurationService.GetValue("Authentication:Google:ClientId");
            var clientSecret = await configurationService.GetValue("Authentication:Google:ClientSecret");

            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", token.refresh_token),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            };

            var response = await Post("https://accounts.google.com/o/oauth2/token", data);
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

        public bool IsConfigured()
        {
            return configurationService.HasValue("Authentication:Google:ClientId")
                   && configurationService.HasValue("Authentication:Google:ClientSecret");
        }

        public async Task<bool> IsEnabled()
        {
            return await IntegrationService.IsEnabled(ApiSource.Youtube);
        }

        public async Task InsertLiveChatMessages(string liveChatId, string messageText)
        {
            if (string.IsNullOrEmpty(liveChatId))
            {
                throw new ArgumentNullException(liveChatId);
            }

            var token = await GetToken();
            if (token != null && token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
            {
                await RefreshToken(token);
            }
            if (!IsValidToken(token))
            {
                logger.LogWarning($"[YoutubeService:GetLiveChatMessages] Invalid Token");
                return;
            }

            var url = $"https://www.googleapis.com/youtube/v3/liveChat/messages?part=snippet";

            var liveChatMessage = new
            {
                snippet = new
                {
                    liveChatId = liveChatId,
                    type = "textMessageEvent",
                    textMessageDetails = new
                    {
                        messageText = messageText
                    }
                }
            };

            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(token.access_token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
                }

                var json = JsonConvert.SerializeObject(liveChatMessage);
                logger.LogInformation($"InsertLiveChatMessages {url}\n{json}");
                var data = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                response = await client.PostAsync(url, data);
            }

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"InsertLiveChatMessages error: {(int)response.StatusCode} - {response.ReasonPhrase}", response);
                logger.LogError($"Content: {content}");
            }

        }

    }
};