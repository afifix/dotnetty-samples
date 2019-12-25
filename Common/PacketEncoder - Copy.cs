using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;

namespace Netty.Examples.Common
{
  public class PacketEncoder : MessageToMessageEncoder<Packet>
  {
    protected override void Encode(IChannelHandlerContext context, Packet packet, List<object> output)
    {
      using (var ms = new MemoryStream())
      using (var bsonWriter = new BsonDataWriter(ms))
      {
        var serializer = new JsonSerializer();
        serializer.Serialize(bsonWriter, packet);     // serialize object into binary JSON
        var data = ms.ToArray();
        var buffer = context.Allocator.Buffer();      // allocate buffer
        buffer.WriteByte((byte)Convert.ToChar(0x02)); // write magic number
        buffer.WriteInt(data.Length);                 // write data length
        buffer.WriteBytes(data);                      // write data
        output.Add(buffer);                           // add buffer to output
      }
    }
  }
}