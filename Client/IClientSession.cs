using Netty.Examples.Common;
using System;

namespace Netty.Examples.Client
{
  public interface IClientSession : ISession
  {
    event EventHandler<Pong> Ponged;

    event EventHandler<ReadIdleStateEvent> Timedout;
  }
}