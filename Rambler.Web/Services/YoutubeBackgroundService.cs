using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Web.Models;

// Details on how to complete this at:
// https://blogs.msdn.microsoft.com/cesardelatorre/2017/11/18/implementing-background-tasks-in-microservices-with-ihostedservice-and-the-backgroundservice-class-net-core-2-x/

namespace Rambler.Web.Services
{
    public class YoutubeBackgroundService : BackgroundService
    {
        private readonly ILogger<YoutubeBackgroundService> logger;
        private readonly YoutubeService youtubeService;
        private readonly ChatService chatService;
        private readonly DashboardService dashboardService;

        private string liveChatId;
        private const int DefaultDelay = 20000;
        private int delay;

        public YoutubeBackgroundService(ILogger<YoutubeBackgroundService> logger, YoutubeService youtubeService,
            ChatService chatService, DashboardService dashboardService)
        {
            this.logger = logger;
            this.youtubeService = youtubeService;
            this.chatService = chatService;
            this.dashboardService = dashboardService;
        }

        private async Task Delay(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delay), token);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            delay = DefaultDelay;
            logger.LogDebug($"YoutubeBackgroundService is starting.");
            await dashboardService.UpdateStatus(ApiSource.Youtube, "Starting", cancellationToken);

            cancellationToken.Register(async () =>
            {
                await dashboardService.UpdateStatus(ApiSource.Youtube, "Stopping", cancellationToken);
                logger.LogDebug($" YoutubeBackgroundService background task is stopping.");
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!youtubeService.IsConfigured())
                    {
                        await dashboardService.UpdateStatus(ApiSource.Youtube, "Not Configured", cancellationToken);
                        logger.LogError($"Youtube is not Configured");
                        await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                        continue;
                    }

                    var token = await youtubeService.GetToken();
                    if (token == null)
                    {
                        throw new UnauthorizedAccessException("Youtube token not found");
                    }

                    if (token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
                    {
                        await youtubeService.RefreshToken(token);
                    }

                    if (!youtubeService.IsValidToken(token))
                    {
                        await dashboardService.UpdateStatus(ApiSource.Youtube, "Needs Authentication", cancellationToken);
                        await Delay(cancellationToken);
                        continue;
                    }

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await dashboardService.UpdateStatus(ApiSource.Youtube, "Running", cancellationToken);

                        try
                        {
                            // get messages from youtube and publish them
                            if (string.IsNullOrEmpty(liveChatId))
                            {
                                logger.LogDebug($"liveChatId is null");
                                var liveBroadcast = await youtubeService.GetLiveBroadcast();
                                if (liveBroadcast == null)
                                {
                                    logger.LogDebug("No live broadcasts found.");
                                    await dashboardService.UpdateStatus(ApiSource.Youtube, "Live stream offline", cancellationToken);
                                    await Delay(cancellationToken);
                                    continue;
                                }

                                liveChatId = liveBroadcast.snippet.liveChatId;
                            }

                            logger.LogDebug($"Getting liveChatMessages for {liveChatId}");
                            var liveChatMessages = await youtubeService.GetLiveChatMessages(liveChatId);
                            if (liveChatMessages == null || !liveChatMessages.items.Any())
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
                                continue;
                            }

                            foreach (var item in liveChatMessages.items)
                            {
                                await chatService.CreateMessage(youtubeService.MapToChatMessage(item));
                            }

                            logger.LogDebug($"Posted {liveChatMessages.items.Count()}.");
                            delay = Math.Max(liveChatMessages.pollingIntervalMillis, DefaultDelay);
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug(ex.GetBaseException().ToString());
                            logger.LogDebug($"{ex.GetBaseException().Message}");
                            await dashboardService.UpdateStatus(ApiSource.Youtube, "Error", cancellationToken);
                        }

                        logger.LogDebug($"Next poll in {delay}ms.");
                        await dashboardService.UpdateStatus(ApiSource.Youtube, "Waiting", cancellationToken);
                        await Delay(cancellationToken);
                    }

                    await dashboardService.UpdateStatus(ApiSource.Youtube, "Stopped", cancellationToken);
                    logger.LogDebug($"YoutubeBackgroundService background task is stopping.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                    await dashboardService.UpdateStatus(ApiSource.Youtube, "Error", cancellationToken);
                }
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }

        }
    }
}