using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rambler.Web.Models;
using Rambler.Web.Models.Youtube.LiveChat;

namespace Rambler.Web.Services
{
    public class YoutubeService
    {
        private readonly UserService userService;

        public YoutubeService(UserService userService)
        {
            this.userService = userService;
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

        public async Task<string> Post(string url, IList<KeyValuePair<string, string>> data)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(data);

                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"POST failed with code: {response.StatusCode}: {responseContent}");
                }

                return responseContent;
            }
        }

        public async Task<User> GetCurrentUser(int id)
        {
            var user = await userService.GetUsers()
                .Include(x => x.GoogleToken)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (DateTime.UtcNow > user.GoogleToken.ExpirationDate)
            {
                return null;
            }

            return user;
        }

        public async Task<IEnumerable<ChatMessage>> GetLiveChatMessages(int id)
        {
            var user = await GetCurrentUser(id);

            var response = await Get(
                $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={id}&part=id,snippet,authorDetails",
                user.GoogleToken.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Youtube API error: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var liveChatMessageList = JsonConvert.DeserializeObject<MessageList>(content);

            return liveChatMessageList.items
                .Where(x => x.snippet.hasDisplayContent)
                .Select(x => new ChatMessage
                {
                    Author = x.AuthorDetails.displayName,
                    Message = x.snippet.displayMessage,
                    Date = x.snippet.publishedAt,
                    Source = "Youtube",
                    SourceMessageId = x.id,
                    SourceAuthorId = x.AuthorDetails.channelId,
                    PollingInterval = liveChatMessageList.pollingIntervalMillis
                });
        }

    }
};