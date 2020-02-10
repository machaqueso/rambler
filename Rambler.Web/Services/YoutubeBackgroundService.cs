using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Data;
using Rambler.Models;

// Details on how to complete this at:
// https://blogs.msdn.microsoft.com/cesardelatorre/2017/11/18/implementing-background-tasks-in-microservices-with-ihostedservice-and-the-backgroundservice-class-net-core-2-x/

namespace Rambler.Web.Services
{
    public class YoutubeBackgroundService : BackgroundService
    {
        private readonly ILogger<YoutubeBackgroundService> logger;
        private readonly IntegrationManager integrationManager;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private YoutubeService youtubeService;

        private const int minimumPollingInterval = 10000;
        private const int defaultDelay = 60000;

        private string liveChatId;
        private int pollingInterval;
        private int delay;
        private string nextPageToken;

        public YoutubeBackgroundService(ILogger<YoutubeBackgroundService> logger, IServiceScopeFactory serviceScopeFactory, IntegrationManager integrationManager)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.integrationManager = integrationManager;
        }

        private async Task UpdateDashboardStatus(string name, string status, CancellationToken cancellationToken)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();
                await dashboardService.UpdateStatus(name, status, cancellationToken);
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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            pollingInterval = minimumPollingInterval;
            delay = defaultDelay;
            nextPageToken = string.Empty;

            cancellationToken.Register(() =>
            {
                logger.LogDebug($"[YoutubeBackgroundService] background task is stopping.");
            });
            logger.LogDebug($"[YoutubeBackgroundService] background task is starting.");

            using (var scope = serviceScopeFactory.CreateScope())
            {
                youtubeService = scope.ServiceProvider.GetRequiredService<YoutubeService>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug($"[YoutubeBackgroundService] entering primary loop.");

                    var integrationTokenSource = new CancellationTokenSource();
                    integrationManager.IntegrationChanged += (s, e) =>
                    {
                        if (e.Name == ApiSource.Youtube && !e.IsEnabled)
                        {
                            integrationTokenSource.Cancel();
                        }
                    };
                    var integrationToken = integrationTokenSource.Token;

                    try
                    {

                        if (!youtubeService.IsEnabled().Result)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube service disabled, skipping.");
                            logger.LogInformation($"Cancellation requested: {integrationToken.IsCancellationRequested}");
                            await UpdateDashboardStatus(ApiSource.Youtube, BackgroundServiceStatus.Disabled,
                                integrationToken);
                            await Task.Delay(1000, integrationToken);
                            continue;
                        }

                        if (!youtubeService.IsConfigured())
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube service not configured, skipping.");
                            await UpdateDashboardStatus(ApiSource.Youtube,
                                BackgroundServiceStatus.NotConfigured, integrationToken);
                            await Task.Delay(1000, integrationToken);
                            continue;
                        }

                        var token = await youtubeService.GetToken();
                        if (token == null)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube token missing, skipping.");
                            await UpdateDashboardStatus(ApiSource.Youtube, BackgroundServiceStatus.Forbidden,
                                integrationToken);
                            await Task.Delay(delay, integrationToken);
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
                            await UpdateDashboardStatus(ApiSource.Youtube, BackgroundServiceStatus.Forbidden,
                                integrationToken);
                            await Task.Delay(delay, integrationToken);
                            continue;
                        }

                        var liveBroadcast = await youtubeService.GetLiveBroadcast();
                        if (liveBroadcast == null)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] liveBroadcast not found.");
                            await UpdateDashboardStatus(ApiSource.Youtube, BackgroundServiceStatus.Error,
                                integrationToken);
                            await Task.Delay(delay, integrationToken);
                            continue;
                        }

                        // TODO: make sure it only returns one liveBroadcast
                        liveChatId = liveBroadcast.snippet.liveChatId;
                        logger.LogDebug($"[YoutubeBackgroundService] liveChatId: {liveChatId}");
                        await UpdateDashboardStatus(ApiSource.Youtube, liveBroadcast.status?.lifeCycleStatus,
                            integrationToken);
                        if (liveBroadcast.status?.lifeCycleStatus != "live")
                        {
                            pollingInterval = delay;
                        }

                        var liveBroadcastCheckDate = DateTime.UtcNow.AddMinutes(5);

                        logger.LogDebug($"[YoutubeBackgroundService] entering secondary loop.");
                        while (!integrationToken.IsCancellationRequested
                               && youtubeService.IsEnabled().Result
                               && token.Status == AccessTokenStatus.Ok)
                        {
                            logger.LogDebug($"[YoutubeBackgroundService] Secondary loop.");

                            var liveChatMessages = await youtubeService.GetLiveChatMessages(liveChatId, nextPageToken);
                            if (liveChatMessages == null || !liveChatMessages.items.Any())
                            {
                                logger.LogDebug($"[YoutubeBackgroundService] No messages found.");
                                await Task.Delay(TimeSpan.FromMilliseconds(pollingInterval), integrationToken);
                                continue;
                            }

                            logger.LogDebug(
                                $"[YoutubeBackgroundService] Received {liveChatMessages.items.Count()} messages");

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
                                pollingInterval = Math.Max(liveChatMessages.pollingIntervalMillis,
                                    minimumPollingInterval);
                            }

                            if (DateTime.UtcNow > liveBroadcastCheckDate)
                            {
                                liveBroadcastCheckDate = DateTime.UtcNow.AddMinutes(5);
                                liveBroadcast = await youtubeService.GetLiveBroadcast();
                                if (liveBroadcast != null)
                                {
                                    await UpdateDashboardStatus(ApiSource.Youtube,
                                        liveBroadcast.status?.lifeCycleStatus, integrationToken);
                                    if (liveBroadcast.status.lifeCycleStatus != "live")
                                    {
                                        pollingInterval = delay;
                                    }
                                }
                            }

                            logger.LogDebug($"[YoutubeBackgroundService] waiting {delay}ms.");
                            await Task.Delay(pollingInterval, integrationToken);
                        }

                        await UpdateDashboardStatus(ApiSource.Youtube, BackgroundServiceStatus.Stopped,
                            integrationToken);
                    }
                    catch (TaskCanceledException tce)
                    {
                        logger.LogInformation(tce.GetBaseException().Message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                        await UpdateDashboardStatus(ApiSource.Youtube, BackgroundServiceStatus.Error, integrationToken);
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            }

        }
    }
}