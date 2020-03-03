using System;
using System.Threading.Tasks;

using DotNetty.Transport.Channels.Groups;

using Netty.Examples.Common.Packets;

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
