using System;

using DotNetty.Common.Utilities;

namespace Netty.Examples.Common
{
    public class Ping : Packet
    {
        // use always this method to create new packet
        // the default constructor is only used for json deserialization
        public static Ping New()
        {
            return new Ping {
                Id = Guid.NewGuid(),
                Type = PacketType.PINGREQ,
                Time = TimeUtil.GetSystemTime()
            };
        }

        public TimeSpan Time { get; set; }
    }
}