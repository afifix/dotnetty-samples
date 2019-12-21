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

  public class Packet
  {
    public Packet()
    {
    }

    public static Packet NewPacket(PacketType type, Guid? id = null)
    {
      return new Packet
      {
        Id = id ?? Guid.NewGuid(),
        Type = type
      };
    }

    public Guid Id { get; set; }

    public PacketType Type { get; set; }
  }
}