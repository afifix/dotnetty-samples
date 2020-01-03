using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Netty.Examples.Common
{
  public class Subject
  {
    public int Id { get; set; }

    public string Code { get; set; }

    public int Max { get; set; }
  }

  public interface ISubjectProvider
  {
    IList<Subject> Get();
  }

  public class FakeSubjectProvider : ISubjectProvider
  {
    public IList<Subject> Get()
    {
      return new List<Subject>
      {
        new Subject
        {
          Id = 1,
          Code = "APP",
          Max = 3
        }
      };
    }
  }

  public class SubscribeProcessor : SimpleChannelInboundHandler<Subscribe>
  {
    private static readonly IInternalLogger Logger;

    public override bool IsSharable => true;

    public event EventHandler<Subscribe> Reading;

    static SubscribeProcessor()
    {
      Logger = InternalLoggerFactory.GetInstance<PongProcessor>();
    }

    protected override void ChannelRead0(IChannelHandlerContext ctx, Subscribe packet)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read0");
      OnReading(ctx, packet);
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{ctx.Channel.Id}] channel read complete");

      base.ChannelReadComplete(ctx);
    }

    private void OnReading(IChannelHandlerContext ctx, Subscribe packet)
    {
      Logger.Trace("raise `Reading` event");
      ThreadPool.QueueUserWorkItem(s => { Reading?.Invoke(ctx, packet); });
    }
  }
}