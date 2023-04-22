using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;

namespace AlertManager.ViewModels
{
    public class ResolveAlertsViewModel : ViewModelBase
    {
        #region fields
        public IResolveAlertsView? View { get; set; }
        private readonly IAlertActionLogger? _resolutionLogger;
        private readonly IElasticSearchLayer _elasticSearch;
        private readonly IDBService _dbService;
        #endregion fields
        #region setters and getters for bindings
        [Reactive]
        public string AlertResolutionComment { get; set; } = "";

        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject? ThisObject { get; set; }

        [Reactive]
        public ResolveFilesViewModel? ResolveFiles { get; set; }

        #endregion setters and getters for bindings

        public void RefreshScreen(AlertsObject newObject)
        {
            try
            {
                ThisObject = newObject ?? throw new ExtractException("ELI53867", "Issue with refreshing screen, object to refresh to is null or invalid");
                ResolveFiles = new(ThisObject, _dbService);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI53868", "Issue refreshing screen", e);
            }
        }
        #region Constructors

        public ResolveAlertsViewModel(
            AlertsObject alertObjectToDisplay,
            IAlertActionLogger alertResolutionLogger,
            IElasticSearchLayer elasticSearch,
            IDBService dBService)
        {
            try
            {
                _resolutionLogger = alertResolutionLogger;
                _elasticSearch = elasticSearch;
                _dbService = dBService;

                RefreshScreen(alertObjectToDisplay);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI53870", "Issue with initializing values", e);
            }
        }

        #endregion Constructors
        private void CommitResolution()
        {
            try
            {
                if (ThisObject == null)
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

                View?.Close("Refresh");
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54200");
            }

        }
    }
}