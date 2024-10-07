using Serilog;

namespace Monopost.Logging
{
    public static class LoggerConfig
    {
        public static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
        }
        public static ILogger GetLogger() => Log.Logger;
    }
}