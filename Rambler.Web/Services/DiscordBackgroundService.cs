using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rambler.Models;
using Rambler.Services;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Rambler.Web.Services
{
    public class DiscordBackgroundService : CustomBackgroundService
    {
        private readonly ILogger<DiscordBackgroundService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IntegrationManager integrationManager;
        private DiscordSocketClient client;
        private ulong? channelId;

        private ConcurrentQueue<string> messageQueue;

        public DiscordBackgroundService(ILogger<DiscordBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IntegrationManager integrationManager)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.integrationManager = integrationManager;

            messageQueue = new ConcurrentQueue<string>();
        }

        private async Task UpdateDashboardStatus(string status, CancellationToken cancellationToken)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();
                await dashboardService.UpdateStatus(ApiSource.Discord, status, cancellationToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"[DiscordBackgroundService] starting.");
            await UpdateDashboardStatus(IntegrationStatus.Starting, cancellationToken);

            client = new DiscordSocketClient();
            client.Log += AddLog;

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var discordService = scope.ServiceProvider.GetRequiredService<DiscordService>();
                var configurationService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();

                client.MessageReceived += ProcessMessage;

                if (!await discordService.IsEnabled())
                {
                    logger.LogWarning($"[DiscordBackgroundService] Discord disabled.");
                    await UpdateDashboardStatus(IntegrationStatus.Disabled, cancellationToken);
                    return;
                }

                if (!discordService.IsConfigured())
                {
                    logger.LogWarning($"[DiscordBackgroundService] discord not configured");
                    await UpdateDashboardStatus(IntegrationStatus.NotConfigured, cancellationToken);
                    return;
                }

                try
                {
                    if (ulong.TryParse(await configurationService.GetValue(ConfigurationSettingNames.DiscordChannelId), out var discordChannelId))
                    {
                        channelId = discordChannelId;
                    }

                    await client.LoginAsync(TokenType.Bot, await discordService.GetToken());
                    await client.StartAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                    await UpdateDashboardStatus(IntegrationStatus.Error, cancellationToken);
                    return;
                }
            }

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"[DiscordBackgroundService] stopping.");
            await UpdateDashboardStatus(IntegrationStatus.Stopping, cancellationToken);
            await client.StopAsync();

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await UpdateDashboardStatus(IntegrationStatus.Connected, cancellationToken);

            integrationManager.MessageSent += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Message))
                {
                    Send(e.Message);
                }
            };

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException tex)
            {
                logger.LogInformation(tex.GetBaseException(), tex.GetBaseException().Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
            }
            finally
            {
                await UpdateDashboardStatus(IntegrationStatus.Stopped, cancellationToken);
            }

        }

        private void Send(string messageText)
        {
            var channel = client.GetChannel(686087842421669940) as ISocketMessageChannel;
            channel.SendMessageAsync(messageText);
        }

        private Task AddLog(LogMessage log)
        {
            logger.LogInformation(log.Exception, log.Message);
            return Task.CompletedTask;
        }

        private async Task ProcessMessage(SocketMessage socketMessage)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var discordService = scope.ServiceProvider.GetRequiredService<DiscordService>();
                var chatProcessor = scope.ServiceProvider.GetRequiredService<ChatProcessor>();

                // If channelId defined, ignores incoming messages from other channels
                if (channelId.HasValue && socketMessage.Channel.Id != channelId.Value)
                {
                    return;
                }

                var chatMessage = discordService.ProcessMessage(socketMessage);
                await chatProcessor.ProcessMessage(chatMessage);
            }
        }

    }
}
