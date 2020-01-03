using Netty.Examples.Common;
using System;

namespace Netty.Examples.Server
{
    public interface IServerSession : ISession
    {
        event EventHandler<ReadIdleStateEvent> ClientTimedout;

        event EventHandler<Ping> ClientPinged;

        event EventHandler<Subscribe> ClientSubscribing;

        event EventHandler<Subscribe> ClientSubscribed;

        event EventHandler NewClientConnected;

        event EventHandler ClientDisconnected;
    }
}
