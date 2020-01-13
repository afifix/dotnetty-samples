using System;

namespace Netty.Examples.Common
{
    public enum PacketType
    {
        PINGREQ,
        PINGRESP,
        SUBREQ,
        SUBACK,
    }

    public abstract class Packet
    {
        public Guid Id { get; set; }

        public PacketType Type { get; set; }
    }
}
