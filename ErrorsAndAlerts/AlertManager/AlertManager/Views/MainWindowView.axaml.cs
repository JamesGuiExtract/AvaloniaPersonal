using Avalonia.Controls;
using System;

namespace AlertManager.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            InitializeWindowManager();
        }

        private void InitializeWindowManager()
        {
            this.Activated += HandleWindowActivated;
        }

        private void HandleWindowActivated(object sender, EventArgs e)
        {
            Services.WindowManager.AddWindow((Window)sender);
        }


    }
}
