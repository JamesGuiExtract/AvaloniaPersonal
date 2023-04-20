using AlertManager.Services;
using Avalonia.Controls;
using System;
using Extract.ErrorHandling;
using ReactiveUI;

namespace AlertManager.Views
{
    public partial class ConfigureAlertsView : Window
    {
        public ConfigureAlertsView()
        {
            InitializeComponent();
            InitializeButton();
            InitializeWindowManager();
        }

        public void CloseWindowBehind()
        {
            try 
            { 
                this.Close("Return"); 
            } 
            catch (Exception e) 
            { 
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54262")); 
            }
        }

        private void InitializeButton()
        {

            closeWindow.Click += delegate
            {
                CloseWindowBehind();
            };
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
