using Microsoft.AspNetCore.Mvc;

namespace Rambler.Web.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}