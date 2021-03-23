using System;
using System.IO;
using System.Threading.Tasks;
using DiscordNetBot.Interfaces;
using DiscordNetBot.Services;
using DiscordNetBot.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DiscordNetBot
{
    class Program
    {
        static async Task Main(string[] args)
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
                .ConfigureServices((context, services) =>
                {
                    services.Configure<DiscordSettings>(context.Configuration.GetSection("discord"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<DiscordSettings>>().Value);
                    
                    services.AddSingleton<IBotService, BotHostedService>();
                    services.AddHostedService(provider => provider.GetRequiredService<IBotService>());
                    services.AddSingleton(provider =>
                        provider.GetRequiredService<CommandHandler>().InstallCommandsAsync());
                });
            
            using var builtHost = host.Build();
            await builtHost.StartAsync();
            await builtHost.WaitForShutdownAsync();
        }
    }
}
