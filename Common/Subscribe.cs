using DotNetty.Common.Utilities;
using System;

namespace Netty.Examples.Common
{
  public class Subscribe : Packet
  {
    public static Subscribe New()
    {
      return new Subscribe
      {
        Id = Guid.NewGuid(),
        Time = TimeUtil.GetSystemTime()
      };
    }

    public int SubjectId { get; set; }

    public TimeSpan Time { get; set; }
  }
}