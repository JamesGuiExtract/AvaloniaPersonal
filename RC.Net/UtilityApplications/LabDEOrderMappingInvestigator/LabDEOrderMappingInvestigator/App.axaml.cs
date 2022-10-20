using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LabDEOrderMappingInvestigator.Models;
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
            // BEGIN Composition Root
            // Register dependencies to simplify creating the object graph
            SplatRegistrations.RegisterLazySingleton<IAFUtility, AFUtilityClass>();
            SplatRegistrations.RegisterLazySingleton<IAnalysisService, AnalysisService>();
            SplatRegistrations.RegisterLazySingleton<ICustomerDatabaseService, SqliteCustomerDatabaseService>();
            SplatRegistrations.RegisterLazySingleton<IFileBrowserDialogService, FileBrowserDialogService>();
            SplatRegistrations.RegisterLazySingleton<ILabOrderDatabaseService, SqliteLabOrderDatabaseService>();
            SplatRegistrations.RegisterLazySingleton<ILabOrderFileService, VOALabOrderFileService>();
            SplatRegistrations.RegisterLazySingleton<ILabTestMappingSuggestionService, LuceneLabTestMappingSuggestionService>();
            SplatRegistrations.RegisterLazySingleton<ILabTestMatchListViewModelFactory, LabTestMatchListViewModelFactory>();
            SplatRegistrations.RegisterLazySingleton<ILabTestMatchViewModelFactory, LabTestMatchViewModelFactory>();
            SplatRegistrations.Register<MainWindowViewModelFactory>();
            SplatRegistrations.SetupIOC();

            var factory = Locator.Current.GetService<MainWindowViewModelFactory>();
            (factory is not null).Assert("Main view model factory is null!");

            // END Composition Root

            // Create the AutoSuspendHelper
            (ApplicationLifetime is not null).Assert("ApplicationLifetime is null!");

#pragma warning disable CA2000 // Dispose objects before losing scope
            AutoSuspendHelper suspension = new(ApplicationLifetime); // This needs to be kept undisposed until the application closes in order to save the state
#pragma warning restore CA2000 // Dispose objects before losing scope

            // This function is run when there is no saved application state
            RxApp.SuspensionHost.CreateNewAppState = () => factory.CreateViewModel();

            string appStateFile = Path.Combine(
                Directory.CreateDirectory(_appStateFolder).FullName,
                nameof(LabDEOrderMappingInvestigator) + ".appstate.json");

            // Setup the functions used to build/save the view model via the model class
            RxApp.SuspensionHost.SetupDefaultSuspendResume(
                new JsonSuspensionDriver<MainWindowViewModel, MainWindowModel>(appStateFile, factory));

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

    /// <summary>
    /// Handle uncaught exceptions by displaying a message box dialog
    /// </summary>
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
