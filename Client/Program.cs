using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Netty.Examples.Common;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Netty.Examples.Client
{
  internal class Program
  {
    private static readonly IPAddress Host;
    private static readonly int Port;

    static Program()
    {
      Host = IPAddress.Parse("127.0.0.1");
      Port = 8007;
    }

    private static async Task RunClientAsync()
    {
      Helper.SetConsoleLogger();

      var group = new MultithreadEventLoopGroup();
      var logger = new LoggingHandler("CLI", LogLevel.TRACE);
      try
      {
        var bootstrap = new Bootstrap();
        bootstrap
          .Group(group)
          .Channel<TcpSocketChannel>()
          .Option(ChannelOption.TcpNodelay, true)
          //.Handler(new LoggingHandler("CLI-LSTN"))
          .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
          {
            var pipeline = channel.Pipeline;

            //pipeline.AddLast(new LoggingHandler("CLI", LogLevel.TRACE));
            pipeline.AddLast("encoder", new PacketEncoder());
            pipeline.AddLast("decoder", new PacketDecoder());
            pipeline.AddLast("keep-alive", new KeepMeAliveChannel());
            pipeline.AddLast("idle", new ReadIdleStateHandler(2, 9999));
            pipeline.AddLast("client", new ClientChannelHandler());
            pipeline.AddLast("ping", new PingProcessor());
            pipeline.AddLast("pong", new PongProcessor());
            pipeline.AddLast("timeout", new TimeoutHandler());
          }));

        var clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(Host, Port));

        Console.ReadLine();

        await clientChannel.CloseAsync();
      }
      finally
      {
        await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
      }
    }

    private static void Main() => RunClientAsync().Wait();
  }
}