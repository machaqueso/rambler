using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        public async Task<IActionResult> GetStatus()
        {
            var status = await discordService.GetStatus();

            return Ok(status.ToString());
        }

        [Route("guild")]
        public async Task<IActionResult> GetGuilds()
        {
            var guilds = await discordService.GetGuilds();

            return Ok(guilds.Select(x => new
            {
                Id = x.Id.ToString(),
                x.Name,
                x.MemberCount
            }));
        }

        [Route("channel")]
        public async Task<IActionResult> GetChannels()
        {
            var guilds = await discordService.GetGuilds();
            var channels = guilds.SelectMany(x => x.Channels);

            return Ok(channels.Select(x => new
            {
                x.Id,
                x.Name
            }));
        }

        [Route("guild/{id}/textChannel")]
        public async Task<IActionResult> GetTextChannels(ulong id)
        {
            var guild = (await discordService.GetGuilds()).FirstOrDefault(x => x.Id == id);

            if (guild == null)
            {
                return NotFound();
            }

            return Ok(guild.TextChannels
                .OrderBy(x => x.Position)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Position,
                    x.IsNsfw,
                    x.Mention,
                    x.Topic
                }));
        }

        [Route("defaultChannel")]
        public async Task<IActionResult> GetDefaultChannel()
        {
            var guilds = await discordService.GetGuilds();

            return Ok(guilds.Select(x => new
            {
                x.DefaultChannel.Id,
                x.DefaultChannel.Name
            }));
        }
    }
}