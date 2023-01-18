using Avalonia.Controls;
using Avalonia.Interactivity;
using IndexConverterV2.Models;
using IndexConverterV2.ViewModels;
using System;
using System.Threading.Tasks;

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