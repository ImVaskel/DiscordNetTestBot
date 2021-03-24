using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordNetBot.Extensions;

namespace DiscordNetBot.Modules
{
    [Name("Info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {

        private async Task WhoisSendInfo(SocketGuildUser user)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithInvisible()
                .WithTitle($"Info about {user}")
                .WithDescription($"Name: {user} [{user.Id}] \n" +
                                 $"Bot: {user.IsBotHumanize()} \n" +
                                 $"Nickname: {user.Nickname} \n" +
                                 $"Created At: {user.CreatedAt} \n" +
                                 $"Joined At: {user.JoinedAt} \n" +
                                 $"Mention: {user.Mention} \n" +
                                 $"Premium Since: {user.PremiumSince}")
                .WithThumbnailUrl(user.GetAvatarUrl());

            await ReplyAsync(embed: builder.Build());
        }
        
        [Command("whois")]
        [Alias("userinfo")]
        [Summary("Sends info about the author.")]
        [RequireContext(ContextType.Guild)]
        public async Task WhoisAsync()
        {
            await WhoisSendInfo(Context.Guild.GetUser(Context.User.Id));
        }
        
        [Command("whois")]
        [Alias("userinfo")]
        [Summary("Sends info about the given user.")]
        [RequireContext(ContextType.Guild)]
        public async Task WhoisAsync(SocketGuildUser user)
        {
            await WhoisSendInfo(user);
        }
    }
}