using System.Net;

namespace Netty.Examples.Client
{
  public class SessionOption
  {
    public IPAddress Host { get; set; }
    public int Port { get; set; }
    public int KeepAliveInterval { get; set; }
    public int KeepAliveRetries { get; set; }
    public int IdleTimeout { get; set; }
  }
}