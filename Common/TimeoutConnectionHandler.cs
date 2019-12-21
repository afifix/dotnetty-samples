using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class TimeoutConnectionHandler : ChannelHandlerAdapter
  {
    private static readonly IInternalLogger Logger;

    static TimeoutConnectionHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<TimeoutConnectionHandler>();
    }

    public override void UserEventTriggered(IChannelHandlerContext context, object evt)
    {
      if (evt is ReadIdleStateEvent idleStateEvent)
      {
        var channel = context.Channel;
        Logger.Warn($"[{channel.Id}] READ TIMEOUT {idleStateEvent.Retries} | EXCEEDED: {idleStateEvent.MaxRetriesExceeded}");
        if (idleStateEvent.MaxRetriesExceeded)
        {
          context.FireExceptionCaught(ReadTimeoutException.Instance);
          context.CloseAsync();
        }
      }

      base.UserEventTriggered(context, evt);
    }
  }
}