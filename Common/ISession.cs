using System;
using System.Threading.Tasks;

namespace Netty.Examples.Common
{
    public interface ISession
    {
        event EventHandler Connected;

        event EventHandler Closed;

        Task RunAsync();

        Task CloseAsync();
    }
}