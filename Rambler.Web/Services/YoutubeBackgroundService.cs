using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Details on how to complete this at:
// https://blogs.msdn.microsoft.com/cesardelatorre/2017/11/18/implementing-background-tasks-in-microservices-with-ihostedservice-and-the-backgroundservice-class-net-core-2-x/

namespace Rambler.Web.Services
{
    public class YoutubeBackgroundService : BackgroundService
    {
        private readonly ILogger<YoutubeBackgroundService> logger;
        private readonly YoutubeService youtubeService;
        private readonly ChatService chatService;

        private string liveChatId;
        private const int DefaultDelay = 20000;

        public YoutubeBackgroundService(ILogger<YoutubeBackgroundService> logger, YoutubeService youtubeService, ChatService chatService)
        {
            this.logger = logger;
            this.youtubeService = youtubeService;
            this.chatService = chatService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delay = DefaultDelay;

            logger.LogDebug($"YoutubeBackgroundService is starting.");

            stoppingToken.Register(() => logger.LogDebug($" YoutubeBackgroundService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug($"YoutubeBackgroundService task doing background work.");

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
                            await Task.Delay(TimeSpan.FromMilliseconds(delay), stoppingToken);
                            continue;
                        }

                        liveChatId = liveBroadcast.snippet.liveChatId;
                    }

                    logger.LogDebug($"Getting liveChatMessages for {liveChatId}");
                    var liveChatMessages = await youtubeService.GetLiveChatMessages(liveChatId);
                    if (liveChatMessages == null || !liveChatMessages.items.Any())
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(delay), stoppingToken);
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
                }

                logger.LogDebug($"Next poll in {delay}ms.");
                await Task.Delay(TimeSpan.FromMilliseconds(delay), stoppingToken);
            }

            logger.LogDebug($"YoutubeBackgroundService background task is stopping.");
        }

    }
}
