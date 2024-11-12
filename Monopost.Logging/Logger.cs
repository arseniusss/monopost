using DotNetEnv;
using Serilog;

namespace Monopost.Logging
{
    public static class LoggerConfig
    {
        public static void ConfigureLogging()
        {
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            var parentDirectory = currentDirectory?.Parent?.Parent?.Parent?.Parent;

            if (parentDirectory == null)
            {
                throw new Exception("Path to .env file not found.");
            }

            var envFilePath = Path.Combine(parentDirectory.ToString(), ".env");

            Env.Load(envFilePath);

            var connectionString = Environment.GetEnvironmentVariable("SEQ_URL");

            if (connectionString == null)
            {
                throw new Exception($"path = {envFilePath} Seq connection string not found in environment variables.");
            }
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(connectionString)
                .CreateLogger();
        }
        public static ILogger GetLogger() => Log.Logger;
    }
}