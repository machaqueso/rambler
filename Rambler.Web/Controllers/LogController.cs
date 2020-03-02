using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Rambler.Data;

namespace Rambler.Web.Controllers
{
    public class LogController : Controller
    {
        private readonly DataContext db;

        public LogController(DataContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var logs = db.Logs.OrderByDescending(x => x.Id);

            return View(logs);
        }
    }
}