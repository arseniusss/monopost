using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using DotNetEnv; // Ensure you have this import
using Monopost.DAL.DataAccess;
using System.IO;

namespace Monopost.Web
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Specify the path to the .env file in the root directory of the solution
            var envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.env");
            Env.Load(envFilePath); // Load .env file

            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            var serviceCollection = new ServiceCollection();

            // Add DbContext with the loaded connection string
            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Add other services here

            ServiceProvider = serviceCollection.BuildServiceProvider();
            base.OnStartup(e);
        }
    }
}
