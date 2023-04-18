using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.Views;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.ViewModels
{
    public class ResolveAlertsViewModel : ReactiveObject
    {
        #region fields
        //fields
        private ResolveAlertsView ThisWindow = new();
        private IAlertActionLogger? ResolutionLogger;

        [Reactive]
        public string AlertResolutionComment { get; set; } = "";
        #endregion fields
        #region setters and getters for bindings
        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject? ThisObject { get; set; } = new AlertsObject();

        [Reactive]
        public AssociatedFilesUserControl? AssociatedFileUserControl { get; set; }

        private IElasticSearchLayer elasticSearch = new ElasticSearchService();

        #endregion setters and getters for bindings
        public void RefreshScreen(AlertsObject newObject)
        {
            try
            {
                if (newObject == null)
                {
                    throw new ExtractException("ELI53867", "Issue with refreshing screen, object to refresh to is null or invalid");
                }
                ThisObject = newObject;
                SetUserControl();
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53868", "Issue refreshing screen", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
        }
        #region Constructors
        //These constructors use splat/reactive UI for dependency inversion
        public ResolveAlertsViewModel() : this(new AlertsObject(), new ResolveAlertsView(), Locator.Current.GetService<IAlertActionLogger>(), Locator.Current.GetService<IElasticSearchLayer>())
        {
        }
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay) : this(alertObjectToDisplay, new ResolveAlertsView(), Locator.Current.GetService<IAlertActionLogger>(), Locator.Current.GetService<IElasticSearchLayer>())
        {
        }
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, ResolveAlertsView thisWindow) : this(alertObjectToDisplay, thisWindow, Locator.Current.GetService<IAlertActionLogger>(), Locator.Current.GetService<IElasticSearchLayer>())
        {
        }
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, ResolveAlertsView thisWindow, IAlertActionLogger? alertResolutionLogger, IElasticSearchLayer elasticSearch)
        {
            try
            {
                if (ThisObject == null)
                {
                    throw new Exception("ThisObject is null");
                }
                RefreshScreen(alertObjectToDisplay);

                ResolutionLogger = (alertResolutionLogger == null) ? new AlertActionLogger() : alertResolutionLogger;
                this.ThisWindow = thisWindow;
                this.elasticSearch = elasticSearch;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI53870", "Issue with initializing values", e);
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
        }

        #endregion Constructors
        private void CommitResolution()
        {
            try
            {
                if(ThisObject == null)
                {
                    throw new Exception("current Alert is null");
                }

                AlertActionDto newResolution = new();
                newResolution.ActionTime = DateTime.Now;
                newResolution.ActionComment = AlertResolutionComment;
                newResolution.ActionType = TypeOfResolutionAlerts.Resolved.ToString();

                ThisObject.Resolutions.Add(newResolution);
                elasticSearch.AddAlertAction(
                    newResolution, 
                    ThisObject.AlertId
                );

                ThisWindow.Close("Refresh");
            }
            catch(Exception e)
            {
                throw e.AsExtractException("ELI54200");
            }
            
        }

        public void SetUserControl()
        {

            try
            {
                ThisObject = ThisObject == null ? new() : ThisObject;

                ResolveFilesViewModel alertsViewModel = new(ThisObject, new DBService(new FileProcessingDB()));
                AssociatedFileUserControl = new()
                {
                    DataContext = alertsViewModel
                };
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54140", "Issue displaying the the events table", e);
                ex.Log();
            }
        }
    }
}