using AlertManager.Services;
using Avalonia.Controls;
using ReactiveUI;
using System;

namespace AlertManager.Views
{
    public partial class EnvironmentInformationView : Window
    {
        public EnvironmentInformationView()
        {
            InitializeComponent();

            closeWindow.Click += delegate
            {
                this.Close("Close");
            };
        }
    }
}
