﻿using System;
using System.Threading.Tasks;

using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

using Microsoft.Extensions.Logging;

using Netty.Examples.Common;
using Netty.Examples.Common.Packets;

namespace Netty.Examples.Client
{
    public class ClientChannel : BaseChannelWrapper
    {
        private readonly ILogger<ClientChannel> _logger;
        private readonly SessionOption _option;

        private IEventLoopGroup _eventLoop;

        public ClientChannel(
          ILoggerFactory loggerFactory,
          ISessionOptionProvider sessionOptionProvider)
        {
            _option = sessionOptionProvider?.Get() ?? throw new ArgumentNullException(nameof(sessionOptionProvider));
            _logger = loggerFactory?.CreateLogger<ClientChannel>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        internal Action<ReadIdleStateEventArgs> TimedoutCallback { get; set; }

        internal Action<Pong> PingResponseReceivedCallback { get; set; }

        internal Action ConnectedCallback { get; set; }

        internal Action ClosedCallback { get; set; }

        internal Action<Suback> SubackReceivedCallback { get; set; }

        public override async Task RunAsync()
        {
            if(_connecting || _closing)
                return;

            if(_channel != null && _channel.Active)
                return;

            _connecting = true;

            var pingResponseProcessor = new PacketProcessor<Pong>();
            pingResponseProcessor.Reading += (o, e) => PingResponseReceivedCallback?.Invoke(e.Packet);

            var subackProcessor = new PacketProcessor<Suback>();
            subackProcessor.Reading += (o, e) => SubackReceivedCallback?.Invoke(e.Packet);

            var sessionChannelHandler = new SessionChannelHandler();
            sessionChannelHandler.Closed += async (o, e) => await CloseAsync().ConfigureAwait(false);
            sessionChannelHandler.Inactivated += async (o, e) => await CloseAsync().ConfigureAwait(false);

            var timedoutHandler = new TimeoutHandler();
            timedoutHandler.Timedout += (o, e) => TimedoutCallback?.Invoke(e);

            try
            {
                _eventLoop = new MultithreadEventLoopGroup();
                var bootstrap = new Bootstrap()
                  .Group(_eventLoop)
                  .Channel<TcpSocketChannel>()
                  .Option(ChannelOption.TcpNodelay, true)
                  .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(2000))
                  .Handler(new ActionChannelInitializer<ISocketChannel>(channel => {
                      _ = channel.Pipeline
                          .AddLast("encoder", new PacketEncoder())
                          .AddLast("decoder", new PacketDecoder())
                          .AddLast("keep-alive", new KeepMeAliveChannel(_option.KeepAliveInterval))
                          .AddLast("idle", new ReadIdleStateHandler(_option.IdleTimeout, _option.KeepAliveRetries))
                          .AddLast("ping-response", pingResponseProcessor)
                          .AddLast("suback", subackProcessor)
                          .AddLast("timeout", timedoutHandler)
                          .AddLast("client", sessionChannelHandler);
                  }));

                _channel = await bootstrap.ConnectAsync(_option.Host, _option.Port).ConfigureAwait(false);
                ConnectedCallback?.Invoke();
                _logger.LogTrace("connected.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
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
                if(_eventLoop.IsShutdown || _eventLoop.IsShuttingDown)
                    return;

                await _channel.CloseAsync().ConfigureAwait(false);
                await _eventLoop.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                _logger.LogTrace("closed.");
                ClosedCallback();
            }
            finally
            {
                _closing = false;
                _channel = null;
                _eventLoop = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _logger.LogTrace("disposed.");
        }
    }
}
