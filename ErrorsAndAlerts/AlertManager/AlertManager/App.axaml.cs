using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AlertManager.Interfaces;
using AlertManager.Services;
using AlertManager.ViewModels;
using AlertManager.Views;
using Splat;

namespace AlertManager
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            //Dependency injection
            SplatRegistrations.RegisterLazySingleton<IDBService, DBService>();
            SplatRegistrations.RegisterLazySingleton<ILoggingTarget, LoggingTargetElasticsearch>();
            SplatRegistrations.RegisterLazySingleton<IAlertResolutionLogger, AlertResolutionLogger>();
            SplatRegistrations.SetupIOC();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindowView
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
