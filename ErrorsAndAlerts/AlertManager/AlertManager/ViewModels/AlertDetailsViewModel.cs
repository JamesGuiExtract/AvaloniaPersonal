using System;
using AlertManager.Models.AllDataClasses;
using AlertManager.Views;
using Avalonia.Controls;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AlertManager.ViewModels
{
	public class AlertDetailsViewModel : ReactiveObject
	{
        [Reactive]
		public AlertsObject ThisAlert { get; set; }

        private Window thisWindow;


        //TODO testing
        public AlertDetailsViewModel(AlertsObject alertObject, Window thisWindow)
		{
            ThisAlert = alertObject;
            this.thisWindow = thisWindow;

        }

        /// <summary>
        /// Opens a new window displaying the environment details for the current alert.
        /// </summary>
        /// <returns></returns>
		public string OpenEnvironmentView()
		{
            string? result = "";

            EnvironnmentInformationViewModel environmentViewModel = new(ThisAlert);
            EnvironmentInformationView environmentWindow = new()
            {
                DataContext = (environmentViewModel)
            };

            try
            {
                result = environmentWindow.ShowDialog<string>(thisWindow).ToString();
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54145", "Issue displaying the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            result ??= "";

            return result;
        }

		public string OpenAssociatedEvents()
		{
            //will be modified in a future Jira, to see alertview table
            string? result = "";


            EventsOverallViewModel eventViewModel = new();

            EnvironmentInformationView eventWindow = new()
            {
                DataContext = (eventViewModel)
            };

            try
            {
                result = eventWindow.ShowDialog<string>(thisWindow).ToString();
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54144", "Issue displaying the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            result ??= "";

            return result;
        }

		public string ResolveWindow()
		{
            string? result = "";

            ResolveAlertsView resolveWindow = new();
            ResolveAlertsViewModel resolveViewModel = new(ThisAlert, resolveWindow);
            resolveWindow.DataContext = resolveViewModel;

            try
            {
                result = resolveWindow.ShowDialog<string>(thisWindow).ToString();
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54139", "Issue displaying the the events table", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }

            result ??= "";

            return result;
        }

    }
}