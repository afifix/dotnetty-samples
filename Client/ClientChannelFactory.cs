using System;

using Netty.Examples.Common;
using Netty.Examples.Common.Packets;

namespace Netty.Examples.Client
{
    public class ClientChannelFactory : IChannelFactory
    {
        private readonly Func<IChannelWrapper> _factory;

        public ClientChannelFactory(Func<IChannelWrapper> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IChannelWrapper Create(
          Action connectedCallback,
          Action closedCallback,
          Action<ReadIdleStateEventArgs> timeoutCallback,
          Action<Pong> pongCallback,
          Action<Suback> subackCallback)
        {
            var c = _factory();
            if(!(c is ClientChannel channel))
                return c;

            channel.ConnectedCallback = connectedCallback ?? throw new ArgumentNullException(nameof(connectedCallback));
            channel.ClosedCallback = closedCallback ?? throw new ArgumentNullException(nameof(closedCallback));
            channel.PingResponseReceivedCallback = pongCallback ?? throw new ArgumentNullException(nameof(pongCallback));
            channel.TimedoutCallback = timeoutCallback ?? throw new ArgumentNullException(nameof(timeoutCallback));
            channel.SubackReceivedCallback = subackCallback ?? throw new ArgumentNullException(nameof(subackCallback));
            return c;
        }

        public IChannelWrapper Create()
        {
            return Create(
              Helper.Nop(),
              Helper.Nop(),
              Helper.Nop<ReadIdleStateEventArgs>(),
              Helper.Nop<Pong>(),
              Helper.Nop<Suback>());
        }
    }
}
