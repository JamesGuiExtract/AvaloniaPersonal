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
    }
}
