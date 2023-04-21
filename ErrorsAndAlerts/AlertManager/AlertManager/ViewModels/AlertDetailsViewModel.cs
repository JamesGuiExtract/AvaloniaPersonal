using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Views;
using Avalonia.Controls;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;

namespace AlertManager.ViewModels
{
    public class AlertDetailsViewModel : ReactiveObject
	{
        [Reactive]
		public AlertsObject ThisAlert { get; set; }

        private readonly EventsOverallViewModelFactory _eventsOverallViewModelFactory;
        private Window thisWindow;
        private IElasticSearchLayer _elasticService;
        private readonly IAlertActionLogger _alertResolutionLogger;

        [Reactive]
        public string AlertResolutionHistory { get; set; } = "";


        public AlertDetailsViewModel(
            EventsOverallViewModelFactory eventsOverallViewModelFactory,
            IElasticSearchLayer elastic,
            IAlertActionLogger alertResolutionLogger,
            AlertsObject alertObject,
            Window thisWindow)
		{
            _eventsOverallViewModelFactory = eventsOverallViewModelFactory;
            _elasticService = elastic;
            _alertResolutionLogger = alertResolutionLogger;

            ThisAlert = alertObject;
            this.thisWindow = thisWindow;
            AlertResolutionHistory = AlertHistoryToString();
        }

        /// <summary>
        /// Opens a new window displaying the environment details for the current alert.
        /// </summary>
        /// <returns></returns>
		public string OpenEnvironmentView()
		{
            string? result = "";

            EnvironmentInformationViewModel environmentViewModel = new(ThisAlert, _elasticService);
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

        private string AlertHistoryToString()
        {
            string returnString = "";
            try
            {
                if (ThisAlert == null || ThisAlert.Actions == null)
                {
                    throw new Exception(" Issue with retrieving object");
                }

                foreach (var resolution in ThisAlert.Actions)
                {
                    returnString += "Previous Comment: " + resolution.ActionComment +
                        "  Time: " + resolution.ActionTime + "  Type: " + resolution.ActionType + "\n";
                }
            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54223"));
                return "";
            }

            return returnString;
        }

        public string OpenAssociatedEvents()
		{
            string? result = "";

            try
            {
                if(ThisAlert.AssociatedEvents == null)
                {
                    throw new ExtractException("ELI54134", "Issue with Alert Object");
                }

                EventListWindowViewModel eventViewModel = new(_eventsOverallViewModelFactory, ThisAlert.AssociatedEvents, "Associated Events");

                EventListWindowView eventWindow = new()
                {
                    DataContext = (eventViewModel)
                };

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
            ResolveAlertsViewModel resolveViewModel = new(ThisAlert, resolveWindow, _alertResolutionLogger, _elasticService);
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