using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Netty.Examples.Common;
using System;

namespace Netty.Examples.Client
{
  public class ClientChannelHandler : SimpleChannelInboundHandler<Packet>
  {
    private static readonly IInternalLogger Logger;

    static ClientChannelHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<ClientChannelHandler>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Packet packet)
    {
      Logger.Info($"[{ctx.Channel.Id}] channel read0 {packet.Type}");
    }

    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
      if (e is ReadTimeoutException re)
      {
        Logger.Error($"[{ctx.Channel.Id}] channel timeout exception: {re.Message}");
        ctx.CloseAsync();
        return;
      }

      Logger.Error($"[{ctx.Channel.Id}] channel exception: {e.Message}");
      ctx.CloseAsync();
    }
  }
}