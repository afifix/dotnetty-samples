using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public class Client : ISession
  {
    private readonly SessionOption _option;
    private readonly ILogger<Client> _logger;

    private MultithreadEventLoopGroup _workerGroup;
    private IChannel _channel;
    private readonly IServiceProvider _sericeProvider;

    public Client(ILoggerFactory loggerFactory, ISessionOptionProvider sessionOptionProvider, IServiceProvider serviceProvider)
    {
      _logger = loggerFactory?.CreateLogger<Client>() ?? throw new ArgumentNullException(nameof(loggerFactory));
      _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
      _sericeProvider = serviceProvider;
    }

    public event EventHandler Connected;

    public event EventHandler Closed;

    public event EventHandler<Pong> Ponged;

    public void OnConnected()
    {
      _logger.LogTrace("raise `Connected` event");
      ThreadPool.QueueUserWorkItem(s => { Connected?.Invoke(this, EventArgs.Empty); });
    }

    public void OnClosed()
    {
      _logger.LogTrace("raise `Closed` event");
      ThreadPool.QueueUserWorkItem(s => { Closed?.Invoke(this, EventArgs.Empty); });
    }

    public void OnPonged(Pong packet)
    {
      _logger.LogTrace("raise `Ponged` event");
      ThreadPool.QueueUserWorkItem(s => { Ponged?.Invoke(this, packet); });
    }

    public async Task RunAsync()
    {
      _workerGroup = new MultithreadEventLoopGroup();
      var bootstrap = new Bootstrap();
      bootstrap
        .Group(_workerGroup)
        .Channel<TcpSocketChannel>()
        .Option(ChannelOption.TcpNodelay, true)
        .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
        {
          var pipeline = channel.Pipeline;

          pipeline.AddLast("encoder", new PacketEncoder());
          pipeline.AddLast("decoder", new PacketDecoder());
          pipeline.AddLast("keep-alive", new KeepMeAliveChannel(4));
          pipeline.AddLast("idle", new ReadIdleStateHandler(10, 3));
          pipeline.AddLast("client", new ClientChannelHandler());
          pipeline.AddLast("ping", new PingProcessor());
          pipeline.AddLast("pong", new PongProcessor(OnPonged));
          pipeline.AddLast("timeout", new TimeoutHandler());
        }));

      _channel = await bootstrap.ConnectAsync(_option.Host, _option.Port);
      OnConnected();
    }

    public async Task CloseAsync()
    {
      await _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
      OnClosed();
    }
  }
}