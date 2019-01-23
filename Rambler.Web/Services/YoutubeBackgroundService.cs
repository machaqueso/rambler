using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Web.Data;
using Rambler.Models;

// Details on how to complete this at:
// https://blogs.msdn.microsoft.com/cesardelatorre/2017/11/18/implementing-background-tasks-in-microservices-with-ihostedservice-and-the-backgroundservice-class-net-core-2-x/

namespace Rambler.Web.Services
{
    public class YoutubeBackgroundService : BackgroundService
    {
        private readonly ILogger<YoutubeBackgroundService> logger;
        private YoutubeService youtubeService;
        private ChatService chatService;
        private DashboardService dashboardService;
        private readonly IServiceScopeFactory serviceScopeFactory;

        private const int minimumPollingInterval = 10000;
        private const int defaultDelay = 1000;

        private string liveChatId;
        private int pollingInterval;
        private int delay;

        public YoutubeBackgroundService(ILogger<YoutubeBackgroundService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            pollingInterval = minimumPollingInterval;
            delay = defaultDelay;

            cancellationToken.Register(() =>
            {
                logger.LogDebug($"YoutubeBackgroundService background task is stopping.");
            });

            using (var scope = serviceScopeFactory.CreateScope())
            {
                youtubeService = scope.ServiceProvider.GetRequiredService<YoutubeService>();
                chatService = scope.ServiceProvider.GetRequiredService<ChatService>();
                dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!youtubeService.IsEnabled().Result)
                        {
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Disabled, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        if (!youtubeService.IsConfigured())
                        {
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.NotConfigured, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        var token = await youtubeService.GetToken();
                        if (token == null)
                        {
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Forbidden, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        if (token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
                        {
                            await youtubeService.RefreshToken(token);
                        }

                        if (!youtubeService.IsValidToken(token))
                        {
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Forbidden, cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        while (!cancellationToken.IsCancellationRequested && youtubeService.IsEnabled().Result)
                        {
                            if (string.IsNullOrEmpty(liveChatId))
                            {
                                var liveBroadcast = await youtubeService.GetLiveBroadcast();
                                if (liveBroadcast == null)
                                {
                                    await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Offline, cancellationToken);
                                    await Task.Delay(delay, cancellationToken);
                                    continue;
                                }

                                liveChatId = liveBroadcast.snippet.liveChatId;
                            }

                            var liveChatMessages = await youtubeService.GetLiveChatMessages(liveChatId);
                            await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Connected, cancellationToken);
                            if (liveChatMessages == null || !liveChatMessages.items.Any())
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(pollingInterval), cancellationToken);
                                continue;
                            }

                            foreach (var item in liveChatMessages.items)
                            {
                                await chatService.CreateMessage(youtubeService.MapToChatMessage(item));
                            }

                            pollingInterval = Math.Max(liveChatMessages.pollingIntervalMillis, minimumPollingInterval);
                            await Task.Delay(pollingInterval, cancellationToken);
                        }

                        await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Stopped, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                        await dashboardService.UpdateStatus(ApiSource.Youtube, BackgroundServiceStatus.Error, cancellationToken);
                    }
                    await Task.Delay(delay, cancellationToken);
                }
            }

        }
    }
}