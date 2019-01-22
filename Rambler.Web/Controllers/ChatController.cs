using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Models;

namespace Rambler.Web.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Popup(string fontSize = "inherit", string backgroundColor = "none", string color = "white")
        {
            var chatConfig = new ChatConfig
            {
                FontSize = fontSize,
                BackgroundColor = backgroundColor,
                Color = color
            };

            return View(chatConfig);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Chatbox(string fontSize = "inherit", string backgroundColor = "none", string color = "white")
        {
            var chatConfig = new ChatConfig
            {
                FontSize = fontSize,
                BackgroundColor = backgroundColor,
                Color = color
            };

            return View(chatConfig);
        }

        public IActionResult Reader()
        {
            return View();
        }

    }
}