using System;

namespace Netty.Examples.Common.Packets
{
    public abstract class Packet
    {
        public Guid Id { get; set; }

        public PacketType Type { get; set; }
    }
}
