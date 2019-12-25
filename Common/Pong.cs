using DotNetty.Common.Utilities;
using System;

namespace Netty.Examples.Common
{
  public class Pong : Packet
  {
    // use always this method to create new packet
    // the default constructor is only used for json deserialization
    public static Pong New(Ping ping)
    {
      var time = TimeUtil.GetSystemTime();
      return new Pong
      {
        Id = Guid.NewGuid(),
        RequestId = ping.Id,
        Type = PacketType.PINGRESP,
        Time = time,
        Latency = (time - ping.Time).Milliseconds
      };
    }

    public Guid RequestId { get; set; }

    public int Latency { get; set; }

    public TimeSpan Time { get; set; }
  }
}