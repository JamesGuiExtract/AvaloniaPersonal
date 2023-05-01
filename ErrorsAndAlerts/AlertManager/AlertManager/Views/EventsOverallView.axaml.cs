using AlertManager.Services;
using AlertManager.ViewModels;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Extract.ErrorHandling;
using ReactiveUI;
using System;

namespace AlertManager.Views
{
    public partial class EventsOverallView : ReactiveWindow<EventsOverallViewModel>
    {
        public EventsOverallView()
        {
            InitializeComponent();
            InitializeCloseButton();

            this.BindCommand(ViewModel, vm => vm.OpenEnvironmentView, v => v.OpenEnvironmentViewButton);
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
    }
}
