using System.Threading;

using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
    public class TimeoutHandler : ChannelHandlerAdapter
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<TimeoutHandler>();

        public event NettyEventHandler<ReadIdleStateEventArgs> Timedout;

        public override bool IsSharable => true;

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if(evt is ReadIdleStateEventArgs idleStateEvent)
            {
                var channel = ctx.Channel;
                Logger.Warn($"[{channel.Id}] READ TIMEOUT {idleStateEvent.Retries} | EXCEEDED: {idleStateEvent.MaxRetriesExceeded}");
                _ = ThreadPool.QueueUserWorkItem(s => Timedout?.Invoke(ctx, idleStateEvent));
            }

            base.UserEventTriggered(ctx, evt);
        }
    }
}
