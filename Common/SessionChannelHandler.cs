using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Netty.Examples.Common
{
  public class SessionChannelHandler : ChannelHandlerAdapter
  {
    private static readonly IInternalLogger Logger;

    public event NettyEventHandler Activated;

    public event NettyEventHandler Inactivated;

    public event NettyEventHandler Closed;

    public event NettyEventHandler Registred;

    public event NettyEventHandler Deregistred;

    public event NettyEventHandler Connected;

    public event NettyEventHandler Disconnected;

    public event NettyEventHandler<object> Reading;

    public event NettyEventHandler<object> Writing;

    static SessionChannelHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<SessionChannelHandler>();
    }

    public override bool IsSharable => true;

    public override void ChannelActive(IChannelHandlerContext context)
    {
      Logger.Trace("channel active");
      ThreadPool.QueueUserWorkItem(s => { Activated?.Invoke(context, EventArgs.Empty); });
      base.ChannelActive(context);
    }

    public override async Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
    {
      Logger.Trace("connect async");
      ThreadPool.QueueUserWorkItem(s => { Connected?.Invoke(context, EventArgs.Empty); });
      await base.ConnectAsync(context, remoteAddress, localAddress);
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      Logger.Trace("channel inactive");
      ThreadPool.QueueUserWorkItem(s => { Inactivated?.Invoke(context, EventArgs.Empty); });
      base.ChannelInactive(context);
    }

    public override void ChannelRegistered(IChannelHandlerContext context)
    {
      Logger.Trace("channel registred");
      ThreadPool.QueueUserWorkItem(s => { Registred?.Invoke(context, EventArgs.Empty); });
      base.ChannelRegistered(context);
    }

    public override async Task DeregisterAsync(IChannelHandlerContext context)
    {
      Logger.Trace("channel deregistred");
      ThreadPool.QueueUserWorkItem(s => { Deregistred?.Invoke(context, EventArgs.Empty); });
      await base.DeregisterAsync(context);
    }

    public override async Task DisconnectAsync(IChannelHandlerContext context)
    {
      Logger.Trace("disconnect async");
      ThreadPool.QueueUserWorkItem(s => { Disconnected?.Invoke(context, EventArgs.Empty); });
      await base.DisconnectAsync(context);
    }

    public override async Task CloseAsync(IChannelHandlerContext context)
    {
      Logger.Trace("close async");
      ThreadPool.QueueUserWorkItem(s => { Closed?.Invoke(context, EventArgs.Empty); });
      await base.CloseAsync(context);
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
      Logger.Trace("channel read");
      ThreadPool.QueueUserWorkItem(s => { Reading?.Invoke(context, message); });
      base.ChannelRead(context, message);
    }

    public override async Task WriteAsync(IChannelHandlerContext context, object message)
    {
      Logger.Trace("write async");
      ThreadPool.QueueUserWorkItem(s => { Writing?.Invoke(context, message); });
      await base.WriteAsync(context, message);
    }

    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
      Logger.Trace("exception caught");
      if (e is ReadTimeoutException re)
      {
        Logger.Error($"[{ctx.Channel.Id}] channel timeout exception: {re.Message}");
        return;
      }

      Logger.Error($"[{ctx.Channel.Id}] channel exception: {e.Message}");
    }
  }
}