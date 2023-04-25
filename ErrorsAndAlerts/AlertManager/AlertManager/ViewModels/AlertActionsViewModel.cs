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
    public class AlertActionsViewModel : ViewModelBase
    {
        #region Fields

        public IAlertActionsView? View { get; set; }
        private readonly IAlertActionLogger? _actionLogger;
        private readonly IElasticSearchLayer _elasticSearch;
        private readonly IDBService _dbService;

        [Reactive]
        private int ActionTypeComboIndex { get; set; } = 0;
        private const int SNOOZE_INDEX = 1;

        public DateTimeOffset? SnoozeUntilDate { get; set; } = null;

        [Reactive]
        public string AlertActionComment { get; set; } = "";

        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject? ThisAlert { get; set; }

        [Reactive]
        public AssociatedFilesViewModel? AssociatedFilesVM { get; set; }

        #endregion Fields

        #region Constructors

        public AlertActionsViewModel(
            AlertsObject alertObjectToDisplay,
            IAlertActionLogger alertActionLogger,
            IElasticSearchLayer elasticSearch,
            IDBService dBService)
        {
            try
            {
                _actionLogger = alertActionLogger;
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

        #region Methods

        private void CommitAction()
        {
            try
            {
                if (ThisAlert == null)
                {
                    throw new Exception("current Alert is null");
                }

                AlertActionDto newAction = new();
                newAction.ActionTime = DateTime.Now;
                newAction.ActionComment = AlertActionComment;

                switch (ActionTypeComboIndex)
                {
                    case 0:
                        newAction.ActionType = AlertActionType.Resolve.ToString();
                        break;
                    case 1:
                        newAction.ActionType = AlertActionType.Snooze.ToString();
                        break;
                    case 2:
                        newAction.ActionType = AlertActionType.Mute.ToString();
                        break;
                }

                if (ActionTypeComboIndex == SNOOZE_INDEX)
                {
                    if (SnoozeUntilDate == null)
                    {
                        throw new Exception("Snooze action requires a snooze until date");
                    }
                    newAction.SnoozeDuration = SnoozeUntilDate.Value.DateTime;
                }

                ThisAlert.Actions.Add(newAction);
                _elasticSearch.AddAlertAction(
                    newAction,
                    ThisAlert.AlertId
                );

                View?.Close("Refresh");
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54200");
            }
        }

        public void RefreshScreen(AlertsObject newObject)
        {
            try
            {
                ThisAlert = newObject ?? throw new ExtractException("ELI53867", "Issue with refreshing screen, object to refresh to is null or invalid");

                //Need better error handling here. Problem with database retrieving files prevents taking actions on alerts.
                AssociatedFilesVM = new(ThisAlert, _dbService);
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI53868", "Issue refreshing screen", e);
            }
        }

        #endregion Methods
    }
}