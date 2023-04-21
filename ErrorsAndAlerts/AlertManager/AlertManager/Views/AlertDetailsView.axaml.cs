using AlertManager.Services;
using Avalonia.Controls;
using ReactiveUI;
using System;
using Extract.ErrorHandling;

namespace AlertManager.Views
{
    public partial class AlertDetailsView : Window
    {

        public AlertDetailsView()
        {
            InitializeComponent();
            InitializeCloseButton();
        }

        public void CloseWindowBehind()
        {
            try 
            { 
                this.Close("Return"); 
            } 
            catch (Exception e) 
            { 
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54261")); 
            }
        }

        private void InitializeCloseButton()
        {
            closeWindow.Click += delegate
            {
                CloseWindowBehind();
            };
        }
    }
}
