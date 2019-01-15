using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Models;

namespace Rambler.Web.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Popup(string fontSize = "inherit", string backgroundColor = "none")
        {
            var chatConfig = new ChatConfig
            {
                FontSize = fontSize,
                BackgroundColor = backgroundColor
            };

            return View(chatConfig);
        }
    }
}