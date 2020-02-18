using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Models;
using Rambler.Services;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rambler.Web.Services
{
    public class TwitchBackgroundService : CustomBackgroundService
    {
        private readonly ILogger<TwitchBackgroundService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IntegrationManager integrationManager;

        private const int sendChunkSize = 510;
        private const int receiveChunkSize = 510; // RFC-2812
        private const int connectionTimeout = 60;
        private const int defaultDelay = 1000;

        private int delay;
        private ConcurrentQueue<string> messageQueue;

        public TwitchBackgroundService(ILogger<TwitchBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory, IntegrationManager integrationManager)
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
                await dashboardService.UpdateStatus(ApiSource.Twitch, status, cancellationToken);
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"[TwitchBackgroundService] start.");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug($"[TwitchBackgroundService] stop.");
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() =>
            {
                logger.LogDebug($"[TwitchBackgroundService] background task is stopping.");
            });

            delay = defaultDelay;

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var twitchService = scope.ServiceProvider.GetRequiredService<TwitchService>();
                var twitchManager = scope.ServiceProvider.GetRequiredService<TwitchManager>();

                logger.LogDebug($"[TwitchBackgroundService] starting.");
                await UpdateDashboardStatus(BackgroundServiceStatus.Starting, cancellationToken);

                if (!await twitchService.IsEnabled())
                {
                    logger.LogWarning($"[TwitchBackgroundService] Twitch disabled.");
                    await UpdateDashboardStatus(BackgroundServiceStatus.Disabled, cancellationToken);
                    return;
                }

                if (!twitchService.IsConfigured())
                {
                    logger.LogWarning($"[TwitchBackgroundService] Twitch not configured");
                    await UpdateDashboardStatus(BackgroundServiceStatus.NotConfigured, cancellationToken);
                    return;
                }

                try
                {

                    await CheckToken(cancellationToken, twitchService);
                    var user = await twitchManager.GetUser();

                    integrationManager.MessageSent += (s, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e.Message))
                        {
                            Send($"PRIVMSG #{user.name} :{e.Message}");
                        }
                    };

                    var webSocket = new ClientWebSocket();
                    if (webSocket.State != WebSocketState.Open)
                    {
                        await webSocket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), cancellationToken);

                        var timeout = 0;
                        while (webSocket.State == WebSocketState.Connecting && timeout < connectionTimeout &&
                               !cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                            timeout++;
                        }

                        if (webSocket.State == WebSocketState.Open)
                        {
                            await TwitchHandshake(webSocket, cancellationToken, twitchService, twitchManager);
                            Send($"JOIN :#{user.name}");
                            await UpdateDashboardStatus(BackgroundServiceStatus.Connected,
                                cancellationToken);
                        }
                    }

                    var receiveTask = Receive(webSocket, cancellationToken, twitchService, twitchManager);
                    var sendTask = SenderTask(webSocket, cancellationToken);
                    await Task.WhenAll(receiveTask, sendTask);

                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Ok", cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    await UpdateDashboardStatus(BackgroundServiceStatus.Stopping, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                    await UpdateDashboardStatus("Error", cancellationToken);
                }

                logger.LogDebug($"[TwitchBackgroundService] pal carajo");
            }
        }

        private async Task CheckToken(CancellationToken cancellationToken, TwitchService twitchService)
        {
            var token = await twitchService.GetToken();
            if (token == null)
            {
                await UpdateDashboardStatus(BackgroundServiceStatus.Forbidden, cancellationToken);
                return;
            }

            if (token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
            {
                await twitchService.RefreshToken(token);
            }

            if (token.Status != AccessTokenStatus.Ok)
            {
                await UpdateDashboardStatus(BackgroundServiceStatus.Forbidden, cancellationToken);
                return;
            }
        }

        private async Task TwitchHandshake(ClientWebSocket webSocket, CancellationToken cancellationToken,
            TwitchService twitchService, TwitchManager twitchManager)
        {
            await CheckToken(cancellationToken, twitchService);
            var token = await twitchService.GetToken();
            var user = await twitchManager.GetUser();

            Send($"PASS oauth:{token.access_token}");
            Send($"NICK {user.name}"); // TODO: get nick from twitch API
        }

        private void Send(string message)
        {
            messageQueue.Enqueue(message);
        }

        private async Task SenderTask(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!messageQueue.TryDequeue(out var message))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    continue;
                }

                if (webSocket.State != WebSocketState.Open)
                {
                    throw new InvalidOperationException($"Twitch socket {webSocket.State.ToString()}");
                }

                logger.LogDebug($"[TwitchBackgroundService] Twitch Chat > {message}");
                var encoder = new UTF8Encoding();
                var buffer = encoder.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    cancellationToken);
            }
        }

        private async Task Receive(ClientWebSocket webSocket, CancellationToken cancellationToken, TwitchService twitchService,
            TwitchManager twitchManager)
        {
            if (webSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException($"[TwitchBackgroundService] Twitch socket {webSocket.State.ToString()}");
            }
            var user = await twitchManager.GetUser();

            var encoder = new UTF8Encoding();
            var partial = string.Empty;

            while (webSocket.State == WebSocketState.Open
                   && !cancellationToken.IsCancellationRequested
                   && await twitchService.IsEnabled())
            {
                var buffer = new byte[receiveChunkSize];
                logger.LogDebug($"[TwitchBackgroundService] Listening to socket");
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                logger.LogDebug($"[TwitchBackgroundService] Receive status {result.MessageType}");

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var text = partial + encoder.GetString(buffer).Replace("\0", "");
                    partial = string.Empty;

                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var lines = (text.Substring(0, text.LastIndexOf('\r'))).Split('\r');

                    foreach (var line in lines)
                    {
                        logger.LogDebug($"[TwitchBackgroundService] Twitch Chat < {line}");

                        if (line.Contains("PING"))
                        {
                            var host = line.Substring(line.IndexOf(':'));
                            Send($"PONG :{host}");
                            await UpdateDashboardStatus(BackgroundServiceStatus.Connected,
                                cancellationToken);
                            continue;
                        }

                        await twitchService.ProcessMessage(line);
                    }

                    partial = text.Substring(text.LastIndexOf('\r'));
                }

                await Task.Delay(delay, cancellationToken);
            }
        }

    }
}
