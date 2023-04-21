using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.Views;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.ViewModels
{
    public class ResolveAlertsViewModel : ReactiveObject
    {
        #region fields
        private ResolveAlertsView ThisWindow = new();
        private readonly IAlertActionLogger? _resolutionLogger;
        private readonly IElasticSearchLayer _elasticSearch;
        #endregion fields
        #region setters and getters for bindings
        [Reactive]
        public string AlertResolutionComment { get; set; } = "";

        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject? ThisObject { get; set; } = new AlertsObject();

        [Reactive]
        public AssociatedFilesUserControl? AssociatedFileUserControl { get; set; }

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

        public ResolveAlertsViewModel(
            AlertsObject alertObjectToDisplay,
            ResolveAlertsView thisWindow,
            IAlertActionLogger alertResolutionLogger,
            IElasticSearchLayer elasticSearch)
        {
            try
            {
                if (ThisObject == null)
                {
                    throw new Exception("ThisObject is null");
                }
                RefreshScreen(alertObjectToDisplay);

                _resolutionLogger = alertResolutionLogger;
                ThisWindow = thisWindow;
                _elasticSearch = elasticSearch;
            }
            catch (Exception e)
            {
                throw new ExtractException ("ELI53870", "Issue with initializing values", e);
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
                newResolution.ActionType = AlertStatus.Resolved.ToString();

                ThisObject.Actions.Add(newResolution);
                _elasticSearch.AddAlertAction(
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