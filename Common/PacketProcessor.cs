using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System.Threading;

namespace Netty.Examples.Common
{
  public abstract class PacketProcessor<T> : SimpleChannelInboundHandler<T>
    where T : Packet
  {
    // ReSharper disable once StaticMemberInGenericType
    protected static readonly IInternalLogger Logger;

    public event NettyEventHandler<T> Reading;

    public override bool IsSharable => true;

    static PacketProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<T>();
    }

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
      ThreadPool.QueueUserWorkItem(s => { Reading?.Invoke(ctx, packet); });
    }
  }
}