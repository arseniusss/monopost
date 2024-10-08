using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DotNetEnv;
using Monopost.DAL.DataAccess;
using System.IO;
using Monopost.Logging;
using Serilog;

namespace Monopost.Web
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            LoggerConfig.ConfigureLogging();
            Log.Debug("Application is starting...");

            // Simplified: Retrieve the correct root directory
            var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;

            if (rootDirectory == null)
            {
                throw new InvalidOperationException("Could not find the solution's root directory.");
            }

            var envFilePath = Path.Combine(rootDirectory, ".env");
            Log.Information($"Looking for .env file at: {envFilePath}");

            // Load environment variables from the .env file
            Env.Load(envFilePath);

            // Set up the service collection
            var serviceCollection = new ServiceCollection();
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            Log.Information($"Connected to db using URL {connectionString}");

            // Add DbContext
            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Build the service provider
            ServiceProvider = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Debug("Exiting...");
            Log.CloseAndFlush(); // Ensure to flush and close the log
            base.OnExit(e);
        }
    }
}