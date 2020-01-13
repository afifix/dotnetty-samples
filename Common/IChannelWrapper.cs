using System;
using System.Threading.Tasks;

using DotNetty.Transport.Channels.Groups;

namespace Netty.Examples.Common
{
    public interface IChannelWrapper : IDisposable
    {
        bool Active { get; }

        Task RunAsync();

        Task CloseAsync();

        Task WriteAsync(Packet packet);

        IChannelGroup ChannelGroup { get; }

        IChannelGroup NewChannelGroup();
    }
}
