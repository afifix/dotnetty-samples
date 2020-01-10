using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public interface IClientSession : ISession
  {
    event EventHandler<Pong> Ponged;

    event EventHandler<ReadIdleStateEvent> Timedout;

    event EventHandler<Suback> Subacked;

    Task SubscribeAsync(int subjectId);
  }
}