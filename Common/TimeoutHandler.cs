using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class TimeoutHandler : ChannelHandlerAdapter
  {
    private static readonly IInternalLogger Logger;

    static TimeoutHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<TimeoutHandler>();
    }

    public override bool IsSharable => true;

    public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
    {
      if (evt is ReadIdleStateEvent idleStateEvent)
      {
        var channel = ctx.Channel;
        Logger.Warn($"[{channel.Id}] READ TIMEOUT {idleStateEvent.Retries} | EXCEEDED: {idleStateEvent.MaxRetriesExceeded}");
        if (idleStateEvent.MaxRetriesExceeded)
        {
          ctx.FireExceptionCaught(ReadTimeoutException.Instance);
          ctx.CloseAsync();
        }
      }

      base.UserEventTriggered(ctx, evt);
    }
  }
}