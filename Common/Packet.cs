using System;

namespace Netty.Examples.Common
{
    public enum PacketType
    {
        PINGREQ,
        PINGRESP,
        LICREQ,
        LICRESP,
    }

    public abstract class Packet
    {
        public Guid Id { get; set; }

        public PacketType Type { get; set; }
    }
}