using System;
using System.Threading.Tasks;

using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Sockets;

using Microsoft.Extensions.Logging;

using Netty.Examples.Common;

namespace Netty.Examples.Server
{
    public class ServerChannel : IChannelWrapper
    {
        private IChannel _channel;
        private IEventLoopGroup _eventLoopParent;
        private IEventLoopGroup _eventLoopChild;

        private bool _closing;
        private bool _connecting;

        private readonly SessionOption _option;
        private readonly ILogger<ServerChannel> _logger;

        private bool _disposed;

        internal Action<NettyEventArgs> NewClientConnectedCallback { get; set; }
        internal Action<NettyEventArgs> ClientDisconnectedCallback { get; set; }
        internal Action<ReadIdleStateEventArgs> ClientTimedoutCallback { get; set; }
        internal Action<NettyPacketEventArgs<Ping>> ClientPingCallback { get; set; }
        internal Action<NettyPacketEventArgs<Subscribe>> ClientSubscribeCallback { get; set; }

        public ServerChannel(
            ILoggerFactory loggerFactory,
            ISessionOptionProvider sessionOptionProvider)
        {
            _logger = loggerFactory?.CreateLogger<ServerChannel>() ??
                      throw new ArgumentNullException(nameof(loggerFactory));

            _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
        }

        public bool Active => _channel?.Active ?? false;

        public IChannelGroup ChannelGroup { get; private set; }

        public IChannelGroup NewChannelGroup()
        {
            if(_channel == null || !_channel.Active)
                throw new ApplicationException("server not running.");

            var executor = _channel.Pipeline.FirstContext().Executor;
            return new DefaultChannelGroup(executor);
        }

        public async Task RunAsync()
        {
            if(_connecting || _closing)
                return;

            if(_channel != null && _channel.Active)
                return;

            _connecting = true;
            _channel = null;
            _eventLoopParent = null;
            _eventLoopParent = null;
            if(ChannelGroup != null)
            {
                await ChannelGroup.CloseAsync();
                ChannelGroup = null;
            }

            try
            {
                var sessionChannelHandler = new SessionChannelHandler();
                sessionChannelHandler.Activated += (o, e) => NewClientConnectedCallback?.Invoke(e);
                sessionChannelHandler.Inactivated += (o, e) => ClientDisconnectedCallback?.Invoke(e);
                var pingProcessor = new PingProcessor();
                pingProcessor.Reading += (o, e) => ClientPingCallback?.Invoke(e);
                var timeoutHandler = new TimeoutHandler();
                timeoutHandler.Timedout += (o, e) => ClientTimedoutCallback?.Invoke(e);
                var subscribeProcessor = new SubscribeProcessor();
                subscribeProcessor.Reading += (o, e) => ClientSubscribeCallback?.Invoke(e);

                _eventLoopParent = new MultithreadEventLoopGroup(1);
                _eventLoopChild = new MultithreadEventLoopGroup();

                var bootstrap = new ServerBootstrap()
                    .Group(_eventLoopParent, _eventLoopChild)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel => {
                        _ = channel.Pipeline
                            .AddLast("encoder", new PacketEncoder())
                            .AddLast("decoder", new PacketDecoder())
                            .AddLast("idle", new ReadIdleStateHandler(5, 3))
                            .AddLast("server", sessionChannelHandler)
                            .AddLast("ping", pingProcessor)
                            .AddLast("subscribe", subscribeProcessor)
                            .AddLast("timeout", timeoutHandler);

                        ChannelGroup.Add(channel);
                    }));

                _channel = await bootstrap.BindAsync(_option.Port);
                var executor = _channel.Pipeline.FirstContext().Executor;
                ChannelGroup = new DefaultChannelGroup("clients", executor);

                _logger.LogTrace("started.");
            }
            finally
            {
                _connecting = false;
            }
        }

        public async Task WriteAsync(Packet packet)
        {
            if(_channel == null || !_channel.Active || _closing || _connecting || _disposed)
                return;

            await _channel.WriteAndFlushAsync(packet);
        }

        public async Task CloseAsync()
        {
            if(_closing || _channel == null)
                return;

            _closing = true;

            try
            {
                await Task.WhenAll(
                    _eventLoopParent.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    _eventLoopChild.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

                await ChannelGroup.CloseAsync();

                _channel = null;
                ChannelGroup = null;
                _eventLoopParent = null;
                _eventLoopChild = null;
                _logger.LogTrace("closed.");
            }
            finally
            {
                _closing = false;
            }
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
        {
            if(_disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if(_disposed)
                return;

            if(disposing)
                await CloseAsync();

            _disposed = true;
            _logger.LogTrace("disposed.");
        }

        ~ServerChannel()
        {
            Dispose(false);
        }
    }
}
