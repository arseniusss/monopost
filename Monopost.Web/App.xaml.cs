using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DotNetEnv;
using Monopost.DAL.DataAccess;
using System.IO;

namespace Monopost.Web
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\.env");
            Env.Load(envFilePath);

            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            ServiceProvider = serviceCollection.BuildServiceProvider();
            base.OnStartup(e);
        }
    }
}