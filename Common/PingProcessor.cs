using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System.Threading.Tasks;

namespace Netty.Examples.Common
{
  public class PingProcessor : SimpleChannelInboundHandler<Ping>
  {
    private static readonly IInternalLogger Logger;

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

    public override Task WriteAsync(IChannelHandlerContext ctx, object message)
    {
      if (message is Pong packet)
      {
        Logger.Trace($"[{ctx.Channel.Id}] channel write: response to {packet.RequestId}");
      }

      return ctx.WriteAsync(message);
    }
  }
}