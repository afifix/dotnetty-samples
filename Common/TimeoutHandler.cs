using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class TimeoutHandler : ChannelHandlerAdapter
  {
    private const string Tag = "TIMEOUT";

    private static readonly IInternalLogger Logger;

    static TimeoutHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<TimeoutHandler>();
    }

    public override void UserEventTriggered(IChannelHandlerContext context, object evt)
    {
      if (evt is ReadIdleStateEvent idleStateEvent)
      {
        var channel = context.Channel;
        Logger.Warn($"[{Tag} - {channel.Id}] READ TIMEOUT {idleStateEvent.Retries} | EXCEEDED: {idleStateEvent.MaxRetriesExceeded}");
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