using Avalonia.Controls;
using ReactiveUI;
using System;
using Extract.ErrorHandling;
using AlertManager.ViewModels;
using Avalonia.ReactiveUI;
using AlertManager.Interfaces;

namespace AlertManager.Views
{
    public partial class ResolveAlertsView : ReactiveWindow<ResolveAlertsViewModel>, IResolveAlertsView
    {
        public ResolveAlertsView()
        {
            InitializeComponent();
            InitializeCloseButton();

            this.WhenActivated(disposables =>
            {
                if (ViewModel is ResolveAlertsViewModel vm)
                {
                    vm.View = this;
                }
            });
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
