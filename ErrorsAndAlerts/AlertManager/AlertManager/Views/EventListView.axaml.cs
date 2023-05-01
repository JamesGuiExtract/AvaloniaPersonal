using AlertManager.ViewModels;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace AlertManager.Views
{
    public partial class EventListView : ReactiveUserControl<EventListViewModel>
    {
        public EventListView()
        {
            InitializeComponent();

            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.firstButton);
            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.previousButton);
            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.nextButton);
            this.BindCommand(ViewModel, vm => vm.LoadPage, v => v.lastButton);
        }
    }
}
