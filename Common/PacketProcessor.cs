using System.Threading;

using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
    public abstract class PacketProcessor<T> : SimpleChannelInboundHandler<T>
        where T : Packet
    {
        protected static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<T>();

        public event NettyEventHandler<NettyPacketEventArgs<T>> Reading;

        public override bool IsSharable => true;

        protected override void ChannelRead0(IChannelHandlerContext ctx, T packet)
        {
            Logger.Trace($"channel read0");
            OnReading(ctx, packet);
        }

        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            Logger.Trace($"channel read complete");

            base.ChannelReadComplete(ctx);
        }

        private void OnReading(IChannelHandlerContext ctx, T packet)
        {
            Logger.Trace("raise `Reading` event");
            _ = ThreadPool.QueueUserWorkItem(s => Reading?.Invoke(ctx, new NettyPacketEventArgs<T>(ctx, packet)));
        }
    }
}
