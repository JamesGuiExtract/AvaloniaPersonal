using AlertManager.ViewModels;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace AlertManager.Views
{
    public partial class AssociatedFilesView : ReactiveUserControl<AssociatedFilesViewModel>
    {
        public AssociatedFilesView()
        {
            InitializeComponent();

            this.BindCommand(ViewModel, vm => vm.SetFileStatus, v => v.setStatusButton);
        }
    }
}
