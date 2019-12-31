using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public class ClientChannelHandler : ChannelHandlerAdapter
  {
    private static readonly IInternalLogger Logger;

    public event EventHandler Activated;

    public event EventHandler Inactivated;

    public event EventHandler Closed;

    public event EventHandler Registred;

    public event EventHandler Deregistred;

    public event EventHandler Connected;

    static ClientChannelHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<ClientChannelHandler>();
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
      base.ChannelActive(context);
      ThreadPool.QueueUserWorkItem(s => { Activated?.Invoke(this, EventArgs.Empty); });
    }

    public override async Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
    {
      await base.ConnectAsync(context, remoteAddress, localAddress);
      ThreadPool.QueueUserWorkItem(s => { Connected?.Invoke(this, EventArgs.Empty); });
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
      base.ChannelInactive(context);
      ThreadPool.QueueUserWorkItem(s => { Inactivated?.Invoke(this, EventArgs.Empty); });
    }

    public override void ChannelRegistered(IChannelHandlerContext context)
    {
      base.ChannelRegistered(context);
      ThreadPool.QueueUserWorkItem(s => { Registred?.Invoke(this, EventArgs.Empty); });
    }

    public override async Task DeregisterAsync(IChannelHandlerContext context)
    {
      await base.DeregisterAsync(context);
      ThreadPool.QueueUserWorkItem(s => { Deregistred?.Invoke(this, EventArgs.Empty); });
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

    public override Task DisconnectAsync(IChannelHandlerContext context)
    {
      return base.DisconnectAsync(context);
    }

    public override async Task CloseAsync(IChannelHandlerContext context)
    {
      await base.CloseAsync(context);
      ThreadPool.QueueUserWorkItem(s => { Closed?.Invoke(this, EventArgs.Empty); });
    }
  }
}