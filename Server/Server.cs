using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Netty.Examples.Server
{
    public class Server : IServerSession, IDisposable
    {
        private readonly ILogger<Server> _logger;
        private readonly IChannelFactory _channelFactory;
        private readonly ISubjectProvider _subjectProvider;

        private IChannelWrapper _channel;

        public Server(
          ILoggerFactory loggerFactory,
          IChannelFactory channelFactory,
          ISubjectProvider subjectProvider)
        {
            _logger = loggerFactory?.CreateLogger<Server>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
            _subjectProvider = subjectProvider ?? throw new ArgumentNullException(nameof(subjectProvider));
        }

        public event EventHandler Connected;

        public event EventHandler Closed;

        public event EventHandler<ReadIdleStateEvent> ClientTimedout;

        public event EventHandler<Ping> ClientPinged;

        public event EventHandler<Subscribe> ClientSubscribing;

        public event EventHandler<Subscribe> ClientSubscribed;

        public event EventHandler NewClientConnected;

        public event EventHandler ClientDisconnected;

        public bool Disposed { get; set; }

        public async Task RunAsync()
        {
            if (_channel == null && _channelFactory is ServerChannelFactory factory)
                _channel = factory.Create(
                    OnClientTimedout,
                    OnClientPinged,
                    OnClientSubscribing,
                    OnNewClientConnected,
                    OnClientDisconnected);

            if (_channel == null || _channel.Active)
                return;

            await _channel.RunAsync();
            OnConnected();
        }

        public async Task CloseAsync()
        {
            if (_channel == null || !_channel.Active)
                return;

            await _channel.CloseAsync();
            _channel = null;
            OnClosed();
        }

        public async void Dispose()
        {
            if (Disposed)
                return;

            await CloseAsync();
            Disposed = true;
        }

        internal async void OnClientTimedout(IChannelHandlerContext ctx, ReadIdleStateEvent e)
        {
            _logger.LogTrace("raise `Timedout` event");
            ThreadPool.QueueUserWorkItem(s => { ClientTimedout?.Invoke(this, e); });

            if (e.MaxRetriesExceeded)
                await CloseAsync();
        }

        internal async void OnClientPinged(IChannelHandlerContext ctx, Ping e)
        {
            _logger.LogTrace("raise `ping` event");
            ThreadPool.QueueUserWorkItem(s => { ClientPinged?.Invoke(this, e); });
            await ctx.WriteAndFlushAsync(Pong.New(e));
        }

        internal void OnClientSubscribing(IChannelHandlerContext ctx, Subscribe e)
        {
            _logger.LogTrace("raise `client subscribing` event");
            ThreadPool.QueueUserWorkItem(s => { ClientSubscribing?.Invoke(this, e); });
        }

        internal void OnClientSubscribed(IChannelHandlerContext ctx, Subscribe e)
        {
            _logger.LogTrace("raise `client subscribed` event");
            ThreadPool.QueueUserWorkItem(s => { ClientSubscribed?.Invoke(this, e); });
        }

        internal void OnNewClientConnected(IChannelHandlerContext ctx)
        {
            _logger.LogTrace("raise `new client connected` event");
            ThreadPool.QueueUserWorkItem(s => { NewClientConnected?.Invoke(this, EventArgs.Empty); });
        }

        internal void OnClientDisconnected(IChannelHandlerContext ctx)
        {
            _logger.LogTrace("raise `client disconnected` event");
            ThreadPool.QueueUserWorkItem(s => { ClientDisconnected?.Invoke(this, EventArgs.Empty); });
        }

        internal void OnConnected()
        {
            _logger.LogTrace("raise `Connected` event");
            ThreadPool.QueueUserWorkItem(s => { Connected?.Invoke(this, EventArgs.Empty); });
        }

        internal void OnClosed()
        {
            _logger.LogTrace("raise `Closed` event");
            ThreadPool.QueueUserWorkItem(s => { Closed?.Invoke(this, EventArgs.Empty); });
        }
    }
}
