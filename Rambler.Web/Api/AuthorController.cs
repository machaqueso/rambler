using Microsoft.AspNetCore.Mvc;
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
            return Ok(authorService.GetAuthors());
        }
    }
}