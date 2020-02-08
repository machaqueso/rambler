using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Data;
using Rambler.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class TwitchController : ControllerBase
    {
        private readonly TwitchManager twitchManager;

        public TwitchController(TwitchManager twitchManager)
        {
            this.twitchManager = twitchManager;
        }

        [Route("author")]
        public IActionResult GetAuthors()
        {
            return Ok(twitchManager.GetAuthors());
        }
    }
}
