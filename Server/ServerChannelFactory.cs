using System;

using Netty.Examples.Common;

namespace Netty.Examples.Server
{
    public class ServerChannelFactory : IChannelFactory
    {
        private readonly Func<IChannelWrapper> _factory;

        public ServerChannelFactory(Func<IChannelWrapper> factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        public IChannelWrapper Create(
            Action<ReadIdleStateEventArgs> clientTimedoutCallback,
            Action<NettyPacketEventArgs<Ping>> clientPingCallback,
            Action<NettyPacketEventArgs<Subscribe>> clientSubscribingCallback,
            Action<NettyEventArgs> newClientConnectedCallback,
            Action<NettyEventArgs> clientDisconnectedCallback)
        {
            var c = _factory();
            if(!(c is ServerChannel channel))
                return c;

            channel.ClientTimedoutCallback = clientTimedoutCallback ?? throw new ArgumentNullException(nameof(clientTimedoutCallback));
            channel.ClientPingCallback = clientPingCallback ?? throw new ArgumentNullException(nameof(clientPingCallback));
            channel.ClientSubscribeCallback = clientSubscribingCallback ?? throw new ArgumentNullException(nameof(clientSubscribingCallback));
            channel.NewClientConnectedCallback = newClientConnectedCallback ?? throw new ArgumentNullException(nameof(newClientConnectedCallback));
            channel.ClientDisconnectedCallback = clientDisconnectedCallback ?? throw new ArgumentNullException(nameof(clientDisconnectedCallback));
            return channel;
        }

        public IChannelWrapper Create()
        {
            return Create(
                Helper.Nop<ReadIdleStateEventArgs>(),
                Helper.Nop<NettyPacketEventArgs<Ping>>(),
                Helper.Nop<NettyPacketEventArgs<Subscribe>>(),
                Helper.Nop<NettyEventArgs>(),
                Helper.Nop<NettyEventArgs>());
        }
    }
}
