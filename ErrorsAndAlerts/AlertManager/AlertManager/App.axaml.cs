using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AlertManager.Interfaces;
using AlertManager.Services;
using AlertManager.ViewModels;
using AlertManager.Views;
using Splat;
using UCLID_FILEPROCESSINGLib;
using ReactiveUI;
using Avalonia.Controls;
using Avalonia.VisualTree;

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
            RxApp.DefaultExceptionHandler = new GlobalErrorHandler(() => 
                (Avalonia.Input.FocusManager.Instance?.Scope as IVisual).FindAncestorOfType<Window>(true));

            //Dependency injection
            SplatRegistrations.RegisterLazySingleton<IDBService, DBService>();
            SplatRegistrations.RegisterLazySingleton<IElasticSearchLayer, ElasticSearchService>();
            SplatRegistrations.RegisterLazySingleton<IAlertActionLogger, AlertActionLogger>();
            SplatRegistrations.RegisterLazySingleton<EventsOverallViewModelFactory>();
            SplatRegistrations.Register<MainWindowViewModel>();
            SplatRegistrations.Register<ConfigureAlertsViewModel>();

            SplatRegistrations.SetupIOC();

            Window? mainWindow = new MainWindowView
            {
                DataContext = Locator.Current.GetService<MainWindowViewModel>(),
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
