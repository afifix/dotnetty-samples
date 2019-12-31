using DotNetty.Common.Utilities;
using System;

namespace Netty.Examples.Common
{
  public class Suback : Packet
  {
    public static Suback New(Subscribe packet)
    {
      return new Suback
      {
        Id = Guid.NewGuid(),
        RequestId = packet.Id,
        SubjectId = packet.SubjectId,
        Time = TimeUtil.GetSystemTime()
      };
    }

    public Guid RequestId { get; set; }

    public int SubjectId { get; set; }

    public TimeSpan Time { get; set; }

    public short Result { get; set; }

    public string Message { get; set; }
  }
}