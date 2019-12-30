using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  public interface IChannelClient
  {
    bool Active { get; }

    Task RunAsync();

    Task CloseAsync();
  }
}