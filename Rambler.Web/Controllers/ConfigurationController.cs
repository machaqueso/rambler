using Microsoft.AspNetCore.Mvc;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ConfigurationService configurationService;

        public ConfigurationController(ConfigurationService configurationService)
        {
            this.configurationService = configurationService;
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
            Program.Shutdown();
            return RedirectToAction("Index", "Home");
        }

    }
}