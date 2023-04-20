using AlertManager.Services;
using Avalonia.Controls;
using System;

namespace AlertManager.Views
{
    public partial class AssociatedFilesView : Window
    {
        public AssociatedFilesView()
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
            WindowManager.AddWindow((Window)sender);
        }

    }
}
