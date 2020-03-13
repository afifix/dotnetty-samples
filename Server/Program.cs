using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Netty.Examples.Common;

namespace Netty.Examples.Server
{
    internal class Program
    {
        private static async Task RunAsync()
        {
            using(var serviceProvider = Bootstrap())
            using(var scope = serviceProvider.CreateScope())
            {
                var scopeServiceProvider = scope.ServiceProvider;
                var logger = scopeServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
                var server = scopeServiceProvider.GetRequiredService<IServerSession>();

                try
                {
                    server.Connected += (o, e) => logger.LogInformation("server started.");
                    server.Closed += (o, e) => logger.LogInformation("server closed.");
                    server.NewClientConnected += (o, e) => logger.LogInformation("new client connected");
                    server.ClientDisconnected += (o, e) => logger.LogInformation("client disconnected");
                    server.ClientTimedout += (o, e) => logger.LogInformation("client timedout");
                    server.ClientSubscribed += (o, e) => logger.LogInformation("client subscribed");
                    await server.RunAsync();

                    _ = Console.ReadKey();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error");
                }
                finally
                {
                    await server.CloseAsync();
                }
            }

            _ = Console.ReadKey();
        }

        public static ServiceProvider Bootstrap()
        {
            var loggerFactory = Helper.ConfigureLogger();

            // initialize DI container
            return new ServiceCollection()
               .AddSingleton(loggerFactory)
               .AddScoped<DefaultSessionOptionProvider>()
               .AddScoped<FakeSubjectProvider>()
               .AddScoped<ISubjectProvider>(sp => sp.GetRequiredService<FakeSubjectProvider>())
               .AddScoped<Server>()
               .AddScoped<IServerSession>(sp => sp.GetRequiredService<Server>())
               .AddScoped<ISessionOptionProvider>(sp => sp.GetRequiredService<DefaultSessionOptionProvider>())
               .AddTransient<ServerChannel>()
               .AddScoped<Func<IChannelWrapper>>(sp => sp.GetRequiredService<ServerChannel>)
               .AddScoped<ServerChannelFactory>()
               .AddScoped<IChannelFactory>(sp => sp.GetRequiredService<ServerChannelFactory>())
               .BuildServiceProvider();
        }

        private static void Main()
        {
            Console.Title = "Server";
            RunAsync().Wait();
        }

    }
}
