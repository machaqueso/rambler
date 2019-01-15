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
        private readonly DashboardService dashboardService;
        private readonly TwitchService twitchService;

        private const int sendChunkSize = 510;
        private const int receiveChunkSize = 510; // RFC-2812
        private const int connectionTimeout = 60;

        public TwitchBackgroundService(ILogger<TwitchBackgroundService> logger,
            DashboardService dashboardService, TwitchService twitchService)
        {
            this.logger = logger;
            this.dashboardService = dashboardService;
            this.twitchService = twitchService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"TwitchBackgroundService is starting.");
            await dashboardService.UpdateStatus(ApiSource.Twitch, "Starting", cancellationToken);
            cancellationToken.Register(async () =>
            {
                await dashboardService.UpdateStatus(ApiSource.Youtube, "Stopping", cancellationToken: cancellationToken);
                logger.LogDebug($" TwitchBackgroundService background task is stopping.");
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var webSocket = new ClientWebSocket();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        if (webSocket.State != WebSocketState.Open)
                        {
                            logger.LogDebug($"Opening web socket");
                            await dashboardService.UpdateStatus(ApiSource.Twitch, "Connecting", cancellationToken);
                            await webSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), cancellationToken);

                            var timeout = 0;
                            while (webSocket.State == WebSocketState.Connecting && timeout < connectionTimeout &&
                                   !cancellationToken.IsCancellationRequested)
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
                                timeout++;
                            }

                            logger.LogDebug($"Authenticating");
                            await dashboardService.UpdateStatus(ApiSource.Twitch, "Authenticating", cancellationToken);
                            // authenticate with twitch using oauth
                            await TwitchHandshake(webSocket, cancellationToken);
                        }

                        await dashboardService.UpdateStatus(ApiSource.Twitch, "Connected", cancellationToken);

                        await Send(webSocket, cancellationToken, "JOIN :#machacoder");

                        await Receive(webSocket, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                    await dashboardService.UpdateStatus(ApiSource.Twitch, "Error",
                        cancellationToken: cancellationToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private async Task TwitchHandshake(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            var token = await twitchService.GetToken();
            if (token == null)
            {
                throw new UnauthorizedAccessException("Twitch token not found");
            }

            if (token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
            {
                await twitchService.RefreshToken(token);
            }

            if (token.Status != AccessTokenStatus.Ok)
            {
                throw new UnauthorizedAccessException("Could not refresh authorization token");
            }

            await Send(webSocket, cancellationToken, $"PASS oauth:{token.access_token}");
            await Send(webSocket, cancellationToken, $"NICK machacoder");
        }

        private async Task Send(ClientWebSocket webSocket, CancellationToken cancellationToken, string message)
        {
            logger.LogDebug($"Twitch Chat > {message}");
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
            var partial = string.Empty;
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[receiveChunkSize];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var text = partial + encoder.GetString(buffer).Replace("\0", "");
                    partial = string.Empty;
                    var lines = (text.Substring(0, text.LastIndexOf('\r'))).Split('\r');

                    foreach (var line in lines)
                    {
                        logger.LogDebug($"Twitch Chat < {line}");

                        if (line.Contains("PING"))
                        {
                            var host = line.Substring(line.IndexOf(':'));
                            await Send(webSocket, cancellationToken, $"PONG :{host}");
                            await dashboardService.UpdateStatus(ApiSource.Twitch, "Connected", cancellationToken);
                            continue;
                        }

                        await twitchService.ProcessMessage(line);
                    }

                    partial = text.Substring(text.LastIndexOf('\r'));
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }
    }
}