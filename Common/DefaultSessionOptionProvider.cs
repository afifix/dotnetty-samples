using System.Net;

namespace Netty.Examples.Common
{
    public class DefaultSessionOptionProvider : ISessionOptionProvider
    {
        public SessionOption Get()
        {
            return new SessionOption {
                Host = IPAddress.Parse("127.0.0.1"),
                Port = 8007,
                IdleTimeout = 4,
                KeepAliveInterval = 2,
                KeepAliveRetries = 3
            };
        }
    }
}