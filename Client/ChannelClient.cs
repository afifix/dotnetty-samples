using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public class ChannelClient : IChannelClient, IDisposable
  {
    private readonly ILogger<ChannelClient> _logger;
    private readonly SessionOption _option;

    private IChannel _channel;
    private MultithreadEventLoopGroup _eventLoopGroup;
    private bool _connecting;
    private bool _closing;

    public ChannelClient(
      ILoggerFactory loggerFactory,
      ISessionOptionProvider sessionOptionProvider)
    {
      _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
      _logger = loggerFactory?.CreateLogger<ChannelClient>() ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public bool Disposed { get; set; }

    public bool Active => _channel?.Active ?? false;

    internal Action<object, ReadIdleStateEvent> TimeoutCallback { get; set; }

    internal Action<object, Pong> PongCallback { get; set; }

    public async Task RunAsync()
    {
      if (_connecting || _closing)
        return;

      _connecting = true;
      var pongProcessor = new PongProcessor();
      pongProcessor.Reading += (o, p) => PongCallback?.Invoke(o, p);

      _eventLoopGroup = new MultithreadEventLoopGroup();
      var bootstrap = new Bootstrap();
      bootstrap
        .Group(_eventLoopGroup)
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
          pipeline.AddLast("timeout", new TimeoutHandler());
        }));

      _channel = await bootstrap.ConnectAsync(_option.Host, _option.Port);
      _connecting = false;
    }

    public async Task CloseAsync()
    {
      if (_closing)
        return;

      _closing = true;
      await _eventLoopGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
      _closing = false;
    }

    public void Dispose()
    {
      if (Disposed) return;
      _logger.LogTrace("disposing");
    }
  }
}