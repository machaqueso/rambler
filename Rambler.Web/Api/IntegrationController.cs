using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rambler.Models;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class IntegrationController : ControllerBase
    {
        private readonly IntegrationService integrationService;
        private readonly YoutubeService youtubeService;
        private readonly TwitchService twitchService;

        public IntegrationController(IntegrationService integrationService, YoutubeService youtubeService, TwitchService twitchService)
        {
            this.integrationService = integrationService;
            this.youtubeService = youtubeService;
            this.twitchService = twitchService;
        }

        [Route("")]
        [AllowAnonymous]
        public IActionResult GetIntegrations()
        {
            return Ok(integrationService.GetIntegrations().OrderBy(x => x.Name));
        }

        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateIntegration(int id, [FromBody] Integration integration)
        {
            var entity = await integrationService.GetIntegrations().SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            if (entity.IsEnabled)
            {
                switch (entity.Name)
                {
                    case "Youtube":
                        if (!youtubeService.IsConfigured())
                        {
                            return UnprocessableEntity($"Please configure Youtube integration first.");
                        }

                        break;
                    case "Twitch":
                        if (!twitchService.IsConfigured())
                        {
                            return UnprocessableEntity($"Please configure Twitch integration first.");
                        }

                        break;
                }
            }

            await integrationService.UpdateIntegration(id, integration);
            return NoContent();
        }

        [Route("{id}")]
        public async Task<IActionResult> GetIntegration(int id)
        {
            var entity = await integrationService.GetIntegrations().SingleOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

    }
}