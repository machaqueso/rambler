using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Rambler.Web.Data;
using Rambler.Web.Hubs;
using Rambler.Web.Models;

namespace Rambler.Web.Controllers
{
    public class YoutubeController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly ChatHub chatHub;

        public YoutubeController(IConfiguration configuration, ChatHub chatHub)
        {
            Configuration = configuration;
            this.chatHub = chatHub;
        }

        public async Task<IActionResult> Index()
        {
            using (var db = new DataContext())
            {
                var user = await db.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User();
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }

                user = await db.Users.FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(Request.Cookies["UserId"]))
                {
                    Response.Cookies.Append("UserId", user.Id.ToString());
                }
                else
                {
                    user = await db.Users
                        .Include(x => x.GoogleToken)
                        .FirstAsync(x => x.Id == Convert.ToInt32(Request.Cookies["UserId"]));
                }

                if (user.GoogleToken == null)
                {
                    return Authorize();
                }

                if (DateTime.Now > user.GoogleToken.ExpirationDate)
                {
                    await GoogleRefresh(user);
                }

                return View(user);
            }
        }

        public async Task<IActionResult> Callback(string code)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", Configuration["Authentication:Google:ClientId"]),
                new KeyValuePair<string, string>("client_secret", Configuration["Authentication:Google:ClientSecret"]),
                new KeyValuePair<string, string>("redirect_uri",
                    Url.Action("Callback", "Youtube", null, Request.Scheme, null)),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

            var response = await Post("https://accounts.google.com/o/oauth2/token", data);

            var googleToken = JsonConvert.DeserializeObject<GoogleToken>(response);

            using (var db = new DataContext())
            {
                var user = db.Users.First(x => x.Id == System.Convert.ToInt32(Request.Cookies["UserId"]));
                googleToken.UserId = user.Id;
                user.GoogleToken = googleToken;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Authorize()
        {
            var clientId = Configuration["Authentication:Google:ClientId"];
            var clientSecret = Configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId))
            {
                throw new InvalidOperationException("Authentication:Google:ClientId not configured");
            }

            var redirectUrl = WebUtility.UrlEncode(Url.Action("Callback", "Youtube", null, Request.Scheme, null));
            var oauthRequest =
                $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUrl}&scope=https://www.googleapis.com/auth/youtube.readonly&response_type=code&access_type=offline";

            return Redirect(oauthRequest);
        }

        public async Task<IActionResult> Broadcast()
        {
            User user;

            using (var db = new DataContext())
            {
                user =
                    await
                        db.Users.Include(x => x.GoogleToken)
                            .FirstAsync(x => x.Id == System.Convert.ToInt32(Request.Cookies["UserId"]));

                if (DateTime.Now > user.GoogleToken.ExpirationDate)
                {
                    await GoogleRefresh(user);
                }
            }

            var response = await Get(
                "https://www.googleapis.com/youtube/v3/liveBroadcasts?part=snippet&broadcastType=persistent&mine=true",
                user.GoogleToken.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            var liveBroadcastList = JsonConvert.DeserializeObject<LiveBroadcastList>(content);
            return View("Broadcast", liveBroadcastList);
        }

        public async Task<IActionResult> Messages(string id)
        {
            User user;

            using (var db = new DataContext())
            {
                user = await db.Users.Include(x => x.GoogleToken)
                            .FirstAsync(x => x.Id == System.Convert.ToInt32(Request.Cookies["UserId"]));

                if (DateTime.Now > user.GoogleToken.ExpirationDate)
                {
                    await GoogleRefresh(user);
                }
            }

            var response = await Get($"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={id}&part=id,snippet,authorDetails",
                        user.GoogleToken.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            var liveChatMessageList = JsonConvert.DeserializeObject<LiveChatMessageList>(content);

            foreach (var item in liveChatMessageList.items)
            {
                await chatHub.Clients.All.SendAsync("ReceiveMessage", "youtube", item.snippet.textMessageDetails.messageText);
            }

            return View("Messages", liveChatMessageList);
        }


        private async Task<HttpResponseMessage> Get(string request)
        {
            return await Get(request, string.Empty);
        }

        private async Task<HttpResponseMessage> Get(string request, string accessToken)
        {
            var client = new HttpClient();
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(request),
                Method = HttpMethod.Get,
            };

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await client.GetAsync(request);
            return response;
        }

        private async Task<string> Post(string url, IList<KeyValuePair<string, string>> data)
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

        private async Task GoogleRefresh(User user)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", Configuration["Authentication:Google:ClientId"]),
                new KeyValuePair<string, string>("client_secret", Configuration["Authentication:Google:ClientSecret"]),
                new KeyValuePair<string, string>("refresh_token", user.GoogleToken.refresh_token),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            };

            var response = await Post("https://accounts.google.com/o/oauth2/token", data);

            var googleToken = JsonConvert.DeserializeObject<GoogleToken>(response);

            using (var db = new DataContext())
            {
                var currentUser = db.Users.First(x => x.Id == user.Id);
                currentUser.GoogleToken.access_token = googleToken.access_token;
                currentUser.GoogleToken.expires_in = googleToken.expires_in;
                currentUser.GoogleToken.token_type = googleToken.token_type;
                await db.SaveChangesAsync();
            }
        }
    }
}