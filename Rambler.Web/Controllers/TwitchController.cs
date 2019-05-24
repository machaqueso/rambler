using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public TwitchController(TwitchService twitchService, UserService userService, TwitchManager twitchManager, ConfigurationService configurationService)
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
            var oauthRequest = $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUrl}&response_type=code&scope=chat:read+user_read";

            return Redirect(oauthRequest);
        }

        public async Task<IActionResult> Callback(string code)
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", configurationService.GetValue("Authentication:Twitch:ClientId").Result),
                new KeyValuePair<string, string>("client_secret", configurationService.GetValue("Authentication:Twitch:ClientSecret").Result),
                new KeyValuePair<string, string>("redirect_uri", Url.Action("Callback", "Twitch", null, Request.Scheme, null).ToLower()),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

            var response = await twitchService.Post("https://id.twitch.tv/oauth2/token", data);
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

            await userService.AddToken(user.Id, ApiSource.Twitch, token);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> User()
        {
            var user = await twitchManager.GetUser();

            return View(user);
        }

        public IActionResult Author()
        {
            return View();
        }

    }
}