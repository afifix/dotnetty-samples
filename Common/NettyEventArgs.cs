using System;

using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
    [Serializable]
    public delegate void NettyEventHandler(object sender, NettyEventArgs e);

    [Serializable]
    public delegate void NettyEventHandler<T>(object sender, T e)
        where T : NettyEventArgs;

    public class NettyEventArgs : EventArgs
    {
        public NettyEventArgs(IChannelHandlerContext context) => Context = context ?? throw new ArgumentNullException(nameof(context));

        public IChannelHandlerContext Context { get; }

        public object Message { get; }
    }
}
