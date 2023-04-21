using AlertManager.ViewModels;
using Avalonia.ReactiveUI;

namespace AlertManager.Views
{
    public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            InitializeComponent();
        }
    }
}
