using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Models;
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

        public IActionResult Index()
        {
            if ((youtubeService.IsEnabled().Result && !youtubeService.IsConfigured())
                || (twitchService.IsEnabled().Result && !twitchService.IsConfigured()))
            {
                return RedirectToAction("Index", "Configuration");
            }

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