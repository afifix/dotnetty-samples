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
    public class ServerChannel : BaseChannelWrapper
    {
        private readonly SessionOption _option;
        private readonly ILogger<ServerChannel> _logger;

        private IEventLoopGroup _eventLoopParent;
        private IEventLoopGroup _eventLoopChild;

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

        public override async Task RunAsync()
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
                var clientSessionChannelHandler = new SessionChannelHandler();
                clientSessionChannelHandler.Activated += (o, e) => NewClientConnectedCallback?.Invoke(e);
                clientSessionChannelHandler.Inactivated += (o, e) => ClientDisconnectedCallback?.Invoke(e);

                var pingProcessor = new PacketProcessor<Ping>();
                pingProcessor.Reading += (o, e) => ClientPingCallback?.Invoke(e);

                var subscribeProcessor = new PacketProcessor<Subscribe>();
                subscribeProcessor.Reading += (o, e) => ClientSubscribeCallback?.Invoke(e);

                var timeoutHandler = new TimeoutHandler();
                timeoutHandler.Timedout += (o, e) => ClientTimedoutCallback?.Invoke(e);


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
                            .AddLast("server", clientSessionChannelHandler)
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

        public override async Task CloseAsync()
        {
            if(_closing || _channel == null)
                return;

            _closing = true;

            try
            {
                // close all active sessions
                await ChannelGroup.CloseAsync();
                // disconnect the server
                await _channel.DisconnectAsync();
                // shutdown eventloops
                await Task.WhenAll(
                    _eventLoopParent.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    _eventLoopChild.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

                _channel = null;
                ChannelGroup = null;
                _eventLoopParent = null;
                _eventLoopChild = null;
                _logger.LogInformation("closed.");
            }
            finally
            {
                _closing = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _logger.LogTrace("disposed.");
        }
    }
}
