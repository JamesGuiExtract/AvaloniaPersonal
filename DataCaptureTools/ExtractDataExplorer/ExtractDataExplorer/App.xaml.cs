using Extract.ErrorHandling;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.ReactiveUI;
using Extract.Utilities.WPF;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.Services;
using ExtractDataExplorer.ViewModels;
using ExtractDataExplorer.Views;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ExtractDataExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        readonly string _appStateFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ExtractSystems");

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // BEGIN Composition Root
                MainWindow? mainWindow = null;

                // Handle exceptions thrown from observable subscriptions, e.g.
                RxApp.DefaultExceptionHandler = new GlobalErrorHandler("ELI53913", () => mainWindow);

                mainWindow = new();
                MessageDialogService messageDialogService = new(window => window.Owner = mainWindow);
                ThemingService themingService = new(mainWindow);

                // Register dependencies to simplify creating the object graph
                SplatRegistrations.Register<MainWindowViewModelFactory>();
                SplatRegistrations.RegisterLazySingleton<IAFUtilityFactory, AFUtilityFactory>();
                SplatRegistrations.RegisterLazySingleton<IFileBrowserDialogService, FileBrowserDialogService>();
                SplatRegistrations.RegisterConstant<IThemingService>(themingService);
                SplatRegistrations.RegisterConstant<IMessageDialogService>(messageDialogService);
                SplatRegistrations.RegisterLazySingleton<IAttributeTreeService, AttributeTreeService>();
                SplatRegistrations.SetupIOC();

                MainWindowViewModelFactory factory = Locator.Current.GetService<MainWindowViewModelFactory>()
                    .AssertNotNull("ELI53929", "MainWindowViewModelFactory not found!");

                // END Composition Root

                AutoSuspendHelper suspension = new(this);

                // This function is run when there is no saved application state
                RxApp.SuspensionHost.CreateNewAppState = () => factory.CreateViewModel();

                string appStateFile = Path.Combine(
                    Directory.CreateDirectory(_appStateFolder).FullName,
                    nameof(ExtractDataExplorer) + ".appstate.json");

                // Setup the functions used to build/save the view model via the model class
                RxApp.SuspensionHost.SetupDefaultSuspendResume(
                    new JsonSuspensionDriver<MainWindowViewModel, MainWindowModel>(appStateFile, factory));

                // Load the saved view model state
                var state = RxApp.SuspensionHost.GetAppState<MainWindowViewModel>();

                mainWindow.DataContext = state;
                mainWindow.Closing += MainWindow_Closing;

                mainWindow.Show();

                OnStartupAsync(state);
            }
            catch (Exception ex)
            {
                Extract.ExtractException.Display("ELI53914", ex);
                Application.Current.Shutdown();
            }
        }

        private static async void OnStartupAsync(MainWindowViewModel mainWindowViewModel)
        {
            try
            {
                // Parse the parameters and pass on to the main window
                var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                var result = CommandLineHandler.HandleCommandLine<Options, Options>(args, opts => Result.CreateSuccess(opts));

                if (result.ResultType == ResultType.Failure)
                {
                    throw result.Exception;
                }

                await mainWindowViewModel.UpdateFromCommandLineArgsAsync(result.SuccessValue).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Extract.ExtractException.Display("ELI53930", ex);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Locator.Current.GetService<IAttributeTreeService>() is IAttributeTreeService disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
