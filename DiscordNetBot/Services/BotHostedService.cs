using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordNetBot.Extensions;
using DiscordNetBot.Interfaces;
using DiscordNetBot.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordNetBot.Services
{
    public class BotHostedService : IBotService
    {
        private readonly ILogger<BotHostedService> _logger;
        private readonly DiscordSettings _config;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IHost _host;

        public DiscordSocketClient DiscordClient { get; private set; }
        
        public BotHostedService(
            DiscordSettings config,
            ILogger<BotHostedService> logger,
            IHostApplicationLifetime applicationLifetime,
            IHost host
        )
        {
            _config = config;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            DiscordClient = new DiscordSocketClient();
            _host = host;
        }

        public void ExecuteHandlerAsynchronously<TReturn>(Func<DiscordSocketClient, Task<TReturn>> handler, Action<TReturn> callback)
        {
            if(DiscordClient.ConnectionState != ConnectionState.Connected)
            {
                _logger.LogWarning("A handler attempted to execute a handler while the bot was disconnected");
                
                return;
            }

            // explicitly fire & forget the handler to prevent blocking the gateway;
            _ = handler(DiscordClient).ContinueWith(cb => callback(cb.Result));
        }
        
        private Task BotDisconnected(Exception arg)
        {
            
            if(arg is GatewayReconnectException)
            {
                return Task.CompletedTask;
            }

            _logger.LogCritical(arg, "Discord disconnected with a non-resumable error");
            _applicationLifetime.StopApplication();
            
            return Task.CompletedTask;
        }
        
        private Task BotLoggedIn()
        {
            _logger.LogInformation("Bot has logged into Discord");
            
            return Task.CompletedTask;
        }
        
        private Task BotLog(LogMessage arg)
        {
            var message = $"{arg.Source}: {arg.Message}";
            
            if (arg.Exception is null)
            {
                _logger.Log(arg.Severity.ToLogLevel(), message);
            }
            else
            {
                _logger.Log(arg.Severity.ToLogLevel(), arg.Exception, message);
            }

            return Task.CompletedTask;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DiscordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                ConnectionTimeout = 30000,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LogSeverity.Debug,
                UseSystemClock = true,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages
            });

            DiscordClient.LoggedIn += BotLoggedIn;
            DiscordClient.Disconnected += BotDisconnected;
            DiscordClient.Log += BotLog;

            var token = _config.Token;
            
            await DiscordClient.LoginAsync(TokenType.Bot, token);
            await DiscordClient.StartAsync();
        }
        
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (DiscordClient is null)
            {
                return;
            }

            await DiscordClient.SetStatusAsync(UserStatus.Offline);
            await DiscordClient.StopAsync();
        }
        
        public ValueTask DisposeAsync()
        {
            DiscordClient?.Dispose();

            return new ValueTask();
        }
    }
}