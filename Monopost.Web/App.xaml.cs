using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DotNetEnv;
using Monopost.DAL.DataAccess;
using System.IO;
using Monopost.Logging;
using Serilog;
using Monopost.BLL.Services.Implementations;

namespace Monopost.Web
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            LoggerConfig.ConfigureLogging();
            Log.Debug("Application is starting...");

            var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;

            if (rootDirectory == null)
            {
                throw new InvalidOperationException("Could not find the solution's root directory.");
            }

            var envFilePath = Path.Combine(rootDirectory, ".env");
            Log.Information("Looking for .env file at: (envFilePath)", envFilePath);

            Env.Load(envFilePath);

            var serviceCollection = new ServiceCollection();
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            Log.Information($"Connected to db using URL {connectionString}");

            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            ServiceProvider = serviceCollection.BuildServiceProvider();


            DataScienceSavingPdfService pdf = new DataScienceSavingPdfService("Jar_statement_13.10.2024_193241.csv");
        
            pdf.SaveResults("FinalReport.pdf");
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Debug("Exiting...");
            Log.CloseAndFlush();
            base.OnExit(e);
        }

    }
}