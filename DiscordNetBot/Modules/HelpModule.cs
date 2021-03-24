using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetBot.Extensions;

namespace DiscordNetBot.Modules
{ 
    [Name("Help")]
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
        [Summary("Sends the help command.")]
        public async Task HelpCommandAsync()
        {
            var modules = _commands.Modules.Where(mod => mod.Name != "Help");
            
            EmbedBuilder builder = new EmbedBuilder()
                .WithInvisible();

            foreach (var module in modules)
            {
                builder.AddField(name: module.Name, value: String.Join("", 
                    module.Commands.Select(cmd => $"`{cmd.Name}` ").ToList()));
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("help")]
        [Summary("Sends the help command for the given command. Will send multiple if there are multiple matches.")]
        public async Task HelpCommandAsync(String command)
        {
            List<CommandInfo> commands = _commands.Search(command).Commands.Select(c => c.Command).ToList();

            foreach (var cmd in commands)
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"Help for `{cmd.Name}`")
                    .WithInvisible();

                builder.AddField("Description", cmd.Summary ?? "None");

                builder.AddField("**Aliases**", cmd.Aliases.Count > 0 ?
                    String.Join("", cmd.Aliases.Select(c => $"`{c}` ")) :
                    "`None`");

                builder.AddField("**Parameters**", cmd.Parameters.Count > 0
                    ? String.Join("",
                        cmd.Parameters.Select(p => $"`{p.Name}` ").ToList())
                    : "`None`");

                await ReplyAsync(embed: builder.Build());
            }
        }
    }
}