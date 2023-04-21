using AlertManager.Services;
using Avalonia.Controls;
using System;

namespace AlertManager.Views
{
    public partial class EventListWindowView : Window
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
