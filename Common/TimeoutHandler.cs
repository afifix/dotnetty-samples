using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Threading;

namespace Netty.Examples.Common
{
  public class TimeoutHandler : ChannelHandlerAdapter
  {
    private static readonly IInternalLogger Logger;

    static TimeoutHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<TimeoutHandler>();
    }

    public event EventHandler<ReadIdleStateEvent> Timedout;

    public override bool IsSharable => true;

    public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
    {
      if (evt is ReadIdleStateEvent idleStateEvent)
      {
        var channel = ctx.Channel;
        Logger.Warn($"[{channel.Id}] READ TIMEOUT {idleStateEvent.Retries} | EXCEEDED: {idleStateEvent.MaxRetriesExceeded}");
        ThreadPool.QueueUserWorkItem(s => { Timedout?.Invoke(ctx, idleStateEvent); });
      }

      base.UserEventTriggered(ctx, evt);
    }
  }
}