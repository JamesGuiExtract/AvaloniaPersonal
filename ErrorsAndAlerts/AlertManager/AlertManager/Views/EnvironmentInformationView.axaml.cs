using AlertManager.Services;
using Avalonia.Controls;
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

            InitializeWindowManager();
        }

        private void InitializeWindowManager()
        {
            this.Activated += HandleWindowActivated;
        }

        private void HandleWindowActivated(object sender, EventArgs e)
        {
            WindowManager.AddWindow((Window)sender);
        }

    }
}
