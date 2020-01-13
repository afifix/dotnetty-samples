namespace Netty.Examples.Common
{
    public interface IChannelFactory
    {
        IChannelWrapper Create();
    }
}