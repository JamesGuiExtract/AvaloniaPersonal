using AlertManager.Services;
using AlertManager.ViewModels;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;

namespace AlertManager.Views
{
    public partial class EnvironmentInformationView : ReactiveWindow<EnvironmentInformationViewModel>
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
