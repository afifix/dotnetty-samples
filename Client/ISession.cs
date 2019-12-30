using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public interface ISession
  {
    event EventHandler Connected;

    event EventHandler Closed;

    event EventHandler<Pong> Ponged;

    event EventHandler<ReadIdleStateEvent> Timedout;

    Task RunAsync();

    Task CloseAsync();
  }
}