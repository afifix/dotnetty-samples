using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;

namespace Netty.Examples.Common
{
  public class PongProcessor : SimpleChannelInboundHandler<Pong>
  {
    private const string Tag = "PONG";

    private static readonly IInternalLogger Logger;
    private readonly Action<Pong> _callback;

    static PongProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<PongProcessor>();
    }

    public PongProcessor(Action<Pong> callback)
    {
      _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Pong packet)
    {
      Logger.Trace($"[{Tag} - {ctx.Channel.Id}] channel read0: {packet.Latency}");
      _callback(packet);
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{Tag} - {ctx.Channel.Id}] channel read complete");

      base.ChannelReadComplete(ctx);
    }
  }
}