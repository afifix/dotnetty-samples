using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
    public class ReadIdleStateEventArgs : NettyEventArgs
    {
        /// <summary>
        /// consturctor
        /// </summary>
        /// <param name="retries">number of retries</param>
        /// <param name="maxExceeded">will be true if max retries exceeded</param>
        public ReadIdleStateEventArgs(IChannelHandlerContext context, int retries, bool maxExceeded)
            : base(context)
        {
            Retries = retries;
            MaxRetriesExceeded = maxExceeded;
        }

        /// <summary>
        /// Returns the number of retries
        /// </summary>
        public int Retries { get; }

        /// <summary>
        /// Returns <code>true</code> if this max retries exceeded
        /// </summary>
        public bool MaxRetriesExceeded { get; }
    }
}
