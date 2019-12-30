﻿using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public class Client : ISession
  {
    private readonly ILogger<Client> _logger;
    private readonly IChannelClientFactory _channelFactory;

    private IChannelClient _channel;

    public Client(
      ILoggerFactory loggerFactory,
      IChannelClientFactory channelFactory)
    {
      _logger = loggerFactory?.CreateLogger<Client>() ?? throw new ArgumentNullException(nameof(loggerFactory));
      _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
    }

    public event EventHandler Connected;

    public event EventHandler Closed;

    public event EventHandler<Pong> Ponged;

    public event EventHandler<ReadIdleStateEvent> Timedout;

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

    public void OnPonged(Pong packet)
    {
      _logger.LogTrace("raise `Ponged` event");
      ThreadPool.QueueUserWorkItem(s => { Ponged?.Invoke(this, packet); });
    }

    public async Task RunAsync()
    {
      if (_channel == null)
        _channel = _channelFactory?.Create((o, e) => OnTimedout(e), (o, e) => OnPonged(e));

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
      OnClosed();
    }
  }
}