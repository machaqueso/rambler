using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Rambler.Web.Hubs;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class YoutubeController : Controller
    {
        private readonly IHubContext<ChatHub> chatHubContext;
        private readonly YoutubeService youtubeService;
        private readonly UserService userService;
        private readonly ConfigurationService configurationService;

        public YoutubeController(IConfiguration configuration, IHubContext<ChatHub> chatHubContext,
            YoutubeService youtubeService, UserService userService, ConfigurationService configurationService)
        {
            this.chatHubContext = chatHubContext;
            this.youtubeService = youtubeService;
            this.userService = userService;
            this.configurationService = configurationService;
        }

        public async Task<IActionResult> Index()
        {
            var token = await youtubeService.GetToken();

            return View(token);
        }

        public async Task<IActionResult> Callback(string code)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", await configurationService.GetValue("Authentication:Google:ClientId")),
                new KeyValuePair<string, string>("client_secret", await configurationService.GetValue("Authentication:Google:ClientSecret")),
                new KeyValuePair<string, string>("redirect_uri",
                    Url.Action("Callback", "Youtube", null, Request.Scheme, null)),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

            var response = await youtubeService.Post("https://accounts.google.com/o/oauth2/token", data);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            var token = JsonConvert.DeserializeObject<AccessToken>(content);

            var user = await userService.GetUsers().FirstOrDefaultAsync();
            if (user == null)
            {
                user = new User();
                await userService.Create(user);
            }

            await userService.AddToken(user.Id, ApiSource.Youtube, token);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Authorize()
        {
            if (!youtubeService.IsConfigured())
            {
                return RedirectToAction("Youtube", "Configuration");
            }

            var clientId = await configurationService.GetValue("Authentication:Google:ClientId");
            var redirectUrl = WebUtility.UrlEncode(Url.Action("Callback", "Youtube", null, Request.Scheme, null));
            var oauthRequest =
                $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUrl}&scope=https://www.googleapis.com/auth/youtube.readonly&response_type=code&access_type=offline&prompt=consent";

            return Redirect(oauthRequest);
        }

        public async Task<IActionResult> Broadcast()
        {
            var token = await youtubeService.GetToken();
            if (token == null)
            {
                return await Authorize();
            }

            if (token.Status == AccessTokenStatus.Expired)
            {
                await youtubeService.RefreshToken(token);
            }

            var liveBroadcast = await youtubeService.GetLiveBroadcast();
            return View("Broadcast", liveBroadcast);
        }

        public async Task<IActionResult> Messages(string id)
        {
            var token = await youtubeService.GetToken();
            if (token == null)
            {
                return await Authorize();
            }

            if (token.Status == AccessTokenStatus.Expired)
            {
                await youtubeService.RefreshToken(token);
            }

            var liveChatMessageList = await youtubeService.GetLiveChatMessages(id);

            foreach (var item in liveChatMessageList.items.Where(x => x.snippet.hasDisplayContent))
            {
                await chatHubContext.Clients.All.SendAsync("ReceiveMessage", item.AuthorDetails.displayName,
                    item.snippet.displayMessage);
            }

            return View("Messages", liveChatMessageList);
        }
    }
}