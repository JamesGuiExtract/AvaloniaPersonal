using System;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.Views;
using Avalonia.Controls;
using Elastic.Clients.Elasticsearch;
using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace AlertManager.ViewModels
{
	public class AlertDetailsViewModel : ReactiveObject
	{
        [Reactive]
		public AlertsObject ThisAlert { get; set; }

        private Window thisWindow;

        [Reactive]
        public string AlertResolutionHistory { get; set; } = "";


        public AlertDetailsViewModel(AlertsObject alertObject, Window thisWindow)
		{
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

            EnvironmentInformationViewModel environmentViewModel = new(ThisAlert, Locator.Current.GetService<IElasticSearchLayer>() ?? new ElasticSearchService());
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
                if (ThisAlert == null || ThisAlert.Resolutions == null)
                {
                    throw new Exception(" Issue with retrieving object");
                }

                foreach (var resolution in ThisAlert.Resolutions)
                {
                    returnString += "Previous Comment: " + resolution.ResolutionComment +
                        "  Time: " + resolution.ResolutionTime + "  Type: " + resolution.ResolutionType + "\n";
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

                EventListWindowViewModel eventViewModel = new(ThisAlert.AssociatedEvents, "Associated Events");

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