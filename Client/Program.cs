using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Netty.Examples.Common;

namespace Netty.Examples.Client
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
                var client = scopeServiceProvider.GetRequiredService<IClientSession>();

                try
                {
                    client.Connected += (o, e) => logger.LogInformation("client connected.");
                    client.Closed += (o, e) => logger.LogInformation("client closed.");
                    client.Ponged += (o, e) => logger.LogInformation($"pong received ${e.Packet.Latency}.");
                    client.Subacked += (o, e) => {
                        var p = e.Packet;
                        var message = $"subscription to subject {p.SubjectId} ${(p.Result == 0 ? "not" : string.Empty)} accepted - {p.Message}.";
                        logger.LogInformation(message);
                    };
                    client.Timedout += (o, e) => logger.LogInformation($"server not responding {e.Retries}");
                    await client.RunAsync();
                    await client.SubscribeAsync(2);
                    await client.SubscribeAsync(1);

                    _ = Console.ReadKey();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error");
                }
                finally
                {
                    await client.CloseAsync();
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
                .AddScoped<Client>()
                .AddScoped<IClientSession>(sp => sp.GetRequiredService<Client>())
                .AddScoped<ISessionOptionProvider>(sp => sp.GetRequiredService<DefaultSessionOptionProvider>())
                .AddTransient<ClientChannel>()
                .AddScoped<Func<IChannelWrapper>>(sp => sp.GetRequiredService<ClientChannel>)
                .AddScoped<ClientChannelFactory>()
                .AddScoped<IChannelFactory>(sp => sp.GetRequiredService<ClientChannelFactory>())
                .BuildServiceProvider();
        }

        private static void Main() => RunAsync().Wait();
    }
}
