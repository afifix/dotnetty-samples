using Netty.Examples.Common;
using System;

namespace Netty.Examples.Server
{
    public interface IServerSession : ISession
    {
        event EventHandler<Ping> Pinged;
    }
}
