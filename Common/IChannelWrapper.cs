using System.Threading.Tasks;

namespace Netty.Examples.Common
{
  public interface IChannelWrapper
  {
    bool Active { get; }

    Task RunAsync();

    Task CloseAsync();
  }
}