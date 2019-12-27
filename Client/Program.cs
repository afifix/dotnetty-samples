using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netty.Examples.Common;
using System;
using System.Threading.Tasks;

namespace Netty.Examples.Client
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
        var client = scopeServiceProvider.GetRequiredService<ISession>();

        try
        {
          client.Connected += (o, e) => logger.LogInformation("client connected.");
          client.Closed += (o, e) => logger.LogInformation("client closed.");
          client.Ponged += (o, e) => logger.LogInformation($"pong received ${e.Latency}");
          await client.RunAsync();

          Console.ReadKey();
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error");
        }
        finally
        {
          await client.CloseAsync();
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
      services.AddScoped<ISessionOptionProvider>(sp => sp.GetRequiredService<DefaultSessionOptionProvider>());
      services.AddScoped<Client>();
      services.AddScoped<ISession>(sp => sp.GetRequiredService<Client>());
      //services.AddScoped<PongProcessor>();
      return services.BuildServiceProvider();
    }

    private static void Main() => RunAsync().Wait();
  }
}