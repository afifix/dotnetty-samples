using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public class ClientChannel : IChannelWrapper, IDisposable
  {
    private readonly ILogger<ClientChannel> _logger;
    private readonly SessionOption _option;

    private IChannel _channel;
    private IEventLoopGroup _eventLoop;
    private bool _connecting;
    private bool _closing;

    public ClientChannel(
      ILoggerFactory loggerFactory,
      ISessionOptionProvider sessionOptionProvider)
    {
      _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
      _logger = loggerFactory?.CreateLogger<ClientChannel>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public bool Disposed { get; set; }

    public bool Active => _channel?.Active ?? false;

    internal Action<ReadIdleStateEvent> TimeoutCallback { get; set; }

    internal Action<Pong> PongCallback { get; set; }

    internal Action ConnectedCallback { get; set; }

    internal Action ClosedCallback { get; set; }

    public async Task RunAsync()
    {
      if (_connecting || _closing)
        return;

      if (_channel != null && _channel.Active)
        return;

      _connecting = true;
      _channel = null;
      _eventLoop = null;

      var pongProcessor = new PongProcessor();
      pongProcessor.Reading += (o, p) => PongCallback?.Invoke(p);

      var clientHandler = new SessionChannelHandler();
      clientHandler.Connected += (o, e) => ConnectedCallback?.Invoke();
      clientHandler.Closed += (o, e) => ClosedCallback?.Invoke();

      var timedoutHandler = new TimeoutHandler();
      timedoutHandler.Timedout += (o, e) => TimeoutCallback?.Invoke(e);

      try
      {
        _eventLoop = new MultithreadEventLoopGroup();
        var bootstrap = new Bootstrap();
        bootstrap
          .Group(_eventLoop)
          .Channel<TcpSocketChannel>()
          .Option(ChannelOption.TcpNodelay, true)
          .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
          {
            var pipeline = channel.Pipeline;

            pipeline.AddLast("encoder", new PacketEncoder());
            pipeline.AddLast("decoder", new PacketDecoder());
            pipeline.AddLast("keep-alive", new KeepMeAliveChannel(_option.KeepAliveInterval));
            pipeline.AddLast("idle", new ReadIdleStateHandler(_option.IdleTimeout, _option.KeepAliveRetries));
            pipeline.AddLast("pong", pongProcessor);
            pipeline.AddLast("timeout", timedoutHandler);
            pipeline.AddLast("client", clientHandler);
          }));

        _channel = await bootstrap.ConnectAsync(_option.Host, _option.Port);
        _logger.LogTrace("connected.");
      }
      finally
      {
        _connecting = false;
      }
    }

    public async Task CloseAsync()
    {
      if (_closing || _channel == null)
        return;

      _closing = true;

      if (!_channel.Active)
      {
        _channel = null;
        _eventLoop = null;
        _closing = false;
        return;
      }

      try
      {
        await _channel.CloseAsync();
        await _eventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
        _channel = null;
        _eventLoop = null;
        _logger.LogTrace("closed.");
      }
      finally
      {
        _closing = false;
      }
    }

    public async void Dispose()
    {
      if (Disposed) return;
      await CloseAsync();
      Disposed = true;
      _logger.LogTrace("disposed.");
    }
  }
}