using System;
using System.Collections.Generic;
using System.IO;

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

using Netty.Examples.Common.Packets;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Netty.Examples.Common
{
    public class PacketDecoder : ByteToMessageDecoder
    {
        private static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<PacketDecoder>();

        protected override void Decode(IChannelHandlerContext ctx, IByteBuffer input, List<object> output)
        {
            if(input.ReadableBytes < 5)
                return;

            _ = input.MarkReaderIndex();

            // read the magic number
            var magicNumber = input.ReadByte();
            if(magicNumber != Convert.ToChar(0x02))
            {
                _ = input.ResetReaderIndex();
                throw new Exception("Invalid magic number: " + magicNumber);
            }

            // read data length
            var dataLength = input.ReadInt();
            if(input.ReadableBytes < dataLength)
            {
                _ = input.ResetReaderIndex();
                return;
            }

            // decode data
            var decoded = new byte[dataLength];
            _ = input.ReadBytes(decoded);
            using(var ms = new MemoryStream(decoded))
            {
                var reader = new BsonDataReader(ms);
                // deserialize data
                var serializer = JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                var response = serializer.Deserialize(reader);
                output.Add(response);
                if(response is Packet packet)
                {
                    Logger.Trace($"[{ctx.Channel.Id}] packet decoded: {packet.Type}");
                    return;
                }

                Logger.Warn($"[{ctx.Channel.Id}] decoded: ???");
            }
        }
    }
}
