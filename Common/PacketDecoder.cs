using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;

namespace Netty.Examples.Common
{
  public class PacketDecoder : ByteToMessageDecoder
  {
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
      if (input.ReadableBytes < 5)
      {
        return;
      }

      input.MarkReaderIndex();

      // read the magic number
      var magicNumber = input.ReadByte();
      if (magicNumber != Convert.ToChar(0x02))
      {
        input.ResetReaderIndex();
        throw new Exception("Invalid magic number: " + magicNumber);
      }

      // read data length
      var dataLength = input.ReadInt();
      if (input.ReadableBytes < dataLength)
      {
        input.ResetReaderIndex();
        return;
      }

      // decode data
      var decoded = new byte[dataLength];
      input.ReadBytes(decoded);
      using (var ms = new MemoryStream(decoded))
      using (var reader = new BsonDataReader(ms))
      {
        // deserialize data
        var serializer = new JsonSerializer();
        var response = serializer.Deserialize<Packet>(reader);
        output.Add(response);
      }
    }
  }
}