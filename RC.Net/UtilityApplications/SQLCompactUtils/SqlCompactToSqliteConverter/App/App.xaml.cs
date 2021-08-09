using Extract.Licensing;
using Microsoft.Extensions.DependencyInjection;
using MvvmGen.Events;
using System;
using System.Linq;
using System.Windows;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                ServiceProvider =
                    new ServiceCollection()
                    .AddSingleton<IEventAggregator, EventAggregator>()
                    .AddTransient<MainWindow>()
                    .AddTransient<MainViewModel>()
                    .AddTransient<DatabaseConverterViewModel>()
                    .AddTransient<IDatabaseSchemaManagerProvider, DatabaseSchemaManagerProvider>()
                    .AddTransient<IDatabaseConverter, DatabaseConverter>()
                    .AddTransient<IFileBrowserDialogService, FileBrowserDialogService>()
                    .AddTransient<IMessageDialogService, MessageDialogService>()
                    .BuildServiceProvider(true);

                var mainWindow = ServiceProvider.GetService<MainWindow>();

                var args = Environment.GetCommandLineArgs().Skip(1).ToList();
                ServiceProvider.GetService<IEventAggregator>().Publish(new ArgumentsEvent(args));

                mainWindow.Show();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51791");
                Current.Shutdown();
            }
        }
    }
}
