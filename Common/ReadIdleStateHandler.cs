using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Netty.Examples.Common
{
  public class ReadIdleStateHandler : IdleStateHandler
  {
    private int _retriesCounter;
    private readonly int _maxNumberOfRetries;

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

    protected override void ChannelIdle(IChannelHandlerContext context, IdleStateEvent stateEvent)
    {
      // fire only read idle event
      if (stateEvent.State != IdleState.ReaderIdle)
        return;

      context.FireUserEventTriggered(new ReadIdleStateEvent(_retriesCounter, ++_retriesCounter >= _maxNumberOfRetries));
    }

    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
      // reset retries counter
      _retriesCounter = 0;

      base.ChannelReadComplete(context);
    }
  }
}