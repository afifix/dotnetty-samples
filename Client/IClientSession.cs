using System;
using System.Threading.Tasks;

using Netty.Examples.Common;

namespace Netty.Examples.Client
{
    public interface IClientSession : ISession
    {
        event EventHandler<ReadIdleStateEventArgs> Timedout;

        event EventHandler<PacketEventArgs<Pong>> Ponged;

        event EventHandler<PacketEventArgs<Suback>> Subacked;

        Task SubscribeAsync(int id);
    }
}
