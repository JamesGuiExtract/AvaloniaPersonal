using Avalonia.Controls;
using ReactiveUI;
using System;
using Extract.ErrorHandling;
using AlertManager.ViewModels;
using Avalonia.ReactiveUI;
using AlertManager.Interfaces;

namespace AlertManager.Views
{
    public partial class AlertActionsView : ReactiveWindow<AlertActionsViewModel>, IAlertActionsView
    {
        public AlertActionsView()
        {
            InitializeComponent();
            InitializeCloseButton();

            this.WhenActivated(disposables =>
            {
                if (ViewModel is AlertActionsViewModel vm)
                {
                    vm.View = this;
                }
            });

            this.BindCommand(ViewModel, vm => vm.CommitAction, v => v.commitButton);
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
