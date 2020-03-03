using System;

using Netty.Examples.Common;
using Netty.Examples.Common.Packets;

namespace Netty.Examples.Server
{
    public interface IServerSession : ISession
    {
        event EventHandler<ReadIdleStateEventArgs> ClientTimedout;

        event EventHandler<PacketEventArgs<Ping>> ClientPinged;

        event EventHandler<PacketEventArgs<Subscribe>> ClientSubscribed;

        event EventHandler NewClientConnected;

        event EventHandler ClientDisconnected;
    }
}
