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

    public TimeSpan Time { get; set; }
  }
}