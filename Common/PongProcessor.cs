using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class PongProcessor : SimpleChannelInboundHandler<Pong>
  {
    private const string Tag = "PONG";

    private static readonly IInternalLogger Logger;

    static PongProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<PongProcessor>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Pong packet)
    {
      Logger.Trace($"[{Tag} - {ctx.Channel.Id}] channel read0: {packet.Latency}");
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{Tag} - {ctx.Channel.Id}] channel read complete");

      base.ChannelReadComplete(ctx);
    }
  }
}