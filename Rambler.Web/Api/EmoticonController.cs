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
    public class EmoticonController : ControllerBase
    {
        private readonly EmoticonService EmoticonService;

        public EmoticonController(EmoticonService EmoticonService)
        {
            this.EmoticonService = EmoticonService;
        }

        [Route("")]
        [AllowAnonymous]
        public IActionResult GetEmoticons(int maxItems = 10)
        {
            return Ok(EmoticonService.GetEmoticons()
                .OrderBy(x => x.Regex)
                .Take(maxItems));
        }

        [Route("{id}", Name = "GetEmoticon")]
        public IActionResult GetEmoticon(int id)
        {
            var emoticon = EmoticonService.GetEmoticons()
                .FirstOrDefault(x => x.Id == id);

            if (emoticon == null)
            {
                return NotFound();
            }

            return Ok(emoticon);
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateEmoticon(Emoticon emoticon)
        {
            if (emoticon == null)
            {
                return BadRequest();
            }

            if (EmoticonService.GetEmoticons().Any(x => x.Regex == emoticon.Regex))
            {
                return UnprocessableEntity("emoticon already added");
            }

            await EmoticonService.CreateEmoticon(emoticon);
            return CreatedAtRoute("GetEmoticon", new { id = emoticon.Id }, emoticon);
        }

        [Route("")]
        [HttpPut]
        public async Task<IActionResult> UpdateEmoticon(int id, Emoticon emoticon)
        {
            if (emoticon == null)
            {
                return BadRequest();
            }

            await EmoticonService.UpdateEmoticon(id, emoticon);
            return NoContent();
        }

        [Route("")]
        [HttpDelete]
        public async Task<IActionResult> DeleteEmoticon(Emoticon emoticon)
        {
            if (emoticon == null)
            {
                return BadRequest();
            }

            await EmoticonService.DeleteEmoticon(emoticon.Id);
            return NoContent();
        }
    }
}