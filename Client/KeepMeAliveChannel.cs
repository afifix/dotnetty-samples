using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Netty.Examples.Common;
using System;

namespace Netty.Examples.Client
{
  public class KeepMeAliveChannel : ChannelHandlerAdapter
  {
    private static readonly Action<object, object> KeepAliveAction;
    private static readonly IInternalLogger Logger;

    private bool _initialized;
    private bool _destroyed;

    static KeepMeAliveChannel()
    {
      Logger = InternalLoggerFactory.GetInstance<KeepMeAliveChannel>();
      KeepAliveAction = WrapHandler(KeepAliveAsync);
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="interval">interval in seconds between ping requests</param>
    public KeepMeAliveChannel(int interval)
    {
      KeepAliveInterval = TimeSpan.FromSeconds(interval);
      Reading = false;
    }

    public IScheduledTask KeepAliveScheduledTask { get; set; }

    public TimeSpan LastTimeRead { get; private set; }

    public TimeSpan KeepAliveInterval { get; }

    public bool Reading { get; private set; }

    public override void ChannelActive(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel active");

      Initialize(ctx);
      base.ChannelActive(ctx);
    }

    public override void HandlerAdded(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel handler added");

      if (ctx.Channel.Active && ctx.Channel.Registered)
      {
        // channelActive() event has been fired already, which means this.channelActive() will
        // not be invoked. We have to initialize here instead.
        Initialize(ctx);
      }

      base.HandlerAdded(ctx);
    }

    public override void ChannelRegistered(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel registred");

      // initialize early if channel is active already
      if (ctx.Channel.Active)
      {
        Initialize(ctx);
      }

      base.ChannelRegistered(ctx);
    }

    public override void ChannelInactive(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel inactive");

      Destroy(ctx);
      base.ChannelInactive(ctx);
    }

    public override void HandlerRemoved(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] handler removed");

      Destroy(ctx);
      base.HandlerRemoved(ctx);
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object message)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read");

      Reading = true;
      base.ChannelRead(ctx, message);
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read complete");

      if (Reading)
      {
        Reading = false;
        LastTimeRead = Ticks();
      }
      base.ChannelReadComplete(ctx);
    }

    internal virtual TimeSpan Ticks() => TimeUtil.GetSystemTime();

    private void Initialize(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel initialize");

      if (_initialized || _destroyed) return;
      _initialized = true;
      LastTimeRead = Ticks();
      KeepAliveScheduledTask = Schedule(ctx, KeepAliveAction, this, ctx, KeepAliveInterval);
    }

    private void Destroy(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel destroy");

      if (_destroyed) return;
      _destroyed = true;
      if (KeepAliveScheduledTask == null) return;
      KeepAliveScheduledTask.Cancel();
      KeepAliveScheduledTask = null;
    }

    private static IScheduledTask Schedule(IChannelHandlerContext ctx, Action<object, object> task, object context, object state, TimeSpan delay)
    {
      return ctx.Executor.Schedule(task, context, state, delay);
    }

    private static Action<object, object> WrapHandler(Action<KeepMeAliveChannel, IChannelHandlerContext> action)
    {
      return (handler, ctx) =>
      {
        if (!(handler is KeepMeAliveChannel self) || !(ctx is IChannelHandlerContext context)) return;
        if (!context.Channel.Open) return;
        action(self, context);
      };
    }

    private static async void KeepAliveAsync(KeepMeAliveChannel self, IChannelHandlerContext ctx)
    {
      // calculate next delay
      var nextDelay = self.KeepAliveInterval;
      if (!self.Reading)
      {
        nextDelay -= self.Ticks() - self.LastTimeRead;
      }

      // send a PINGREQ only if channel is not active
      if (!self.Reading && nextDelay.Ticks <= 0)
      {
        await ctx.WriteAndFlushAsync(Ping.New());
        // reset delay
        nextDelay = self.KeepAliveInterval;
      }

      self.KeepAliveScheduledTask = Schedule(ctx, KeepAliveAction, self, ctx, nextDelay);
    }
  }
}