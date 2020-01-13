using System;

using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
    public class NettyPacketEventArgs<T> : NettyEventArgs
        where T : Packet
    {
        public NettyPacketEventArgs(IChannelHandlerContext context, T value)
            : base(context)
        {
            Packet = value ?? throw new ArgumentNullException(nameof(value));
        }

        public T Packet { get; }

        public PacketEventArgs<T> ToPacketEventArgs() => new PacketEventArgs<T>(Packet);
    }
}
