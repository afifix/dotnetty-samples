using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Netty.Examples.Common;
using System;

namespace Netty.Examples.Server
{
    public class ServerChannelHandler : SimpleChannelInboundHandler<Packet>
    {
        private static readonly IInternalLogger Logger;
        private int _counter;

        static ServerChannelHandler()
        {
            Logger = InternalLoggerFactory.GetInstance<ServerChannelHandler>();
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, Packet packet)
        {
            _counter = 0;
            Logger.Info($"channel read {packet.Type} [{++_counter:000}] ");
            if (packet.Type == PacketType.PINGREQ)
                ctx.WriteAndFlushAsync(Packet.NewPacket(PacketType.PINGRESP));
            else
                ctx.WriteAndFlushAsync(Packet.NewPacket(PacketType.LICRESP));

            _counter++;
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is ReadTimeoutException)
                Logger.Error("channel timeout exception: " + exception.Message);
            else
                Logger.Error("channel exception: " + exception);
            context.CloseAsync();
        }
    }
}
