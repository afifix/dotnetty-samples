using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Server
{
    internal class Program
    {
        private static async Task RunAsync()
        {
            using (var serviceProvider = Bootstrap())
            using (var scope = serviceProvider.CreateScope())
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

                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error");
                }
                finally
                {
                    await server.CloseAsync();
                }
            }

            Console.ReadKey();
        }

        public static ServiceProvider Bootstrap()
        {
            var loggerFactory = Helper.ConfigureLogger();

            // initialize DI container
            var services = new ServiceCollection();
            services.AddSingleton(loggerFactory);
            services.AddScoped<DefaultSessionOptionProvider>();
            services.AddScoped<FakeSubjectProvider>();
            services.AddScoped<ISubjectProvider>(sp => sp.GetRequiredService<FakeSubjectProvider>());
            services.AddScoped<Server>();
            services.AddScoped<IServerSession>(sp => sp.GetRequiredService<Server>());
            services.AddScoped<ISessionOptionProvider>(sp => sp.GetRequiredService<DefaultSessionOptionProvider>());
            services.AddTransient<ServerChannel>();
            services.AddScoped<Func<IChannelWrapper>>(sp => sp.GetRequiredService<ServerChannel>);
            services.AddScoped<ServerChannelFactory>();
            services.AddScoped<IChannelFactory>(sp => sp.GetRequiredService<ServerChannelFactory>());

            return services.BuildServiceProvider();
        }

        private static void Main() => RunAsync().Wait();
    }
}
