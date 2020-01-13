using System;

namespace Netty.Examples.Common
{
    public class PacketEventArgs<T> : EventArgs
        where T : Packet
    {
        public PacketEventArgs(T packet) => Packet = packet;

        public T Packet { get; }
    }
}
