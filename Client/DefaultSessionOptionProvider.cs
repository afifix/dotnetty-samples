using System.Net;

namespace Netty.Examples.Client
{
  public class DefaultSessionOptionProvider : ISessionOptionProvider
  {
    public SessionOption Get()
    {
      return new SessionOption
      {
        Host = IPAddress.Parse("127.0.0.1"),
        Port = 8007,
        IdleTimeout = 10,
        KeepAliveInterval = 5,
        KeepAliveRetries = 3
      };
    }
  }
}