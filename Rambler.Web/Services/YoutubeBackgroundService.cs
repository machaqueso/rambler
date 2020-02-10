﻿using System;
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
        private YoutubeService youtubeService;
        private ChatProcessor chatService;
        private DashboardService dashboardService;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private const int minimumPollingInterval = 10000;
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
                chatService = scope.ServiceProvider.GetRequiredService<ChatProcessor>();
                dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug($"[YoutubeBackgroundService] entering primary loop.");
                    try
                    {
                        if (!youtubeService.IsEnabled().Result)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube service disabled, skipping.");
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Disabled, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        if (!youtubeService.IsConfigured())
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube service not configured, skipping.");
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.NotConfigured, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        var token = await youtubeService.GetToken();
                        if (token == null)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] Youtube token missing, skipping.");
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Forbidden, cancellationToken);
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
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Forbidden, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        var liveBroadcast = await youtubeService.GetLiveBroadcast();
                        if (liveBroadcast == null)
                        {
                            logger.LogWarning($"[YoutubeBackgroundService] liveBroadcast not found.");
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Error, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        // TODO: make sure it only returns one liveBroadcast
                        liveChatId = liveBroadcast.snippet.liveChatId;
                        logger.LogDebug($"[YoutubeBackgroundService] liveChatId: {liveChatId}");
                        await dashboardService.UpdateStatus(ApiSource.Youtube, liveBroadcast.status?.lifeCycleStatus, cancellationToken);
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
                                await chatService.ProcessMessage(youtubeService.MapToChatMessage(item));
                            }
                            nextPageToken = liveChatMessages.nextPageToken;

                            // TODO: Switch this to Min when going production
                            logger.LogDebug($"[YoutubeBackgroundService] New polling interval: {liveChatMessages.pollingIntervalMillis}");
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
                                    await dashboardService.UpdateStatus(ApiSource.Youtube, liveBroadcast.status?.lifeCycleStatus, cancellationToken);
                                    if (liveBroadcast.status.lifeCycleStatus != "live")
                                    {
                                        pollingInterval = delay;
                                    }
                                }
                            }

                            await Task.Delay(pollingInterval, cancellationToken);
                        }

                        await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Stopped, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                        await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Error, cancellationToken);
                    }
                    logger.LogDebug($"[YoutubeBackgroundService] waiting {delay}ms.");
                    await Task.Delay(delay, cancellationToken);
                }
            }

        }
    }
}