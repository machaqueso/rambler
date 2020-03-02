using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ConfigurationService configurationService;
        private readonly IHostApplicationLifetime applicationLifetime;

        public ConfigurationController(ConfigurationService configurationService, IHostApplicationLifetime applicationLifetime)
        {
            this.configurationService = configurationService;
            this.applicationLifetime = applicationLifetime;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Youtube()
        {
            return View();
        }

        public IActionResult Twitch()
        {
            return View();
        }

        public IActionResult WordFilter()
        {
            return View();
        }

        public IActionResult Bot()
        {
            return View();
        }

        public IActionResult Shutdown()
        {
            //Program.Shutdown();
            applicationLifetime.StopApplication();
            return RedirectToAction("Index", "Home");
        }

    }
}