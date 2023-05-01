using AlertManager.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace AlertManager.Views
{
    public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            InitializeComponent();

            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.firstButton);
            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.previousButton);
            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.nextButton);
            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.lastButton);
        }
    }
}
