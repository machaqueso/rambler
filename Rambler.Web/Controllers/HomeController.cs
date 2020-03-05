﻿using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly YoutubeService youtubeService;
        private readonly TwitchService twitchService;

        public HomeController(YoutubeService youtubeService, TwitchService twitchService)
        {
            this.youtubeService = youtubeService;
            this.twitchService = twitchService;
        }

        public async Task<IActionResult> Index()
        {
            if ((youtubeService.IsEnabled().Result && !youtubeService.IsConfigured())
                || (twitchService.IsEnabled().Result && !twitchService.IsConfigured()))
            {
                return RedirectToAction("Index", "Configuration");
            }

            await youtubeService.PurgeData();

            return RedirectToAction("Dashboard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}