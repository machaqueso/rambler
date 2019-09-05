using Microsoft.AspNetCore.Mvc;

namespace Rambler.Web.Controllers
{
    public class AuthorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Filter()
        {
            return View();
        }
    }
}