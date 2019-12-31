using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class SubackProcessor : SimpleChannelInboundHandler<Suback>
  {
    private static readonly IInternalLogger Logger;

    static SubackProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<Suback>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Suback packet)
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