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

    }
}