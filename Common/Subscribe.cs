using System;

using DotNetty.Common.Utilities;

namespace Netty.Examples.Common
{
    public class Subscribe : Packet
    {
        public static Subscribe New(int subjectId)
        {
            return new Subscribe {
                Id = Guid.NewGuid(),
                Time = TimeUtil.GetSystemTime(),
                SubjectId = subjectId
            };
        }

        public int SubjectId { get; set; }

        public TimeSpan Time { get; set; }
    }
}