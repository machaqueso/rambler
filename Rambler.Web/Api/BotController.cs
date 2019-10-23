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
    public class BotController : ControllerBase
    {
        private readonly BotService botService;

        public BotController(BotService botService)
        {
            this.botService = botService;
        }

        [Route("action")]
        [AllowAnonymous]
        public IActionResult GetBotActions(int maxItems = 10)
        {
            return Ok(botService.GetBotActions());
        }

        [Route("action/{id}", Name = "GetBotAction")]
        public IActionResult GetBotAction(int id)
        {
            var word = botService.GetBotActions()
                .FirstOrDefault(x => x.Id == id);

            if (word == null)
            {
                return NotFound();
            }

            return Ok(word);
        }

        [Route("action")]
        [HttpPost]
        public async Task<IActionResult> CreateBotAction(BotAction botAction)
        {
            if (botAction == null)
            {
                return BadRequest();
            }

            if (botService.GetBotActions().Any(x => x.Command == botAction.Command
                                                    & x.Parameters == botAction.Parameters))
            {
                return UnprocessableEntity("Bot action already added");
            }

            await botService.CreateBotAction(botAction);
            return CreatedAtRoute("GetBotAction", new { id = botAction.Id }, botAction);
        }

        [Route("action")]
        [HttpPut]
        public async Task<IActionResult> UpdateBotAction(int id, BotAction botAction)
        {
            if (botAction == null)
            {
                return BadRequest();
            }

            await botService.UpdateBotAction(id, botAction);
            return NoContent();
        }

        [Route("action")]
        [HttpDelete]
        public async Task<IActionResult> DeleteBotAction(BotAction botAction)
        {
            if (botAction == null)
            {
                return BadRequest();
            }

            await botService.DeleteBotAction(botAction.Id);
            return NoContent();
        }
    }
}