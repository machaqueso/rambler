using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        [Route("apiStatus")]
        public IActionResult GetApiStatus()
        {
            return Ok(dashboardService.GetApiStatuses());
        }
    }
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
        public async Task<IActionResult> GetSettings()
        {
            return Ok(await configurationService.GetSettings());
        }
    }
}