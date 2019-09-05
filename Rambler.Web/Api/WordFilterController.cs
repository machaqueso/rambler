using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class WordFilterController : ControllerBase
    {
        private readonly WordFilterService wordFilterService;

        public WordFilterController(WordFilterService wordFilterService)
        {
            this.wordFilterService = wordFilterService;
        }

        [Route("")]
        [AllowAnonymous]
        public IActionResult GetWordFilters(int maxItems = 10)
        {
            return Ok(wordFilterService.GetWordFilters()
                .OrderBy(x => x.Word));
        }

        [Route("{id}", Name = "GetWordFilter")]
        public IActionResult GetWordFilter(int id)
        {
            var word = wordFilterService.GetWordFilters()
                .FirstOrDefault(x => x.Id == id);

            if (word == null)
            {
                return NotFound();
            }

            return Ok(word);
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateWordFilter(WordFilter word)
        {
            if (word == null)
            {
                return BadRequest();
            }

            if (wordFilterService.GetWordFilters().Any(x => x.Word == word.Word))
            {
                return UnprocessableEntity("Word already added");
            }

            await wordFilterService.CreateWordFilter(word);
            return CreatedAtRoute("GetWordFilter", new { id = word.Id }, word);
        }

        [Route("")]
        [HttpPut]
        public async Task<IActionResult> UpdateWordFilter(int id, WordFilter word)
        {
            if (word == null)
            {
                return BadRequest();
            }

            await wordFilterService.UpdateWordFilter(id, word);
            return NoContent();
        }

        [Route("")]
        [HttpDelete]
        public async Task<IActionResult> DeleteWordFilter(WordFilter word)
        {
            if (word == null)
            {
                return BadRequest();
            }

            await wordFilterService.DeleteWordFilter(word.Id);
            return NoContent();
        }
    }
}