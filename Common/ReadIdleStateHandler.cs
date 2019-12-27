﻿using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class ReadIdleStateHandler : IdleStateHandler
  {
    private const string Tag = "IDLE";

    private static readonly IInternalLogger Logger;

    private int _retriesCounter;
    private readonly int _maxNumberOfRetries;

    static ReadIdleStateHandler()
    {
      Logger = InternalLoggerFactory.GetInstance<ReadIdleStateHandler>();
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="readIdleTimeout">read idle timeout</param>
    /// <param name="maxNumberRetries">maximum number of retries</param>
    public ReadIdleStateHandler(int readIdleTimeout, int maxNumberRetries)
        : base(readIdleTimeout, 0, 0)
    {
      _maxNumberOfRetries = maxNumberRetries;
    }

    protected override void ChannelIdle(IChannelHandlerContext ctx, IdleStateEvent stateEvent)
    {
      Logger.Trace($"[{Tag} - {ctx.Channel.Id}] channel idle: {stateEvent.State}");

      // fire only read idle event
      if (stateEvent.State != IdleState.ReaderIdle)
        return;

      ctx.FireUserEventTriggered(new ReadIdleStateEvent(_retriesCounter, ++_retriesCounter >= _maxNumberOfRetries));
    }

    public override void ChannelReadComplete(IChannelHandlerContext ctx)
    {
      Logger.Trace($"[{Tag} - {ctx.Channel.Id}] channel read complete");
      // reset retries counter
      _retriesCounter = 0;

      base.ChannelReadComplete(ctx);
    }
  }
}