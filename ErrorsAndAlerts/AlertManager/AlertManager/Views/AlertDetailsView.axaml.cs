using AlertManager.Services;
using Avalonia.Controls;
using ReactiveUI;
using System;
using Extract.ErrorHandling;
using AlertManager.ViewModels;
using Avalonia.ReactiveUI;

namespace AlertManager.Views
{
    public partial class AlertDetailsView : ReactiveWindow<AlertDetailsViewModel>
    {

        public AlertDetailsView()
        {
            InitializeComponent();
            InitializeCloseButton();

            this.BindCommand(ViewModel, vm => vm.OpenEnvironmentView, v => v.environmentInformationButton);
            this.BindCommand(ViewModel, vm => vm.OpenAssociatedEvents, v => v.associatedEventsButton);
            this.BindCommand(ViewModel, vm => vm.ActionsWindow, v => v.actionsWindowButton);
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
