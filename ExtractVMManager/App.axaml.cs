using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ExtractVMManager.ViewModels;
using ExtractVMManager.Views;
using ExtractVMManager.Services;
using Extract.ErrorHandling;

namespace ExtractVMManager
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                base.OnFrameworkInitializationCompleted();

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var db = new Database();

                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(db),
                    };
                }
            }
            catch (System.Exception ex)
            {
                ex.AsExtractException("ELI53666").Display();
            }
        }
    }
}
