using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Models.Exceptions;
using Rambler.Services;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationService configurationService;

        public ConfigurationController(ConfigurationService configurationService)
        {
            this.configurationService = configurationService;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateSetting(ConfigurationSetting setting)
        {
            if (setting == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(setting.Key))
            {
                return UnprocessableEntity($"Setting key is required");
            }

            if (string.IsNullOrWhiteSpace(setting.Name))
            {
                setting.Name = setting.Key;
            }

            if (string.IsNullOrWhiteSpace(setting.Value))
            {
                return UnprocessableEntity($"Setting value is required");
            }

            try
            {
                await configurationService.CreateSetting(setting);
            }
            catch (UnprocessableEntityException uex)
            {
                return UnprocessableEntity(uex.Message);
            }
            catch (ConflictException cex)
            {
                return Conflict(cex.Message);
            }

            return CreatedAtAction("GetSetting", new { id = setting.Id }, setting);
        }

        [Route("")]
        [HttpGet]
        public IActionResult GetSettings()
        {
            return Ok(configurationService.GetSettings());
        }

        [Route("{id}", Name = "GetSetting")]
        [HttpGet]
        public IActionResult GetSetting(int id)
        {
            var setting = configurationService.GetSettings().SingleOrDefault(x => x.Id == id);
            if (setting == null)
            {
                return NotFound();
            }

            return Ok(setting);
        }

        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateSetting(int id, [FromBody] ConfigurationSetting setting)
        {
            await configurationService.UpdateSetting(id, setting);
            return NoContent();
        }

        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSetting(int id)
        {
            await configurationService.DeleteSetting(id);
            return NoContent();
        }

        [Route("names")]
        [HttpGet]
        public IActionResult GetSettingNames()
        {
            var names = configurationService.GetConfigurationSettingNames()
                .Where(x => !configurationService.GetSettings().Select(s => s.Key).Contains(x));

            return Ok(names);
        }
    }
}