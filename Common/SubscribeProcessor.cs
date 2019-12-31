using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class SubscribeProcessor : SimpleChannelInboundHandler<Subscribe>
  {
    private static readonly IInternalLogger Logger;

    public override bool IsSharable => true;

    static SubscribeProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<PongProcessor>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Subscribe packet)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read0");
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read complete");

      base.ChannelReadComplete(ctx);
    }
  }
}