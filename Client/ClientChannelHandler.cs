using DotNetty.Common.Concurrency;
using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public class ClientChannelHandler : SimpleChannelInboundHandler<Packet>
  {
    private static readonly Action<object, object> KeepAliveAction;
    private static readonly IInternalLogger Logger;

    private IChannelHandlerContext _ctx;
    private byte[] _array;
    private int _counter;
    private bool _reading;

    private readonly TimeSpan _keepAliveInterval;
    private TimeSpan _lastTimeRead;

    private IScheduledTask _keepAliveScheduledTask;

    static ClientChannelHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<ClientChannelHandler>();
      KeepAliveAction = WrapHandler(KeepAliveAsync);
    }

    private string Name { get; set; }

    public ClientChannelHandler()
    {
      _keepAliveInterval = TimeSpan.FromSeconds(4);
      _reading = false;
    }

    public override void ChannelActive(IChannelHandlerContext ctx)
    {
      Logger.Info($"channel active {ctx.Channel.Id}");

      _array = new byte[256];
      _ctx = ctx;
      Initialize(ctx);
      base.ChannelActive(ctx);

      // Send the initial messages.
      //GenerateTraffic();
    }

    public override void ChannelRead(IChannelHandlerContext ctx, object message)
    {
      Logger.Info($"channel read {ctx.Channel.Id}");

      _reading = true;
      base.ChannelRead(ctx, message);
    }

    protected override void ChannelRead0(IChannelHandlerContext context, Packet packet)
    {
      Logger.Info($"channel read0 {context.Channel.Id}: {packet.Type}");
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Info($"channel read complete {ctx.Channel.Id}");

      if (_reading)
      {
        _reading = false;
        _lastTimeRead = Ticks();
      }
      base.ChannelReadComplete(ctx);
    }

    public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
    {
      if (e is ReadTimeoutException)
      {
        Logger.Error("channel timeout exception: " + e.Message);
      }
      else
      {
        Logger.Error("channel exception: " + e);
        _ctx.CloseAsync();
      }
    }

    private void Initialize(IChannelHandlerContext context)
    {
      _lastTimeRead = Ticks();
      _keepAliveScheduledTask = Schedule(context, KeepAliveAction, this, context, _keepAliveInterval);
    }

    internal virtual TimeSpan Ticks() => TimeUtil.GetSystemTime();

    private static IScheduledTask Schedule(IChannelHandlerContext ctx, Action<object, object> task, object context, object state, TimeSpan delay)
    {
      return ctx.Executor.Schedule(task, context, state, delay);
    }

    private async void GenerateTraffic()
    {
      try
      {
        // Flush the outbound buffer to the socket.
        // Once flushed, generate the same amount of traffic again.
        Logger.Info($"writing [{++_counter:000}]");
        await _ctx.WriteAndFlushAsync(Packet.NewPacket(PacketType.LICREQ));
        await Task.Delay(7000);
        GenerateTraffic();
      }
      catch
      {
        await _ctx.CloseAsync();
      }
    }

    private static Action<object, object> WrapHandler(Action<ClientChannelHandler, IChannelHandlerContext> action)
    {
      return (handler, ctx) =>
      {
        if (!(handler is ClientChannelHandler self) || !(ctx is IChannelHandlerContext context))
          return;

        if (!context.Channel.Open)
          return;

        action(self, context);
      };
    }

    private static async void KeepAliveAsync(ClientChannelHandler self, IChannelHandlerContext context)
    {
      // calculer la prochaine delais du PINGREQ
      var nextDelay = self._keepAliveInterval;
      if (!self._reading)
      {
        nextDelay -= self.Ticks() - self._lastTimeRead;
      }

      // demander PINGREQ si seulement si seulement si pas d'activite pendant [KeepAliveInterval]
      if (!self._reading && nextDelay.Ticks <= 0)
      {
        // envoyer une PINGREQ
        await context.WriteAndFlushAsync(Packet.NewPacket(PacketType.PINGREQ));
        // reinitialiser le delais
        nextDelay = self._keepAliveInterval;
      }

      // here we go again
      self._keepAliveScheduledTask = Schedule(context, KeepAliveAction, self, context, nextDelay);
    }
  }
}