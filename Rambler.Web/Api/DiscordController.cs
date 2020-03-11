using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class DiscordController : ControllerBase
    {
        private readonly DiscordService discordService;

        public DiscordController(DiscordService discordService)
        {
            this.discordService = discordService;
        }

        [Route("status")]
        public async Task< IActionResult> GetStatus()
        {
            var status = await discordService.GetStatus();

            return Ok(status.ToString());
        }

        [Route("guild")]
        public async Task< IActionResult> GetGuilds()
        {
            var guilds = await discordService.GetGuilds();

            return Ok(guilds.Select(x=>new
            {
                x.Id,
                x.Name,
                x.MemberCount
            }));
        }

        [Route("channel")]
        public async Task< IActionResult> GetChannels()
        {
            var guilds = await discordService.GetGuilds();
            var channels = guilds.SelectMany(x => x.Channels);

            return Ok(channels.Select(x=>new
            {
                x.Id,
                x.Name
            }));
        }

        [Route("defaultChannel")]
        public async Task< IActionResult> GetDefaultChannel()
        {
            var guilds = await discordService.GetGuilds();

            return Ok(guilds.Select(x=>new
            {
                x.DefaultChannel.Id,
                x.DefaultChannel.Name
            }));
        }
    }
}