using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DiscordNetBot.Interfaces
{
    public interface IBotService : IHostedService, IAsyncDisposable
    {
        public DiscordSocketClient DiscordClient { get; }
        public void ExecuteHandlerAsynchronously<TReturn>(Func<DiscordSocketClient, Task<TReturn>> handler, Action<TReturn> callback);
    }
}