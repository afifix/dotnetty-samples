using System;

using DotNetty.Common.Utilities;

namespace Netty.Examples.Common.Packets
{
    public class Suback : Packet
    {
        public static Suback New(Subscribe packet, short result, string message = null)
        {
            return new Suback {
                Id = Guid.NewGuid(),
                Type = PacketType.SUBACK,
                RequestId = packet.Id,
                SubjectId = packet.SubjectId,
                Time = TimeUtil.GetSystemTime(),
                Result = result,
                Message = message ?? string.Empty
            };
        }

        public Guid RequestId { get; set; }

        public int SubjectId { get; set; }

        public TimeSpan Time { get; set; }

        public short Result { get; set; }

        public string Message { get; set; }
    }
}
