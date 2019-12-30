using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Threading;

namespace Netty.Examples.Common
{
  public class PongProcessor : SimpleChannelInboundHandler<Pong>
  {
    private static readonly IInternalLogger Logger;

    public event EventHandler<Pong> Reading;

    static PongProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<PongProcessor>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Pong packet)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read0: {packet.Latency}");
      OnReading(packet);
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read complete");

      base.ChannelReadComplete(ctx);
    }

    private void OnReading(Pong pong)
    {
      Logger.Trace("raise `Reading` event");
      ThreadPool.QueueUserWorkItem(s => { Reading?.Invoke(this, pong); });
    }
  }
}