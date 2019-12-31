using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
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
    private static readonly IInternalLogger Logger;

    static PacketEncoder()
    {
      Logger = InternalLoggerFactory.GetInstance<PacketEncoder>();
    }

    public override bool IsSharable => true;

    protected override void Encode(IChannelHandlerContext ctx, Packet packet, List<object> output)
    {
      using (var ms = new MemoryStream())
      using (var bsonWriter = new BsonDataWriter(ms))
      {
        // serialize object to binary JSON
        var settings = new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.All
        };
        var serializer = JsonSerializer.Create(settings);
        serializer.Serialize(bsonWriter, packet);
        var data = ms.ToArray();

        var buffer = ctx.Allocator.Buffer(); // allocate buffer
        buffer.WriteByte((byte)Convert.ToChar(0x02)); // write magic number
        buffer.WriteInt(data.Length);                 // write data length
        buffer.WriteBytes(data);                      // write data
        output.Add(buffer);                           // add buffer to output
        Logger.Trace($"[{ctx.Channel.Id}] packet encode {packet.Type}");
      }
    }
  }
}