using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaDashboard.Interfaces;
using AvaloniaDashboard.Services;
using AvaloniaDashboard.ViewModels;
using AvaloniaDashboard.Views;
using Splat;

namespace AvaloniaDashboard
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
