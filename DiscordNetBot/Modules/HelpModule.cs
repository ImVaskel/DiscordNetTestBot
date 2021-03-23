using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordNetBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public HelpModule(
            DiscordSocketClient client,
            CommandService commands)
        {
            _client = client;
            _commands = commands;
        }

        [Command("help")]
        [Description("Sends the help command.")]
        public async Task HelpCommandAsync()
        {
            var modules = _commands.Modules;
            
            EmbedBuilder builder = new EmbedBuilder();

            foreach (var module in modules)
            {
                builder.AddField(name: module.Name, value: module.Commands.Select(cmd => "`{cmd}` ").ToString());
            }

            await ReplyAsync(embed: builder.Build());
        }
    }
}