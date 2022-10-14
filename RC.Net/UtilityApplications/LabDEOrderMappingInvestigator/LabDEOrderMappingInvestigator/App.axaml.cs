using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LabDEOrderMappingInvestigator.Services;
using LabDEOrderMappingInvestigator.ViewModels;
using LabDEOrderMappingInvestigator.Views;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using UCLID_AFUTILSLib;

namespace LabDEOrderMappingInvestigator
{
    public partial class App : Application
    {
        readonly string _appStateFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ExtractSystems");

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            SplatRegistrations.RegisterLazySingleton<IAFUtility, AFUtilityClass>();
            SplatRegistrations.RegisterLazySingleton<IAnalysisService, AnalysisService>();
            SplatRegistrations.RegisterLazySingleton<ILabOrderService, LabOrderService>();
            SplatRegistrations.RegisterLazySingleton<IFileBrowserDialogService, FileBrowserDialogService>();
            SplatRegistrations.Register<MainWindowViewModel>();
            SplatRegistrations.SetupIOC();

            // Create the AutoSuspendHelper
            (ApplicationLifetime is not null).Assert("ApplicationLifetime is null!");

#pragma warning disable CA2000 // Dispose objects before losing scope
            AutoSuspendHelper suspension = new(ApplicationLifetime); // This needs to be kept undisposed until the application closes in order to save the state
#pragma warning restore CA2000 // Dispose objects before losing scope

            RxApp.SuspensionHost.CreateNewAppState = () =>
            {
                var viewModel = Locator.Current.GetService<MainWindowViewModel>();
                (viewModel is not null).Assert("Main view model is null!");
                return viewModel;
            };

            string appStateFile = Path.Combine(
                Directory.CreateDirectory(_appStateFolder).FullName,
                nameof(LabDEOrderMappingInvestigator) + ".appstate.json");
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new JsonSuspensionDriver(appStateFile));

            suspension.OnFrameworkInitializationCompleted();

            // Load the saved view model state
            var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = state };
                RxApp.DefaultExceptionHandler = new GlobalErrorHandler(desktop.MainWindow);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }

    public class GlobalErrorHandler : IObserver<Exception>
    {
        readonly Window _mainWindow;

        public GlobalErrorHandler(Window mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void OnNext(Exception value)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
              .GetMessageBoxStandardWindow("Error", value?.Message);

            messageBoxStandardWindow.ShowDialog(_mainWindow);
        }

        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }
}
