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

        public IChannelWrapper Create(Action<object, ReadIdleStateEvent> timeoutCallback)
        {
            var c = _factory();
            if (!(c is ServerChannel channel))
                return c;

            channel.TimeoutCallback = timeoutCallback ?? throw new ArgumentNullException(nameof(timeoutCallback));
            return channel;
        }

        public IChannelWrapper Create()
        {
            return Create(Helper.Nop<object, ReadIdleStateEvent>());
        }
    }
}
