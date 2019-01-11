using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Rambler.Web.Models;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [Route("apiStatus")]
        public IActionResult GetApiStatus()
        {
            var apiStatuses = new List<ApiStatus>
            {
                new ApiStatus(ApiSource.Youtube),
                new ApiStatus(ApiSource.Twitch)
            };

            return Ok(apiStatuses);
        }
    }
}