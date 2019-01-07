﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Rambler.Web.Data;
using Rambler.Web.Hubs;
using Rambler.Web.Models;
using Rambler.Web.Models.Youtube.LiveBroadcast;
using Rambler.Web.Models.Youtube.LiveChat;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class YoutubeController : Controller
    {
        public IConfiguration Configuration { get; }
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly YoutubeService youtubeService;

        public YoutubeController(IConfiguration configuration, IHubContext<ChatHub> chatHubContext,
            YoutubeService youtubeService)
        {
            Configuration = configuration;
            this.chatHubContext = chatHubContext;
            this.youtubeService = youtubeService;
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

            var response = await youtubeService.Post("https://accounts.google.com/o/oauth2/token", data);

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

            var response = await youtubeService.Get(
                "https://www.googleapis.com/youtube/v3/liveBroadcasts?part=snippet&broadcastType=persistent&mine=true",
                user.GoogleToken.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int) response.StatusCode, content);
            }

            var liveBroadcastList = JsonConvert.DeserializeObject<List>(content);
            return View("Broadcast", liveBroadcastList);
        }

        public async Task<IActionResult> Messages(string id)
        {
            User user;

            using (var db = new DataContext())
            {
                user = await db.Users.Include(x => x.GoogleToken)
                    .FirstAsync(x => x.Id == Convert.ToInt32(Request.Cookies["UserId"]));

                if (DateTime.Now > user.GoogleToken.ExpirationDate)
                {
                    await GoogleRefresh(user);
                }
            }

            var response = await youtubeService.Get(
                $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={id}&part=id,snippet,authorDetails",
                user.GoogleToken.access_token);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int) response.StatusCode, content);
            }

            var liveChatMessageList = JsonConvert.DeserializeObject<MessageList>(content);

            foreach (var item in liveChatMessageList.items.Where(x => x.snippet.hasDisplayContent))
            {
                await chatHubContext.Clients.All.SendAsync("ReceiveMessage", item.AuthorDetails.displayName,
                    item.snippet.displayMessage);
            }

            return View("Messages", liveChatMessageList);
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

            var response = await youtubeService.Post("https://accounts.google.com/o/oauth2/token", data);

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