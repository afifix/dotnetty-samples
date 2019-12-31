using System;
using System.Threading.Tasks;

namespace Netty.Examples.Common
{
  public interface ISession
  {
    event EventHandler Connected;

    event EventHandler Closed;

    event EventHandler<ReadIdleStateEvent> Timedout;

    Task RunAsync();

    Task CloseAsync();
  }
}