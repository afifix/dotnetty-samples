using DotNetty.Transport.Channels.Groups;
using System.Threading.Tasks;

namespace Netty.Examples.Common
{
  public interface IChannelWrapper
  {
    bool Active { get; }

    Task RunAsync();

    Task CloseAsync();

    Task WriteAsync(Packet packet);

    IChannelGroup ChannelGroup { get; }

    IChannelGroup NewChannelGroup();
  }
}