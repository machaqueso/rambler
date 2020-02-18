using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rambler.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Rambler.Web.Services
{
    public class YoutubeBackgroundService : CustomBackgroundService
    {
        private readonly ILogger<YoutubeBackgroundService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private YoutubeService youtubeService;

        private const int minimumPollingInterval = 15000;
        private const int defaultDelay = 60000;

        private string liveChatId;
        private int pollingInterval;
        private int delay;
        private string nextPageToken;

        public YoutubeBackgroundService(ILogger<YoutubeBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        private async Task UpdateDashboardStatus(string status, CancellationToken cancellationToken)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();
                await dashboardService.UpdateStatus(ApiSource.Youtube, status, cancellationToken);
            }
        }

        private async Task ProcessMessage(ChatMessage message)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var chatProcessor = scope.ServiceProvider.GetRequiredService<ChatProcessor>();
                await chatProcessor.ProcessMessage(message);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await UpdateDashboardStatus(BackgroundServiceStatus.Stopped, cancellationToken);
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            pollingInterval = minimumPollingInterval;
            delay = defaultDelay;
            nextPageToken = string.Empty;

            cancellationToken.Register(() =>
            {
                logger.LogDebug($"[YoutubeBackgroundService] background task is stopping.");
            });

            logger.LogDebug($"[YoutubeBackgroundService] starting.");
            await UpdateDashboardStatus(BackgroundServiceStatus.Starting, cancellationToken);

            using (var scope = serviceScopeFactory.CreateScope())
            {
                youtubeService = scope.ServiceProvider.GetRequiredService<YoutubeService>();

                if (!youtubeService.IsEnabled().Result)
                {
                    logger.LogWarning($"[YoutubeBackgroundService] Youtube disabled.");
                    await UpdateDashboardStatus(BackgroundServiceStatus.Disabled, cancellationToken);
                    return;
                }

                if (!youtubeService.IsConfigured())
                {
                    logger.LogWarning($"[YoutubeBackgroundService] Youtube not configured.");
                    await UpdateDashboardStatus(BackgroundServiceStatus.NotConfigured, cancellationToken);
                    return;
                }

                while (!cancellationToken.IsCancellationRequested)
                {

                    try
                    {

                        var token = await youtubeService.GetToken();
                        if (token == null)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube token missing, skipping.");
                            await UpdateDashboardStatus(BackgroundServiceStatus.Forbidden,
                                cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        if (token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube token expired, refreshing");
                            await youtubeService.RefreshToken(token);
                        }

                        if (!youtubeService.IsValidToken(token))
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube token invalid.");
                            await UpdateDashboardStatus(BackgroundServiceStatus.Forbidden,
                                cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        var liveBroadcast = await youtubeService.GetLiveBroadcast();
                        if (liveBroadcast == null)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] liveBroadcast not found.");
                            await UpdateDashboardStatus(BackgroundServiceStatus.Error,
                                cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        // TODO: make sure it only returns one liveBroadcast
                        liveChatId = liveBroadcast.snippet.liveChatId;
                        logger.LogDebug($"[YoutubeBackgroundService] liveChatId: {liveChatId}");
                        await UpdateDashboardStatus(liveBroadcast.status?.lifeCycleStatus,
                            cancellationToken);
                        if (liveBroadcast.status?.lifeCycleStatus != "live")
                        {
                            pollingInterval = delay;
                        }

                        var liveBroadcastCheckDate = DateTime.UtcNow.AddMinutes(5);

                        logger.LogDebug($"[YoutubeBackgroundService] entering secondary loop.");
                        while (!cancellationToken.IsCancellationRequested
                               && youtubeService.IsEnabled().Result
                               && token.Status == AccessTokenStatus.Ok)
                        {

                            logger.LogDebug($"[YoutubeBackgroundService] Secondary loop.");

                            var liveChatMessages = await youtubeService.GetLiveChatMessages(liveChatId, nextPageToken);
                            if (liveChatMessages == null || !liveChatMessages.items.Any())
                            {
                                logger.LogDebug($"[YoutubeBackgroundService] No messages found.");
                                await Task.Delay(TimeSpan.FromMilliseconds(pollingInterval), cancellationToken);
                                continue;
                            }

                            logger.LogDebug($"[YoutubeBackgroundService] Received {liveChatMessages.items.Count()} messages");

                            foreach (var item in liveChatMessages.items)
                            {
                                await ProcessMessage(youtubeService.MapToChatMessage(item));
                            }

                            nextPageToken = liveChatMessages.nextPageToken;

                            // TODO: Switch this to Min when going production
                            logger.LogDebug(
                                $"[YoutubeBackgroundService] New polling interval: {liveChatMessages.pollingIntervalMillis}");
                            logger.LogDebug($"[YoutubeBackgroundService] Next page: {liveChatMessages.nextPageToken}");

                            if (liveBroadcast.status?.lifeCycleStatus == "live")
                            {
                                pollingInterval = Math.Max(liveChatMessages.pollingIntervalMillis, minimumPollingInterval);
                            }

                            if (DateTime.UtcNow > liveBroadcastCheckDate)
                            {
                                liveBroadcastCheckDate = DateTime.UtcNow.AddMinutes(5);
                                liveBroadcast = await youtubeService.GetLiveBroadcast();
                                if (liveBroadcast != null)
                                {
                                    await UpdateDashboardStatus(liveBroadcast.status?.lifeCycleStatus, cancellationToken);
                                    if (liveBroadcast.status.lifeCycleStatus != "live")
                                    {
                                        pollingInterval = delay;
                                    }
                                }
                            }

                            logger.LogDebug($"[YoutubeBackgroundService] waiting {pollingInterval}ms.");
                            await Task.Delay(pollingInterval, cancellationToken);
                        }

                        await UpdateDashboardStatus(BackgroundServiceStatus.Stopped,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                        await UpdateDashboardStatus(BackgroundServiceStatus.Error, cancellationToken);
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }

            logger.LogDebug($"[YoutubeBackgroundService] ExecuteAsync exit");

        }
    }
}