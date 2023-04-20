using AlertManager.Services;
using Avalonia.Controls;
using Extract.ErrorHandling;
using ReactiveUI;
using System;

namespace AlertManager.Views
{
    public partial class EventsOverallView : Window
    {
        public EventsOverallView()
        {
            InitializeComponent();
            InitializeCloseButton();
            InitializeWindowManager();
        }

        public void CloseWindowBehind()
        {
            try
            { 
                this.Close("Return"); 
            }
            catch(Exception e) 
            { 
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54260")); 
            }
        }

        private void InitializeCloseButton()
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
