using AlertManager.ViewModels;
using Avalonia.ReactiveUI;

namespace AlertManager.Views
{
    public partial class EventListView : ReactiveUserControl<EventListViewModel>
    {
        public EventListView()
        {
            InitializeComponent();
        }
    }
}
