using Netty.Examples.Common;
using System;

namespace Netty.Examples.Client
{
  public class ClientChannelFactory : IChannelFactory
  {
    private readonly Func<IChannelWrapper> _factory;

    public ClientChannelFactory(Func<IChannelWrapper> factory)
    {
      _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IChannelWrapper Create(
      Action<object, ReadIdleStateEvent> timeoutCallback,
      Action<object, Pong> pongCallback)
    {
      var channel = _factory();
      if (!(channel is ClientChannel channelClient))
        return channel;

      channelClient.PongCallback = pongCallback ?? throw new ArgumentNullException(nameof(pongCallback));
      channelClient.TimeoutCallback = timeoutCallback ?? throw new ArgumentNullException(nameof(timeoutCallback));
      return channel;
    }

    public IChannelWrapper Create()
    {
      return Create(Helper.Nop<object, ReadIdleStateEvent>(), Helper.Nop<object, Pong>());
    }
  }
}