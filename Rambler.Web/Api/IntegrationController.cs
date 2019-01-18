using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Models;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly IntegrationService integrationService;

        public IntegrationController(IntegrationService integrationService)
        {
            this.integrationService = integrationService;
        }

        [Route("")]
        public IActionResult GetIntegrations()
        {
            return Ok(integrationService.GetIntegrations());
        }

        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateIntegration(int id, [FromBody] Integration integration)
        {
            await integrationService.UpdateIntegration(id, integration);
            return NoContent();
        }


    }
}