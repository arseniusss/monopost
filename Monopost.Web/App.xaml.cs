﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using DotNetEnv;
using Monopost.DAL.DataAccess;
using Monopost.Logging;
using Serilog;
<<<<<<< HEAD
using Monopost.BLL.Services.Implementations;
=======
using Monopost.Web.Views;
>>>>>>> test_UI

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

            ServiceProvider = serviceCollection.BuildServiceProvider();

<<<<<<< HEAD
            string outputDirectory = Environment.GetEnvironmentVariable("OUTPUT_DIRECTORY");
            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Monopost");
                Directory.CreateDirectory(outputDirectory);
            }

            DataScienceSavingPdfService pdf = new DataScienceSavingPdfService("Jar_statement_13.10.2024_193241.csv");
            pdf.SaveResults("FinalReport.pdf", outputDirectory);
=======
            var mainWindow = new MainWindow();
            mainWindow.Show();


>>>>>>> test_UI
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

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Debug("Exiting...");
            Log.CloseAndFlush();
            base.OnExit(e);
        }

    }
}
