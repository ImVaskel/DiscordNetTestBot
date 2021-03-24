using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetBot.Extensions;
using DiscordNetBot.Interfaces;
using DiscordNetBot.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordNetBot.Services
{
    public class CommandHostedService : IHostedService
    {
        private readonly ILogger<CommandHostedService> _logger;
        private readonly DiscordSettings _config;
        private readonly IServiceProvider _provider;
        private readonly IBotService _botService;
        private readonly CommandService _commandService;
        
        public IServiceProvider Services { get; }

        public CommandHostedService(
            ILogger<CommandHostedService> logger,
            IServiceProvider provider,
            IBotService botService,
            CommandService commandService,
            DiscordSettings config,
            IServiceProvider services
        )
        {
            _logger = logger;
            _provider = provider;
            _botService = botService;
            _commandService = commandService;
            _config = config;
            Services = services;
            
        }
        
        private Task LogCommand(LogMessage arg)
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

        private async Task BotMessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            if (!(message is SocketUserMessage userMessage))
            {
                return; 
            }
            var argPos = 0;

            if (!userMessage.HasStringPrefix(_config.Prefix, ref argPos))
            {
                return;
            }
            
            _botService.ExecuteHandlerAsynchronously(
                handler: (client) =>
                {
                    var context = new SocketCommandContext(client, userMessage);
                    return _commandService.ExecuteAsync(context, argPos, _provider);
                },
                callback: async (result) =>
                {
                    if (result.IsSuccess)
                    {
                        return;
                    }
                    
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        return;
                    }

                    if (result.Error.HasValue)
                    {
                        var embed = new EmbedBuilder()
                            .WithDescription($"{result.Error.Value}; {result.ErrorReason}")
                            .Build();
                        await message.Channel.SendMessageAsync(embed: embed);
                    }
                });
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _commandService.Log += LogCommand;
            _botService.DiscordClient.MessageReceived += BotMessageReceivedAsync;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _botService.DiscordClient.MessageReceived -= BotMessageReceivedAsync;
            _botService.DiscordClient.Log -= LogCommand;

            return Task.CompletedTask;
        }
    }
}