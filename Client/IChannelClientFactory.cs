using Netty.Examples.Common;
using System;

namespace Netty.Examples.Client
{
  public interface IChannelClientFactory
  {
    IChannelClient Create(
      Action<object, ReadIdleStateEvent> timeoutCallback = null,
      Action<object, Pong> pongCallback = null);
  }
}