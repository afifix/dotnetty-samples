using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class PingProcessor : SimpleChannelInboundHandler<Ping>
  {
    private static readonly IInternalLogger Logger;

    public override bool IsSharable => true;

    static PingProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<PingProcessor>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Ping msg)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read0 {msg.Id}");
      var packet = Pong.New(msg);
      ctx.WriteAndFlushAsync(packet);
    }
  }
}