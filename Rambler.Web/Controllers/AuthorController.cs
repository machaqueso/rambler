using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rambler.Services;

namespace Rambler.Web.Controllers
{
    public class AuthorController : Controller
    {
        private readonly AuthorService authorService;

        public AuthorController(AuthorService authorService)
        {
            this.authorService = authorService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Filter()
        {
            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var author = await authorService.GetAuthors()
                .Include(x => x.ChatMessages)
                .Include(x => x.AuthorFilters)
                .Include(x => x.AuthorScoreHistories)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }
    }
}