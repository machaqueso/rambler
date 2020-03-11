using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;
using System.Linq;
using System.Threading.Tasks;

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
        [HttpGet]
        public IActionResult GetIntegrations()
        {
            return Ok(integrationService.GetIntegrations()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x=>x.Name));
        }

        [Route("{id}", Name="GetIntegration")]
        [HttpGet]
        public async Task<IActionResult> GetIntegration(int id)
        {
            var integration = await integrationService.GetIntegrations()
                .SingleOrDefaultAsync(x => x.Id == id);

            return Ok(integration);
        }

        [HttpPost]
        [Route("active")]
        public async Task<IActionResult> ActivateIntegrations()
        {
            var integrations = integrationService.GetIntegrations().OrderBy(x => x.Name);
            foreach (var integration in integrations)
            {
                if (integration.IsEnabled && integration.IsInactive)
                {
                    await integrationService.Activator(integration);
                }
            }

            return Ok(integrations);
        }

        [Route("{id}/enabled")]
        [HttpPut]
        public async Task<IActionResult> EnableIntegration(int id, [FromBody] Integration integration)
        {
            if (integration.IsEnabled)
            {
                switch (integration.Name)
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

            await integrationService.Activator(integration);
            return NoContent();
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateIntegration([FromBody] Integration integration)
        {
            await integrationService.Create(integration);
            return CreatedAtRoute("GetIntegration", new { id = integration.Id }, integration);
        }
        
        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateIntegration(int id, [FromBody] Integration integration)
        {
            await integrationService.Update(integration);

            return NoContent();
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteIntegration(int id)
        {
            await integrationService.Delete(id);
            return NoContent();
        }

    }
}