using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AlertManager.Interfaces;
using AlertManager.Services;
using AlertManager.ViewModels;
using AlertManager.Views;
using Splat;
using ReactiveUI;
using Avalonia.Controls;

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
            SplatRegistrations.RegisterLazySingleton<IElasticSearchLayer, ElasticSearchService>();
            SplatRegistrations.RegisterLazySingleton<IAlertResolutionLogger, AlertResolutionLogger>();
            SplatRegistrations.SetupIOC();

            Window? mainWindow = null;
            RxApp.DefaultExceptionHandler = new GlobalErrorHandler(() => mainWindow);

            mainWindow = new MainWindowView
            {
                DataContext = new MainWindowViewModel(),
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = mainWindow;
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
