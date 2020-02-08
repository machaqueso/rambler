using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rambler.Models;
using Rambler.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class AuthorController : ControllerBase
    {
        private readonly AuthorService authorService;

        public AuthorController(AuthorService authorService)
        {
            this.authorService = authorService;
        }

        [Route("")]
        public IActionResult GetAuthors()
        {
            var authors = authorService.GetAuthors()
                .Include(x => x.AuthorFilters)
                .Where(x => x.Source != null)
                .OrderBy(x => x.Name);

            return Ok(authors.Select(x => new
            {
                x.Id,
                x.Name,
                x.Source,
                x.SourceAuthorId,
                x.Points,
                x.Score,
                Lists = string.Join(',', x.AuthorFilters.Select(l => l.FilterType))
            }).ToList());
        }

        [Route("action")]
        public IActionResult GetActions()
        {
            return Ok(ActionTypes.All);
        }

        [HttpPut]
        [Route("{id}/{command}")]
        public async Task<IActionResult> AuthorAction(int id, string command)
        {
            await authorService.AuthorAction(id, command);
            return Ok();
        }
    }
}