using System;
using System.Threading.Tasks;

using Netty.Examples.Common;
using Netty.Examples.Common.Packets;

namespace Netty.Examples.Client
{
    public interface IClientSession : ISession
    {
        event EventHandler<ReadIdleStateEventArgs> Timedout;

        event EventHandler<PacketEventArgs<Pong>> PingResponseReceived;

        event EventHandler<PacketEventArgs<Suback>> SubackReceived;

        Task SubscribeAsync(int id);
    }
}
