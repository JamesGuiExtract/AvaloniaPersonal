using AlertManager.Services;
using AlertManager.ViewModels;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using System;

namespace AlertManager.Views
{
    public partial class EventListWindowView : ReactiveWindow<EventListWindowViewModel>
    {
        public EventListWindowView()
        {
            InitializeComponent();

            closeWindow.Click += delegate
            {
                this.Close("Close");
            };
        }
    }
}
