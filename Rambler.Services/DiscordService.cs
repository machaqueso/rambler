using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Rambler.Models;

namespace Rambler.Services
{
    public class DiscordService
    {
        private readonly ILogger<DiscordService> logger;
        private readonly ConfigurationService configurationService;
        private readonly IntegrationService integrationService;


        public DiscordService(ILogger<DiscordService> logger, ConfigurationService configurationService, IntegrationService integrationService)
        {
            this.logger = logger;
            this.configurationService = configurationService;
            this.integrationService = integrationService;
        }

        public bool IsConfigured()
        {
            return configurationService.HasValue(ConfigurationSettingNames.DiscordToken);
        }

        public async Task<bool> IsEnabled()
        {
            return await integrationService.IsEnabled(ApiSource.Discord);
        }

        public async Task<string> GetToken()
        {
            if (!IsConfigured())
            {
                throw new InvalidOperationException($"Discord bot token is not configured");
            }

            return await configurationService.GetValue(ConfigurationSettingNames.DiscordToken);
        }

        private bool clientReady;

        public async Task<IReadOnlyCollection<SocketGuild>> GetGuilds()
        {
            var token = await GetToken();

            using var client = new DiscordSocketClient();
            client.Log += AddLog;

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            clientReady = false;
            client.Ready += OnClientReady;
            // HACK: wait for client.Ready event
            while (!clientReady)
            {
                await Task.Delay(1000);
            }

            return client.Guilds;
        }

        private Task OnClientReady()
        {
            clientReady = true;
            return Task.CompletedTask;
        }

        private Task AddLog(LogMessage log)
        {
            logger.LogInformation(log.Exception, log.Message);
            return Task.CompletedTask;
        }

        public async Task<UserStatus> GetStatus()
        {
            var token = await GetToken();

            using var client = new DiscordSocketClient();
            await client.LoginAsync(TokenType.Bot, token);
            return client.Status;
        }

        public ChatMessage ProcessMessage(SocketMessage socketMessage)
        {
            if (socketMessage == null)
            {
                throw new ArgumentNullException(nameof(socketMessage));
            }

            var chatMessage = new ChatMessage
            {
                Message = socketMessage.Content,
                Date = DateTime.UtcNow,
                Source = ApiSource.Discord,
                Author = new Author
                {
                    Name = socketMessage.Author.Username,
                    Source = ApiSource.Discord,
                    SourceAuthorId = socketMessage.Author.Id.ToString()
                },
                Type = "textMessageEvent"
            };

            return chatMessage;
        }

        public async Task SetActiveTextChannel(ulong id)
        {
            if (configurationService.HasValue(ConfigurationSettingNames.DiscordChannelId))
            {
                await configurationService.SetValue(ConfigurationSettingNames.DiscordChannelId, id.ToString());
                return;
            }

            await configurationService.CreateSetting(new ConfigurationSetting
            {
                Key = ConfigurationSettingNames.DiscordChannelId,
                Value = id.ToString()
            });
        }
    }
}