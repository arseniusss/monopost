using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using DotNetEnv;
using Monopost.DAL.DataAccess;
using Monopost.Logging;
using Serilog;
using Monopost.BLL.Services.Implementations;
using Monopost.DAL.Repositories.Interfaces;
using Monopost.DAL.Repositories.Implementations;
using Monopost.Web.Views;

namespace Monopost.Web
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            LoggerConfig.ConfigureLogging();
            Log.Debug("Application is starting...");

            var rootDirectory = GetRootDirectory();
            var envFilePath = Path.Combine(rootDirectory, ".env");
            Log.Information("Looking for .env file at: {EnvFilePath}", envFilePath);

            Env.Load(envFilePath);

            var serviceCollection = new ServiceCollection();
            ConfigureDatabase(serviceCollection);  
            ConfigureServices(serviceCollection);   

     
            ServiceProvider = serviceCollection.BuildServiceProvider();

            string outputDirectory = Environment.GetEnvironmentVariable("OUTPUT_DIRECTORY");
            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Monopost");
                Directory.CreateDirectory(outputDirectory);
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private static string GetRootDirectory()
        {
            var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
            if (rootDirectory == null)
            {
                throw new InvalidOperationException("Could not find the solution's root directory.");
            }
            return rootDirectory;
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            Log.Information($"Connected to db using URL {connectionString}");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITemplateRepository, TemplateRepository>();
            services.AddSingleton<ITemplateFileRepository, TemplateFileRepository>();
            services.AddSingleton<ICredentialRepository, CredentialRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IPostRepository, PostRepository>();
            services.AddSingleton<IPostMediaRepository, PostMediaRepository>();
    
            services.AddSingleton<MainWindow>();
            services.AddSingleton<LoginPage>();
            services.AddSingleton<MainPage>();
            services.AddSingleton<ProfilePage>();
            services.AddSingleton<MonobankPage>();
            services.AddSingleton<PostingPage>();
            services.AddSingleton<AdminPage>();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            Log.Debug("Exiting...");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
