using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;

namespace Netty.Examples.Server
{
    public class ServerChannelHandler : ChannelHandlerAdapter
    {
        private static readonly IInternalLogger Logger;

        static ServerChannelHandler()
        {
            Logger = InternalLoggerFactory.GetInstance<ServerChannelHandler>();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            if (exception is ReadTimeoutException)
                Logger.Error("channel timeout exception: " + exception.Message);
            else
                Logger.Error("channel exception: " + exception);
            context.CloseAsync();
        }
    }
}
