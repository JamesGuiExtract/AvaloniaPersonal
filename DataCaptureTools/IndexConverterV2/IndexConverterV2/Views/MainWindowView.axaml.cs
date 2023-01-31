using Avalonia.Controls;
using IndexConverterV2.ViewModels;
using IndexConverterV2.Services;

namespace IndexConverterV2.Views
{
    public interface IView { }

    public partial class MainWindowView : Window, IView
    {
        public MainWindowView()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this, new IndexConverterDialogService());
        }
    }
}