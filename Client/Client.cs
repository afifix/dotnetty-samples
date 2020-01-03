using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

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

    public event EventHandler<Pong> Ponged;

    public event EventHandler<ReadIdleStateEvent> Timedout;

    public async Task RunAsync()
    {
      if (_channel == null && _channelFactory is ClientChannelFactory factory)
        _channel = factory.Create(OnConnected, OnClosed, OnTimedout, OnPonged);

      if (_channel == null || _channel.Active)
        return;

      await _channel.RunAsync();
    }

    public async Task CloseAsync()
    {
      if (_channel == null || !_channel.Active)
        return;

      await _channel.CloseAsync();
    }

    internal async void OnTimedout(ReadIdleStateEvent ev)
    {
      _logger.LogTrace("raise `Timedout` event");
      if (ev.MaxRetriesExceeded)
        await CloseAsync();

      ThreadPool.QueueUserWorkItem(s => { Timedout?.Invoke(this, ev); });
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

    internal void OnPonged(Pong packet)
    {
      _logger.LogTrace("raise `Ponged` event");
      ThreadPool.QueueUserWorkItem(s => { Ponged?.Invoke(this, packet); });
    }
  }
}