using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetIntegrations()
        {
            return Ok(integrationService.GetIntegrations().OrderBy(x => x.Name));
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

        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateIntegration(int id, [FromBody] Integration integration)
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

    }
}