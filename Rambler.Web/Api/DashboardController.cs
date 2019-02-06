using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
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
}