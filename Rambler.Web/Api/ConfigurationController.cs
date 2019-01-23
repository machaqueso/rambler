using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationService configurationService;

        public ConfigurationController(ConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        [Route("")]
        public IActionResult GetSettings()
        {
            return Ok(configurationService.GetSettings());
        }

        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateSetting(int id, [FromBody] ConfigurationSetting setting)
        {
            await configurationService.UpdateSetting(id, setting);
            return NoContent();
        }

    }
}