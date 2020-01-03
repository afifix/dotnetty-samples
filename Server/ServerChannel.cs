using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Server
{
    public class ServerChannel : IChannelWrapper, IDisposable
    {
        private IChannel _channel;
        private IEventLoopGroup _eventLoopParent;
        private IEventLoopGroup _eventLoopChild;

        private bool _closing;
        private bool _connecting;

        private readonly SessionOption _option;
        private readonly ILogger<ServerChannel> _logger;

        internal Action<object, ReadIdleStateEvent> TimeoutCallback { get; set; }

        public ServerChannel(
            ILoggerFactory loggerFactory,
            ISessionOptionProvider sessionOptionProvider)
        {
            _logger = loggerFactory?.CreateLogger<ServerChannel>() ??
                      throw new ArgumentNullException(nameof(loggerFactory));

            _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
        }

        public bool Disposed { get; set; }

        public bool Active => _channel?.Active ?? false;

        public async Task RunAsync()
        {
            if (_connecting || _closing)
                return;

            if (_channel != null && _channel.Active)
                return;

            _connecting = true;
            _channel = null;
            _eventLoopParent = null;
            _eventLoopParent = null;

            try
            {
                _eventLoopParent = new MultithreadEventLoopGroup(1);
                _eventLoopChild = new MultithreadEventLoopGroup();

                var sessionChannelHandler = new SessionChannelHandler();
                sessionChannelHandler.Activated += (o, e) => _logger.LogInformation("ACTIVATED");
                sessionChannelHandler.Inactivated += (o, e) => _logger.LogInformation("INACTIVATED");

                var bootstrap = new ServerBootstrap();
                bootstrap.Group(_eventLoopParent, _eventLoopChild);
                bootstrap.Channel<TcpServerSocketChannel>();

                var pingProcessor = new PingProcessor();
                var timeoutHandler = new TimeoutHandler();
                var subscribeProcessor = new SubscribeProcessor();

                var decoder = new PacketDecoder();
                bootstrap
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        channel.Pipeline
                            .AddLast("encoder", new PacketEncoder())
                            .AddLast("decoder", new PacketDecoder())
                            .AddLast("idle", new ReadIdleStateHandler(20, 3))
                            .AddLast("server", sessionChannelHandler)
                            .AddLast("ping", pingProcessor)
                            .AddLast("subscribe", subscribeProcessor)
                            .AddLast("timeout", timeoutHandler);
                    }));

                _channel = await bootstrap.BindAsync(_option.Port);
                _logger.LogTrace("started.");
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

            try
            {
                await Task.WhenAll(
                    _eventLoopParent.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    _eventLoopChild.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

                _channel = null;
                _eventLoopParent = null;
                _eventLoopChild = null;
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
