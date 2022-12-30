using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using IndexConverterV2.ViewModels;
using IndexConverterV2.Views;
using ReactiveUI;
using System;

namespace IndexConverterV2
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                ExpressionObserver.DataValidators.RemoveAll(x => x is DataAnnotationsValidationPlugin);
                RxApp.DefaultExceptionHandler = new GlobalErrorHandler(() => desktop.MainWindow);
                desktop.MainWindow = new MainWindowView
                {
                    DataContext = new MainWindowViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }

    /// <summary>
    /// Handle uncaught exceptions by displaying a message box dialog
    /// </summary>
    public class GlobalErrorHandler : IObserver<Exception>
    {
        private Func<Window> _mainWindow;

        public GlobalErrorHandler(Func<Window> mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void OnNext(Exception value)
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
              .GetMessageBoxStandardWindow("Error", value?.Message);

            messageBoxStandardWindow.ShowDialog(_mainWindow());
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}