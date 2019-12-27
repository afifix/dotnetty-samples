using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples
{
  public interface ISession
  {
    event EventHandler Connected;

    event EventHandler Closed;

    event EventHandler<Pong> Ponged;

    Task RunAsync();

    Task CloseAsync();
  }
}