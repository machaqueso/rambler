using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Rambler.Web.Controllers
{
    public class YoutubeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}