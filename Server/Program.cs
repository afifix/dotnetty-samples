using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Server
{
    internal class Program
    {
        private static async Task RunAsync()
        {
            const int port = 8007;

            Helper.SetConsoleLogger();

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                  .Option(ChannelOption.SoBacklog, 100)
                  //.Handler(new LoggingHandler("SRV-LSTN"))
                  .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                  {
                      channel.Pipeline
                          .AddLast(new LoggingHandler("SRV", LogLevel.TRACE))
                          .AddLast("encoder", new PacketEncoder())
                          .AddLast("decoder", new PacketDecoder())
                          .AddLast("idle", new ReadIdleStateHandler(5, 9999))
                          .AddLast("server", new ServerChannelHandler())
                          .AddLast("timeout", new TimeoutConnectionHandler());
                  }));

                var boundChannel = await bootstrap.BindAsync(port);

                Console.ReadLine();

                await boundChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                  bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                  workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        private static void Main() => RunAsync().Wait();
    }
}
