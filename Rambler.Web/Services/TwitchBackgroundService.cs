using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rambler.Models;
using Rambler.Services;

namespace Rambler.Web.Services
{
    public class TwitchBackgroundService : BackgroundService
    {
        private readonly ILogger<TwitchBackgroundService> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IntegrationManager integrationManager;

        private DashboardService dashboardService;
        private TwitchService twitchService;
        private TwitchManager twitchManager;
        private ChatService chatService;

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
                twitchService = scope.ServiceProvider.GetRequiredService<TwitchService>();
                dashboardService = scope.ServiceProvider.GetRequiredService<DashboardService>();
                twitchManager = scope.ServiceProvider.GetRequiredService<TwitchManager>();
                chatService = scope.ServiceProvider.GetRequiredService<ChatService>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!await twitchService.IsEnabled())
                        {
                            await dashboardService.UpdateStatus(ApiSource.Twitch, BackgroundServiceStatus.Disabled,
                                cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        if (!twitchService.IsConfigured())
                        {
                            await dashboardService.UpdateStatus(ApiSource.Twitch, BackgroundServiceStatus.NotConfigured,
                                cancellationToken);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }

                        await CheckToken(cancellationToken);
                        var user = await twitchManager.GetUser();

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
                                await TwitchHandshake(webSocket, cancellationToken);
                                await Send(webSocket, cancellationToken, $"JOIN :#{user.name}");
                                await dashboardService.UpdateStatus(ApiSource.Twitch, BackgroundServiceStatus.Connected, cancellationToken);
                            }
                        }

                        while (!cancellationToken.IsCancellationRequested && await twitchService.IsEnabled())
                        {
                            await Receive(webSocket, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.GetBaseException(), ex.GetBaseException().Message);
                        await dashboardService.UpdateStatus(ApiSource.Twitch, "Error", cancellationToken);
                    }

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        private async Task CheckToken(CancellationToken cancellationToken)
        {
            var token = await twitchService.GetToken();
            if (token == null)
            {
                await dashboardService.UpdateStatus(ApiSource.Twitch, BackgroundServiceStatus.Forbidden, cancellationToken);
                return;
            }

            if (token.Status == AccessTokenStatus.Expired && token.HasRefreshToken)
            {
                await twitchService.RefreshToken(token);
            }

            if (token.Status != AccessTokenStatus.Ok)
            {
                await dashboardService.UpdateStatus(ApiSource.Twitch, BackgroundServiceStatus.Forbidden, cancellationToken);
                return;
            }
        }

        private async Task TwitchHandshake(ClientWebSocket webSocket, CancellationToken cancellationToken)
        {
            await CheckToken(cancellationToken);
            var token = await twitchService.GetToken();
            var user = await twitchManager.GetUser();

            await Send(webSocket, cancellationToken, $"PASS oauth:{token.access_token}");
            await Send(webSocket, cancellationToken, $"NICK {user.name}"); // TODO: get nick from twitch API
        }

        private async Task Send(ClientWebSocket webSocket, CancellationToken cancellationToken, string message)
        {
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

        private async Task<WebSocketReceiveResult> ReceiverTask(ClientWebSocket webSocket, CancellationToken cancellationToken, byte[] buffer)
        {
            //var loopToken = new CancellationTokenSource();
            var timeoutToken = new CancellationTokenSource(5000).Token;
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);

            // TODO: Potential memory leak?
            integrationManager.IntegrationChanged += (s, e) =>
            {
                if (e.Name == ApiSource.Twitch && !e.IsEnabled)
                {
                    linkedToken.Cancel();
                }
            };

            try
            {
                return await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), linkedToken.Token);
            }
            catch (TaskCanceledException)
            {
                // YUM!
            }

            return new WebSocketReceiveResult(0, WebSocketMessageType.Text, true);
        }

        private async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(
            WebSocket socket, CancellationToken cancelToken)
        {
            WebSocketReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[receiveChunkSize];
            do
            {
                response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            } while (!response.EndOfMessage);

            return (response, message);
        }

        private async Task SenderTask(CancellationToken cancellationToken)
        {

        }

        private async Task Receive(ClientWebSocket webSocket, CancellationToken cancellationToken)
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
                var result = await ReceiverTask(webSocket, cancellationToken, buffer);

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
                            await Send(webSocket, cancellationToken, $"PONG :{host}");
                            await dashboardService.UpdateStatus(ApiSource.Twitch, BackgroundServiceStatus.Connected,
                                cancellationToken);
                            continue;
                        }

                        await twitchService.ProcessMessage(line);
                    }

                    partial = text.Substring(text.LastIndexOf('\r'));
                }

                foreach (var item in chatService.GetQueuedMessages("Twitch").Include(x => x.Message).ToList())
                {
                    if (!string.IsNullOrWhiteSpace(item.Message?.Message))
                    {
                        await Send(webSocket, cancellationToken, $"PRIVMSG #{user.name} :{item.Message.Message}");
                    }
                    await chatService.DequeueMessage(item.Id);
                }
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}