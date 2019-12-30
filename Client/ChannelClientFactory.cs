using Netty.Examples.Common;
using System;

namespace Netty.Examples.Client
{
  public class ChannelClientFactory : IChannelClientFactory
  {
    private readonly Func<IChannelClient> _factory;

    public ChannelClientFactory(Func<IChannelClient> factory)
    {
      _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IChannelClient Create(
      Action<object, ReadIdleStateEvent> timeoutCallback = null,
      Action<object, Pong> pongCallback = null)
    {
      var channel = _factory();
      if (!(channel is ChannelClient channelClient))
        return channel;

      channelClient.PongCallback = pongCallback;
      channelClient.TimeoutCallback = timeoutCallback;
      return channel;
    }
  }
}