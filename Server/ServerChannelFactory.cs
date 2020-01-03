using DotNetty.Transport.Channels;
using Netty.Examples.Common;
using System;

namespace Netty.Examples.Server
{
    public class ServerChannelFactory : IChannelFactory
    {
        private readonly Func<IChannelWrapper> _factory;

        public ServerChannelFactory(Func<IChannelWrapper> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public IChannelWrapper Create(
            Action<IChannelHandlerContext, ReadIdleStateEvent> clientTimedoutCallback,
            Action<IChannelHandlerContext, Ping> clientPingCallback,
            Action<IChannelHandlerContext, Subscribe> clientSubscribingCallback,
            Action<IChannelHandlerContext> newClientConnectedCallback,
            Action<IChannelHandlerContext> clientDisconnectedCallback)
        {
            var c = _factory();
            if (!(c is ServerChannel channel))
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
                Helper.Nop<IChannelHandlerContext, ReadIdleStateEvent>(),
                Helper.Nop<IChannelHandlerContext, Ping>(),
                Helper.Nop<IChannelHandlerContext, Subscribe>(),
                Helper.Nop<IChannelHandlerContext>(),
                Helper.Nop<IChannelHandlerContext>());
        }
    }
}
