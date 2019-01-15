using Microsoft.AspNetCore.Mvc;

namespace Rambler.Web.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Popup()
        {
            return View();
        }
    }
}