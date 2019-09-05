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
    public class AuthorFilterController : ControllerBase
    {
        private readonly AuthorService authorService;

        public AuthorFilterController(AuthorService authorService)
        {
            this.authorService = authorService;
        }


        [HttpGet]
        [Route("")]
        public IActionResult GetAuthorFilters()
        {
            var filters = authorService.GetFilters()
                .Include(x => x.Author)
                .OrderBy(x => x.FilterType)
                .ThenBy(x => x.Author.Name);

            return Ok(filters.Select(x => new
            {
                x.Id,
                x.FilterType,
                x.Date,
                x.AuthorId,
                x.Author.Name
            }));
        }

        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> DeleteAuthorFilter(AuthorFilter filter)
        {
            if (!await authorService.GetFilters().AnyAsync(x => x.Id == filter.Id))
            {
                return NotFound();
            }

            await authorService.DeleteFilter(filter.Id);
            return NoContent();
        }
    }
}