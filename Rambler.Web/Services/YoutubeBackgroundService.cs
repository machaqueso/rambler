using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Rambler.Web.Services
{
    public class YoutubeBackgroundService : BackgroundService
    {
        private readonly ILogger<YoutubeBackgroundService> logger;
        private readonly YoutubeService youtubeService;

        public YoutubeBackgroundService(ILogger<YoutubeBackgroundService> logger, YoutubeService youtubeService)
        {
            this.logger = logger;
            this.youtubeService = youtubeService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug($"YoutubeBackgroundService is starting.");

            stoppingToken.Register(() =>
                logger.LogDebug($" GracePeriod background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug($"GracePeriod task doing background work.");

                // get messages from youtube and publish them
                // need user id

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            logger.LogDebug($"GracePeriod background task is stopping.");
        }

    }
}