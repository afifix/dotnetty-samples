﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Netty.Examples.Common;
using Netty.Examples.Common.Packets;

namespace Netty.Examples.Client
{
    public class Client : IClientSession
    {
        private readonly ILogger<Client> _logger;
        private readonly IChannelFactory _channelFactory;

        private IChannelWrapper _channel;

        public Client(
          ILoggerFactory loggerFactory,
          IChannelFactory channelFactory)
        {
            _logger = loggerFactory?.CreateLogger<Client>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        }

        public event EventHandler Connected;

        public event EventHandler Closed;

        public event EventHandler<ReadIdleStateEventArgs> Timedout;

        public event EventHandler<PacketEventArgs<Pong>> PingResponseReceived;

        public event EventHandler<PacketEventArgs<Suback>> SubackReceived;

        public async Task SubscribeAsync(int subjectId) => await _channel.WriteAsync(Subscribe.New(subjectId)).ConfigureAwait(false);

        public async Task RunAsync()
        {
            if(_channel == null && _channelFactory is ClientChannelFactory factory)
                _channel = factory.Create(OnConnected, OnClosed, OnTimedout, OnPonged, OnSubacked);

            if(_channel == null || _channel.Active)
                return;

            await _channel.RunAsync().ConfigureAwait(false);
        }

        public async Task CloseAsync()
        {
            if(_channel == null || !_channel.Active)
                return;

            await _channel.CloseAsync().ConfigureAwait(false);
        }

        internal async void OnTimedout(ReadIdleStateEventArgs ev)
        {
            _logger.LogTrace("raise `Timedout` event");
            if(ev.MaxRetriesExceeded)
                await CloseAsync().ConfigureAwait(false);

            _ = ThreadPool.QueueUserWorkItem(s => Timedout?.Invoke(this, ev));
        }

        internal void OnConnected()
        {
            _logger.LogTrace("raise `Connected` event");
            _ = ThreadPool.QueueUserWorkItem(s => Connected?.Invoke(this, EventArgs.Empty));
        }

        internal void OnClosed()
        {
            _logger.LogTrace("raise `Closed` event");
            _ = ThreadPool.QueueUserWorkItem(s => Closed?.Invoke(this, EventArgs.Empty));
        }

        internal void OnPonged(Pong packet)
        {
            _logger.LogTrace("raise `Ponged` event");
            _ = ThreadPool.QueueUserWorkItem(s => PingResponseReceived?.Invoke(this, new PacketEventArgs<Pong>(packet)));
        }

        internal void OnSubacked(Suback packet)
        {
            _logger.LogTrace("raise `Subacked` event");
            _ = ThreadPool.QueueUserWorkItem(s => SubackReceived?.Invoke(this, new PacketEventArgs<Suback>(packet)));
        }
    }
}
