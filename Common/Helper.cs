using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Netty.Examples.Common
{
  public static class Helper
  {
    private const string Template =
      "[{Timestamp:HH:mm:ss} {Level:u3}] <:{SourceContext}> {Message:lj}{NewLine}{Exception}";

    public static ILoggerFactory ConfigureLogger()
    {
      // configure Serilog
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose() // set debug as minimum level for Serilog
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: Template)
        .CreateLogger();

      // create an instance of `ILoggerFactory`
      var loggerFactory = LoggerFactory.Create(BuildLoggerFactory);

      // override the DotNetty default logging factory
      InternalLoggerFactory.DefaultFactory = loggerFactory;

      return loggerFactory;
    }

    public static void BuildLoggerFactory(ILoggingBuilder builder)
    {
      // configure providers
      builder.AddSerilog(dispose: true);
      // configure minimum level
      builder.SetMinimumLevel(LogLevel.Trace);
    }
  }
}