using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordNetBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public async Task TestCommand()
        {
            await ReplyAsync("I am alive!");
        }
    }
}