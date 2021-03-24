using System;
using System.Drawing;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordNetBot.Extensions
{
    public static class DiscordNetExtensions
    {
        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Debug,
                _ => LogLevel.Debug
            };
        }

        public static EmbedBuilder WithInvisible(this EmbedBuilder builder)
        {
            return builder.WithColor(47, 49, 54);
        }

        public static String IsBotHumanize(this SocketGuildUser user)
        {
            return user.IsBot ? "Yes" : "No";
        }
    }
}