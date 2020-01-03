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

    public event EventHandler Activated;

    public event EventHandler Inactivated;

    public event EventHandler Closed;

    public event EventHandler Registred;

    public event EventHandler Deregistred;

    public event EventHandler Connected;

    public event EventHandler Disconnected;

    public event EventHandler<object> Reading;

    public event EventHandler<object> Writing;

    static SessionChannelHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<SessionChannelHandler>();
    }

    public override bool IsSharable => true;

    public override void ChannelActive(IChannelHandlerContext context)
    {
      base.ChannelActive(context);
      ThreadPool.QueueUserWorkItem(s => { Activated?.Invoke(context, EventArgs.Empty); });
    }

    public override async Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
    {
      await base.ConnectAsync(context, remoteAddress, localAddress);
      ThreadPool.QueueUserWorkItem(s => { Connected?.Invoke(context, EventArgs.Empty); });
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      base.ChannelInactive(context);
      ThreadPool.QueueUserWorkItem(s => { Inactivated?.Invoke(context, EventArgs.Empty); });
    }

    public override void ChannelRegistered(IChannelHandlerContext context)
    {
      base.ChannelRegistered(context);
      ThreadPool.QueueUserWorkItem(s => { Registred?.Invoke(context, EventArgs.Empty); });
    }

    public override async Task DeregisterAsync(IChannelHandlerContext context)
    {
      await base.DeregisterAsync(context);
      ThreadPool.QueueUserWorkItem(s => { Deregistred?.Invoke(context, EventArgs.Empty); });
    }

    public override async Task DisconnectAsync(IChannelHandlerContext context)
    {
      await base.DisconnectAsync(context);
      ThreadPool.QueueUserWorkItem(s => { Disconnected?.Invoke(context, EventArgs.Empty); });
    }

    public override async Task CloseAsync(IChannelHandlerContext context)
    {
      await base.CloseAsync(context);
      ThreadPool.QueueUserWorkItem(s => { Closed?.Invoke(context, EventArgs.Empty); });
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
      base.ChannelRead(context, message);
      ThreadPool.QueueUserWorkItem(s => { Reading?.Invoke(context, message); });
    }

    public override async Task WriteAsync(IChannelHandlerContext context, object message)
    {
      await base.WriteAsync(context, message);
      ThreadPool.QueueUserWorkItem(s => { Writing?.Invoke(context, message); });
    }

    public override async void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
      if (e is ReadTimeoutException re)
      {
        Logger.Error($"[{ctx.Channel.Id}] channel timeout exception: {re.Message}");
        await CloseAsync(ctx);
        return;
      }

      Logger.Error($"[{ctx.Channel.Id}] channel exception: {e.Message}");
      await CloseAsync(ctx);
    }
  }
}