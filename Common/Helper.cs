using DotNetty.Common.Internal.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Netty.Examples.Common
{
  public static class Helper
  {
    public static void SetConsoleLogger() => InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => true, false));
  }
}