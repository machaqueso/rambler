using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rambler.Models;
using Rambler.Services;

namespace Rambler.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("api")]
    public class DiscordController : ControllerBase
    {
        private readonly DiscordService discordService;
        private readonly ConfigurationService configurationService;

        public DiscordController(DiscordService discordService, ConfigurationService configurationService)
        {
            this.discordService = discordService;
            this.configurationService = configurationService;
        }

        [Route("status")]
        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var status = await discordService.GetStatus();

            return Ok(status.ToString());
        }

        [Route("guild")]
        [HttpGet]
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
        [HttpGet]
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
        [HttpGet]
        public async Task<IActionResult> GetTextChannels(ulong id)
        {
            var guild = (await discordService.GetGuilds()).FirstOrDefault(x => x.Id == id);

            if (guild == null)
            {
                return NotFound();
            }

            ulong? channelId = null;
            if (ulong.TryParse(await configurationService.GetValue(ConfigurationSettingNames.DiscordChannelId), out var discordChannelId))
            {
                channelId = discordChannelId;
            }

            return Ok(guild.TextChannels
                .OrderBy(x => x.Position)
                .Select(x => new
                {
                    Id = x.Id.ToString(),
                    x.Name,
                    x.Position,
                    x.IsNsfw,
                    x.Mention,
                    x.Topic,
                    IsActive = (channelId.HasValue && channelId.Value == x.Id)
                }));
        }

        [Route("defaultChannel")]
        [HttpGet]
        public async Task<IActionResult> GetDefaultChannel()
        {
            var guilds = await discordService.GetGuilds();

            return Ok(guilds.Select(x => new
            {
                x.DefaultChannel.Id,
                x.DefaultChannel.Name
            }));
        }

        [Route("guild/{guildId}/textChannel/{id}/active")]
        [HttpPut]
        public async Task<IActionResult> SetActiveTextChannel(ulong guildId, ulong id)
        {
            var guild = (await discordService.GetGuilds()).FirstOrDefault(x => x.Id == guildId);

            if (guild == null)
            {
                return NotFound();
            }

            if (guild.TextChannels.All(x => x.Id != id))
            {
                NotFound();
            }

            await discordService.SetActiveTextChannel(id);

            return NoContent();
        }

    }

}