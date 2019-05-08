using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;

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

        [AllowAnonymous]
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

        [AllowAnonymous]
        public IActionResult Reader()
        {
            return View();
        }

    }
}