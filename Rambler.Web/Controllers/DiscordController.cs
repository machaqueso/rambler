using Microsoft.AspNetCore.Mvc;

namespace Rambler.Web.Controllers
{
    public class DiscordController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Channel(ulong id)
        {
            return View(id);
        }

    }
}