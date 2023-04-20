using Avalonia.Controls;
using ReactiveUI;
using System;
using Extract.ErrorHandling;

namespace AlertManager.Views
{
    public partial class ResolveAlertsView : Window
    {
        public ResolveAlertsView()
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
            catch(Exception e) 
            {
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54263"));
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
