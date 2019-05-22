using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class TwitchController : Controller
    {
        private readonly TwitchService twitchService;
        private readonly UserService userService;
        private readonly TwitchManager twitchManager;
        private readonly ConfigurationService configurationService;

        public TwitchController(TwitchService twitchService, UserService userService, TwitchManager twitchManager,
            ConfigurationService configurationService)
        {
            this.twitchService = twitchService;
            this.userService = userService;
            this.twitchManager = twitchManager;
            this.configurationService = configurationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Authorize()
        {
            if (!twitchService.IsConfigured())
            {
                return RedirectToAction("Twitch", "Configuration");
            }

            var clientId = configurationService.GetValue("Authentication:Twitch:ClientId").Result;
            var redirectUrl = Url.Action("Callback", "Twitch", null, Request.Scheme, null).ToLower();
            var oauthRequest =
                $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUrl}&response_type=code&scope=chat:read+user_read";

            return Redirect(oauthRequest);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Callback(string code, string state)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id",
                    configurationService.GetValue("Authentication:Twitch:ClientId").Result),
                new KeyValuePair<string, string>("client_secret",
                    configurationService.GetValue("Authentication:Twitch:ClientSecret").Result),
                new KeyValuePair<string, string>("redirect_uri",
                    Url.Action("Callback", "Twitch", null, Request.Scheme, null).ToLower()),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

            var response = await twitchService.Post("https://id.twitch.tv/oauth2/token", data);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            var token = JsonConvert.DeserializeObject<AccessToken>(content);
            if (token == null)
            {
                throw new InvalidOperationException("Unable to deserialize authentication token");
            }

            var twitchUser = await twitchManager.GetUser(token);
            var username = twitchUser.name;

            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("Twitch channel title not found.");
            }

            var user = await userService.GetUsers().SingleOrDefaultAsync(x => x.ExternalAccounts.Any(y => y.ReferenceId == twitchUser._id.ToString()));
            if (user == null)
            {
                user = new User
                {
                    UserName = username
                };
                await userService.Create(user);
            }
            await userService.AddToken(user.Id, ApiSource.Twitch, token);
            await userService.AddExternalAccount(user.Id, ApiSource.Twitch, twitchUser._id.ToString(), twitchUser.name);

            if (state == HttpContext.Session.GetString("state"))
            {
                return RedirectToAction("tokenlogin", "Account", new { accessToken = token.access_token });
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> TwitchUser()
        {
            var token = await twitchService.GetToken();
            var user = await twitchManager.GetUser(token);

            return View(user);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (!twitchService.IsConfigured())
            {
                throw new ApplicationException("Twitch API is not configured");
            }

            var state = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("state", state);

            var clientId = configurationService.GetValue("Authentication:Twitch:ClientId").Result;
            var redirectUrl = Url.Action("Callback", "Twitch", null, Request.Scheme, null).ToLower();
            var oauthRequest = $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}" +
                               $"&redirect_uri={redirectUrl}" +
                               $"&response_type=code" +
                               $"&scope=chat:read+user_read" +
                               $"&state={state}";

            return Redirect(oauthRequest);
        }
    }
}