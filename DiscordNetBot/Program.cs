using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordNetBot.Interfaces;
using DiscordNetBot.Services;
using DiscordNetBot.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordNetBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(builder =>
                {
                    builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.json", optional: false);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddDebug();
                    }

                    builder.AddSystemdConsole(o =>
                    {
                        o.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                    });
                })
                .ConfigureServices((context, services) =>
                {
                    // Add configuration
                    services.Configure<DiscordSettings>(context.Configuration.GetSection("discord"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<DiscordSettings>>().Value);
                    
                    // Add discord
                    services.AddSingleton<IBotService, BotHostedService>();
                    services.AddSingleton(provider =>
                    {
                        var commandService = new CommandService(new CommandServiceConfig
                        {
                            CaseSensitiveCommands = false,
                            DefaultRunMode = RunMode.Sync,
                            LogLevel = LogSeverity.Verbose
                        });

                        commandService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);

                        return commandService;
                    });
                    
                    // add hosted services;
                    services.AddHostedService(provider => provider.GetRequiredService<IBotService>());
                    services.AddHostedService<CommandHostedService>();
                });
            
            using var builtHost = host.Build();
            await builtHost.StartAsync();
            await builtHost.WaitForShutdownAsync();
        }
    }
}
