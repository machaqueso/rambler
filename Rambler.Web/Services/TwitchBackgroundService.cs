using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Web.Models;

namespace Rambler.Web.Services
{
    public class TwitchBackgroundService : BackgroundService
    {
        private readonly ILogger<TwitchBackgroundService> logger;
        private readonly ChatService chatService;
        private readonly DashboardService dashboardService;
        private readonly TwitchService twitchService;

        private const int sendChunkSize = 256;
        private const int receiveChunkSize = 256;
        private const int connectionTimeout = 60;

        public TwitchBackgroundService(ChatService chatService, ILogger<TwitchBackgroundService> logger,
            DashboardService dashboardService, TwitchService twitchService)
        {
            this.chatService = chatService;
            this.logger = logger;
            this.dashboardService = dashboardService;
            this.twitchService = twitchService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogDebug($"TwitchBackgroundService is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await dashboardService.UpdateStatus(ApiSource.Twitch, "Starting", stoppingToken);

                    stoppingToken.Register(() =>
                        logger.LogDebug($" TwitchBackgroundService background task is stopping."));
                    var webSocket = new ClientWebSocket();

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (webSocket.State != WebSocketState.Open)
                        {
                            logger.LogDebug($"Opening web socket");
                            await dashboardService.UpdateStatus(ApiSource.Twitch, "Connecting", stoppingToken);
                            await webSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), stoppingToken);

                            var timeout = 0;
                            while (webSocket.State == WebSocketState.Connecting && timeout < connectionTimeout &&
                                   !stoppingToken.IsCancellationRequested)
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                                timeout++;
                            }

                            logger.LogDebug($"Authenticating");
                            await dashboardService.UpdateStatus(ApiSource.Twitch, "Authenticating", stoppingToken);
                            // authenticate with twitch using oauth
                            await TwitchHandshake(webSocket, stoppingToken);
                        }
                        await dashboardService.UpdateStatus(ApiSource.Twitch, "Connected", stoppingToken);
                        await Receive(webSocket, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                    await dashboardService.UpdateStatus(ApiSource.Twitch, "Error", cancellationToken: stoppingToken);
                    throw;
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task TwitchHandshake(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            var token = await twitchService.GetToken();
            if (token == null)
            {
                throw new UnauthorizedAccessException("Twitch token not found");
            }

            await Send(webSocket, cancellationToken, $"PASS oauth:{token.access_token}");
            await Send(webSocket, cancellationToken, $"NICK machacoder");
        }

        private async Task Send(ClientWebSocket webSocket, CancellationToken cancellationToken, string message)
        {
            var encoder = new UTF8Encoding();
            var buffer = encoder.GetBytes(message);
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Twitch socket Socket closed");
            }
        }

        private async Task Receive(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            var encoder = new UTF8Encoding();
            byte[] buffer = new byte[receiveChunkSize];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    await chatService.CreateMessage(new ChatMessage
                    {
                        Source = ApiSource.Twitch,
                        Date = DateTime.UtcNow,
                        Message = encoder.GetString(buffer)
                    });
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }
    }
}