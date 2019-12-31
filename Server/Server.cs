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

        private IChannelWrapper _channel;

        public Server(
          ILoggerFactory loggerFactory,
          IChannelFactory channelFactory)
        {
            _logger = loggerFactory?.CreateLogger<Server>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        }

        public event EventHandler Connected;

        public event EventHandler Closed;

        public event EventHandler<ReadIdleStateEvent> Timedout;

        public event EventHandler<Ping> Pinged;

        public bool Disposed { get; set; }

        public void OnTimedout(ReadIdleStateEvent ev)
        {
            _logger.LogTrace("raise `Timedout` event");
            ThreadPool.QueueUserWorkItem(s => { Timedout?.Invoke(this, ev); });
        }

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

        public async Task RunAsync()
        {
            if (_channel == null)
                _channel = _channelFactory?.Create();

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
    }
}
