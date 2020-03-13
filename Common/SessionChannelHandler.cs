using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
    public class SessionChannelHandler : ChannelHandlerAdapter
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<SessionChannelHandler>();

        public event NettyEventHandler Activated;

        public event NettyEventHandler Inactivated;

        public event NettyEventHandler Closed;

        public event NettyEventHandler Registred;

        public event NettyEventHandler Deregistred;

        public event NettyEventHandler Connected;

        public event NettyEventHandler Disconnected;

        public event NettyEventHandler Reading;

        public event NettyEventHandler Writing;

        public override bool IsSharable => true;

        public override void ChannelActive(IChannelHandlerContext context)
        {
            Logger.Trace("channel active");
            _ = ThreadPool.QueueUserWorkItem(s => Activated?.Invoke(this, new NettyEventArgs(context)));
            base.ChannelActive(context);
        }

        public override async Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            Logger.Trace("connect async");
            _ = ThreadPool.QueueUserWorkItem(s => Connected?.Invoke(this, new NettyEventArgs(context)));
            await base.ConnectAsync(context, remoteAddress, localAddress).ConfigureAwait(false);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Logger.Trace("channel inactive");
            _ = ThreadPool.QueueUserWorkItem(s => Inactivated?.Invoke(this, new NettyEventArgs(context)));
            base.ChannelInactive(context);
        }

        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            Logger.Trace("channel registred");
            _ = ThreadPool.QueueUserWorkItem(s => Registred?.Invoke(this, new NettyEventArgs(context)));
            base.ChannelRegistered(context);
        }

        public override async Task DeregisterAsync(IChannelHandlerContext context)
        {
            Logger.Trace("channel deregistred");
            _ = ThreadPool.QueueUserWorkItem(s => Deregistred?.Invoke(this, new NettyEventArgs(context)));
            await base.DeregisterAsync(context).ConfigureAwait(false);
        }

        public override async Task DisconnectAsync(IChannelHandlerContext context)
        {
            Logger.Trace("disconnect async");
            _ = ThreadPool.QueueUserWorkItem(s => Disconnected?.Invoke(this, new NettyEventArgs(context)));
            await base.DisconnectAsync(context).ConfigureAwait(false);
        }

        public override async Task CloseAsync(IChannelHandlerContext context)
        {
            Logger.Trace("close async");
            _ = ThreadPool.QueueUserWorkItem(s => Closed?.Invoke(this, new NettyEventArgs(context)));
            await base.CloseAsync(context).ConfigureAwait(false);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            Logger.Trace("channel read");
            _ = ThreadPool.QueueUserWorkItem(s => Reading?.Invoke(this, new NettyEventArgs(context)));
            base.ChannelRead(context, message);
        }

        public override async Task WriteAsync(IChannelHandlerContext context, object message)
        {
            Logger.Trace("write async");
            _ = ThreadPool.QueueUserWorkItem(s => Writing?.Invoke(this, new NettyEventArgs(context)));
            await base.WriteAsync(context, message).ConfigureAwait(false);
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Logger.Trace("exception caught");
            if(e is ReadTimeoutException re)
            {
                Logger.Error($"[{ctx.Channel.Id}] channel timeout exception: {re.Message}");
                return;
            }

            Logger.Error($"[{ctx.Channel.Id}] channel exception: {e.Message}");
        }
    }
}
