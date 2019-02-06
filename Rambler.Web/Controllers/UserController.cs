using Microsoft.AspNetCore.Mvc;

namespace Rambler.Web.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Token()
        {
            return View();
        }
    }
}