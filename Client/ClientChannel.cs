using System;
using System.Threading.Tasks;

using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Sockets;

using Microsoft.Extensions.Logging;

using Netty.Examples.Common;

namespace Netty.Examples.Client
{
    public class ClientChannel : IChannelWrapper
    {
        private readonly ILogger<ClientChannel> _logger;
        private readonly SessionOption _option;

        private IChannel _channel;
        private IEventLoopGroup _eventLoop;
        private bool _connecting;
        private bool _closing;
        private bool _disposed;

        public ClientChannel(
          ILoggerFactory loggerFactory,
          ISessionOptionProvider sessionOptionProvider)
        {
            _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
            _logger = loggerFactory?.CreateLogger<ClientChannel>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }


        public bool Active => _channel?.Active ?? false;

        internal Action<ReadIdleStateEventArgs> TimeoutCallback { get; set; }

        internal Action<Pong> PongCallback { get; set; }

        internal Action ConnectedCallback { get; set; }

        internal Action ClosedCallback { get; set; }

        internal Action<Suback> SubackCallback { get; set; }

        public async Task RunAsync()
        {
            if(_connecting || _closing)
                return;

            if(_channel != null && _channel.Active)
                return;

            _connecting = true;
            _channel = null;
            _eventLoop = null;

            var pongProcessor = new PacketProcessor<Pong>();
            pongProcessor.Reading += (o, e) => PongCallback?.Invoke(e.Packet);

            var subackProcessor = new PacketProcessor<Suback>();
            subackProcessor.Reading += (o, e) => SubackCallback?.Invoke(e.Packet);

            var clientHandler = new SessionChannelHandler();
            clientHandler.Connected += (o, e) => ConnectedCallback?.Invoke();
            clientHandler.Closed += (o, e) => ClosedCallback?.Invoke();

            var timedoutHandler = new TimeoutHandler();
            timedoutHandler.Timedout += (o, e) => TimeoutCallback?.Invoke(e);

            try
            {
                _eventLoop = new MultithreadEventLoopGroup();
                var bootstrap = new Bootstrap()
                  .Group(_eventLoop)
                  .Channel<TcpSocketChannel>()
                  .Option(ChannelOption.TcpNodelay, true)
                  .Handler(new ActionChannelInitializer<ISocketChannel>(channel => {
                      _ = channel.Pipeline
                          .AddLast("encoder", new PacketEncoder())
                          .AddLast("decoder", new PacketDecoder())
                          .AddLast("keep-alive", new KeepMeAliveChannel(_option.KeepAliveInterval))
                          .AddLast("idle", new ReadIdleStateHandler(_option.IdleTimeout, _option.KeepAliveRetries))
                          .AddLast("pong", pongProcessor)
                          .AddLast("suback", subackProcessor)
                          .AddLast("timeout", timedoutHandler)
                          .AddLast("client", clientHandler);
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
            if(_closing || _channel == null)
                return;

            _closing = true;

            if(!_channel.Active)
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

        public async Task WriteAsync(Packet packet)
        {
            if(_channel == null || !_channel.Active || _closing || _connecting || _disposed)
                return;

            await _channel.WriteAndFlushAsync(packet);
        }

        public IChannelGroup ChannelGroup { get; }

        public IChannelGroup NewChannelGroup() => throw new NotImplementedException();

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

        ~ClientChannel()
        {
            Dispose(false);
        }
    }
}
